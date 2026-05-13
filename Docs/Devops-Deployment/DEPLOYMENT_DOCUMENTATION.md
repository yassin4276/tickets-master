# Ticketing Platform Deployment Documentation

This document describes the full deployment lifecycle of the Ticketing Platform backend.

---

## 1. Deployment Goals

The goal of this project is to simulate a real-world DevOps deployment cycle for a backend application.

The deployment covers:

- Containerizing a .NET API
- Running PostgreSQL with persistent storage
- Provisioning AWS infrastructure using Terraform
- Publishing Docker images to GHCR
- Deploying automatically to AWS EC2 using GitHub Actions
- Running Nginx as a reverse proxy
- Validating deployment with health checks
- Running the same workload on Kubernetes locally using Minikube

---

## 2. Docker Image Build

The backend uses a multi-stage Dockerfile.

### Build Stage

- Uses .NET SDK image
- Restores dependencies
- Builds and publishes the API

### Runtime Stage

- Uses ASP.NET runtime image
- Copies published output
- Exposes port `8080`
- Runs the API DLL

Build command:

```bash
docker build -t ticketing-api .
```

Production image:

```text
ghcr.io/yassin4276/ticketing-api:latest
```

---

## 3. Docker Compose Production Deployment

Production compose services:

- `ticketing-postgres`
- `ticketing-api`
- `ticketing-nginx`

Run command:

```bash
docker compose --env-file .env.prod -f docker-compose.prod.yml up -d
```

Pull latest images:

```bash
docker compose --env-file .env.prod -f docker-compose.prod.yml pull
```

Check containers:

```bash
docker ps
```

Check logs:

```bash
docker logs ticketing-api --tail 100
```

Health check:

```bash
curl -f http://localhost/health
```

---

## 4. Environment Variables

The production compose file reads values from `.env.prod`.

Important values:

```env
API_IMAGE=ghcr.io/yassin4276/ticketing-api:latest
POSTGRES_USER=postgres
POSTGRES_PASSWORD=CHANGE_ME
POSTGRES_DB=ticketing_db
DB_CONNECTION_STRING=Host=ticketing-postgres;Port=5432;Database=ticketing_db;Username=postgres;Password=CHANGE_ME
JWT_SECRET_KEY=CHANGE_ME
PUBLIC_BASE_URL=http://EC2_PUBLIC_IP
```

Important note:

`.env.prod` must not be committed to Git.

---

## 5. AWS Infrastructure with Terraform

Terraform directory:

```text
infra/aws/ec2
```

Main resources:

- AWS provider
- Ubuntu AMI data source
- VPC
- Public subnet
- Internet gateway
- Route table
- Route to Internet gateway (`0.0.0.0/0`)
- Route table association
- Security group
- SSH key pair
- EC2 instance
- User data script

### Future AWS (not in this module)

Anything beyond **VPC + subnet + Internet Gateway + route table + security group + EC2** (for example Route 53, ALB/NLB, Auto Scaling, RDS, S3, optional managed cache, Lambda, WAF, or a full CloudWatch strategy) is **out of scope** for `infra/aws/ec2` and is treated as a **future** learning or production step.

Initialize Terraform:

```bash
terraform init
```

Preview changes:

```bash
terraform plan
```

Apply infrastructure:

```bash
terraform apply
```

Outputs include:

- Public IP
- Public DNS
- SSH command
- App URL

---

## 6. EC2 Bootstrap with User Data

The EC2 user data script installs:

- Docker Engine
- Docker CLI
- Docker Compose plugin
- Git
- Required Linux packages

It also creates:

```text
/opt/ticketing
```

This directory stores:

- `docker-compose.prod.yml`
- `.env.prod`
- `nginx/nginx.conf`

---

## 7. Manual EC2 Deployment

Copy files to EC2:

```bash
scp -i ~/.ssh/ticketing-aws docker-compose.prod.yml ubuntu@EC2_PUBLIC_IP:/opt/ticketing/
ssh -i ~/.ssh/ticketing-aws ubuntu@EC2_PUBLIC_IP "mkdir -p /opt/ticketing/nginx"
scp -i ~/.ssh/ticketing-aws nginx/nginx.conf ubuntu@EC2_PUBLIC_IP:/opt/ticketing/nginx/nginx.conf
```

SSH into EC2:

```bash
ssh -i ~/.ssh/ticketing-aws ubuntu@EC2_PUBLIC_IP
```

Run deployment:

```bash
cd /opt/ticketing
docker compose --env-file .env.prod -f docker-compose.prod.yml pull
docker compose --env-file .env.prod -f docker-compose.prod.yml up -d
```

Test:

```bash
curl -f http://localhost/health
curl -f http://EC2_PUBLIC_IP/health
```

---

## 8. CI Workflow

File:

```text
.github/workflows/ci.yml
```

Triggers:

- Pull request to `dev`
- Push to `dev`

Main steps:

1. Checkout code
2. Setup .NET
3. Restore dependencies
4. Build solution
5. Run tests
6. Build Docker image
7. Login to GHCR
8. Push image to GHCR

Image tags:

```text
ghcr.io/yassin4276/ticketing-api:latest
ghcr.io/yassin4276/ticketing-api:${{ github.sha }}
```

---

## 9. CD Workflow

File:

```text
.github/workflows/cd-dev.yml
```

Trigger:

- Runs after the `CI` workflow completes successfully on `dev`

Main steps:

1. SSH into EC2
2. Change directory to `/opt/ticketing`
3. Pull latest image
4. Restart containers
5. Prune unused images
6. Run health check through Nginx

Health check:

```bash
curl -f http://localhost/health
```

Required GitHub Secrets:

```text
EC2_HOST
EC2_USER
EC2_SSH_KEY
```

---

## 10. Nginx Reverse Proxy

Nginx config location:

```text
nginx/nginx.conf
```

Nginx listens on port `80` and forwards requests to:

```text
ticketing-api:8080
```

Main config:

```nginx
location / {
    proxy_pass http://ticketing-api:8080;
    proxy_http_version 1.1;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
}
```

After Nginx is configured, public access to port `8080` is removed from the AWS security group.

Expected result:

```text
http://EC2_PUBLIC_IP/health       works
http://EC2_PUBLIC_IP:8080/health  blocked
```

---

## 11. Kubernetes Local Deployment

Start Minikube:

```bash
minikube start --driver=docker
```

Check cluster:

```bash
minikube status
kubectl get nodes
```

Apply base objects:

```bash
kubectl apply -f k8s/namespace.yml
kubectl apply -f k8s/configmap.yml
kubectl apply -f k8s/secret.yml
```

Apply PostgreSQL:

```bash
kubectl apply -f k8s/postgres-pvc.yml
kubectl apply -f k8s/postgres-deployment.yml
kubectl apply -f k8s/postgres-service.yml
```

Apply API:

```bash
kubectl apply -f k8s/api-deployment.yml
kubectl apply -f k8s/api-service.yml
```

Check resources:

```bash
kubectl get pods -n ticketing
kubectl get svc -n ticketing
kubectl get pvc -n ticketing
```

---

## 12. Kubernetes Ingress

Enable Ingress addon:

```bash
minikube addons enable ingress
```

Apply Ingress:

```bash
kubectl apply -f k8s/ingress.yml
```

Get Minikube IP:

```bash
minikube ip
```

Add to `/etc/hosts`:

```text
MINIKUBE_IP ticketing.local
```

Test:

```bash
curl -i http://ticketing.local/health
```

---

## 13. Kubernetes Rollout and Rollback

Scale API to 2 replicas:

```yaml
replicas: 2
```

Rolling strategy:

```yaml
strategy:
  type: RollingUpdate
  rollingUpdate:
    maxUnavailable: 0
    maxSurge: 1
```

Restart deployment:

```bash
kubectl rollout restart deployment/ticketing-api -n ticketing
kubectl rollout status deployment/ticketing-api -n ticketing
```

Simulate failed deployment:

```bash
kubectl set image deployment/ticketing-api \
  ticketing-api=ghcr.io/yassin4276/ticketing-api:bad-version \
  -n ticketing
```

Check failed Pod:

```bash
kubectl get pods -n ticketing -l app=ticketing-api
```

Rollback:

```bash
kubectl rollout undo deployment/ticketing-api -n ticketing
kubectl rollout status deployment/ticketing-api -n ticketing
```

Validate:

```bash
curl -i http://ticketing.local/health
```

---

## 14. Security Notes

Current learning setup includes temporary security decisions.

Should be improved for production:

- Avoid public SSH access from `0.0.0.0/0`
- Use AWS SSM or dynamic GitHub Actions IP rules
- Rotate exposed secrets
- Use HTTPS
- Use RDS instead of PostgreSQL container
- Use S3 for uploaded files
- Use Terraform remote state
- Avoid using `latest` image tag for production deployments

---

## 15. Final Deployment Summary

Completed deployment lifecycle:

```text
Code
 ↓
GitHub Actions CI
 ↓
Docker image build
 ↓
GHCR push
 ↓
Terraform-provisioned AWS EC2
 ↓
GitHub Actions CD
 ↓
Docker Compose deployment
 ↓
Nginx reverse proxy
 ↓
Health check validation
```

Kubernetes learning deployment:

```text
Minikube
 ↓
Namespace / ConfigMap / Secret
 ↓
PostgreSQL + PVC
 ↓
API Deployment + Service
 ↓
Ingress
 ↓
Rolling update and rollback
```
