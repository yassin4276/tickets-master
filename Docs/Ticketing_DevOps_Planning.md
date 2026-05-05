# Ticketing Platform - DevOps Planning

## 1. Overview

This document defines the DevOps plan for the Ticketing Platform.

The goal of this phase is to decide how the project will be developed, containerized, tested, deployed, monitored, and prepared for a production-like environment.

The initial DevOps scope is intentionally simple, but it is designed to grow step by step.

---

## 2. DevOps Goals

The project DevOps plan aims to achieve the following:

- Run the backend and database using Docker.
- Use separate environments for development and testing.
- Use GitHub Actions for CI/CD.
- Deploy the application after merging to the `dev` branch.
- Use AWS services in a realistic production-like scenario.
- Use Kubernetes later during the deployment stage.
- Manage secrets securely.
- Use structured logging inside the backend.

---

## 3. Containers Plan

The initial project will use two main containers:

```text
1. Backend API Container
2. PostgreSQL Database Container
```

### Backend API Container

The backend container will run the ASP.NET Core API.

Responsibilities:

- Serve API endpoints.
- Handle authentication and authorization.
- Manage events, sessions, seats, ticket types, bookings, payments, and notifications.
- Connect to PostgreSQL.
- Produce structured logs using Serilog.

### PostgreSQL Database Container

The database container will run PostgreSQL.

Responsibilities:

- Store application data.
- Store users, events, sessions, seats, ticket types, bookings, payments, and notifications.
- Support transactions and constraints required for the booking system.

### Future Container

Redis can be added later when the project reaches the real-time and caching stage.

```text
3. Redis Container - planned later
```

Redis may be used later for:

- Seat availability caching.
- Temporary reservation locks.
- SignalR scale-out support.

---

## 4. Environments

The project will start with two environments:

```text
Development
Testing
```

---

## 4.1 Development Environment

The development environment is used locally during daily development.

### Purpose

- Build and run the project locally.
- Test features before opening pull requests.
- Use local environment variables and local Docker Compose.

### Expected Setup

```text
ASP.NET Core Backend
PostgreSQL Container
Docker Compose
.env file
appsettings.Development.json
```

### Notes

- The `.env` file must not be committed to Git.
- Local secrets can also be managed using `.NET user-secrets`.

---

## 4.2 Testing Environment

The testing environment is used after merging into the `dev` branch.

### Purpose

- Deploy the latest stable development version.
- Test the application in an environment closer to production.
- Validate Docker image builds and deployment flow.

### Expected Setup

```text
EC2 Server or Kubernetes later
Backend Container
PostgreSQL Container
.env file on the server or Kubernetes Secrets
```

---

## 5. Git Branch Strategy

The project will use the following branch strategy:

```text
feature/*  -> dev
dev        -> testing deployment
main       -> production later
```

### Branches

| Branch | Purpose |
|---|---|
| feature/* | Used for developing new features |
| dev | Main integration branch for testing |
| main | Stable production branch, planned later |

### Example Feature Branches

```text
feature/auth
feature/events
feature/bookings
feature/payments
feature/notifications
```

---

## 6. Conventional Commits

The project will follow Conventional Commits to keep Git history clean and readable.

### Examples

```text
feat: add booking endpoint
fix: resolve seat concurrency issue
docs: add API design document
refactor: improve booking service structure
test: add booking validation tests
chore: update docker compose
```

---

## 7. CI/CD Plan Using GitHub Actions

GitHub Actions will be used for CI/CD.

The pipeline will run in two main cases:

1. Pull request to `dev`
2. Merge to `dev`

---

## 7.1 Pull Request Pipeline

When a pull request is opened against the `dev` branch, the pipeline should validate the code.

### Trigger

```text
Pull Request -> dev
```

### Pipeline Steps

```text
1. Checkout code
2. Restore dependencies
3. Build solution
4. Run tests
```

### Purpose

This ensures that broken code is not merged into the `dev` branch.

---

## 7.2 Merge to Dev Pipeline

When a pull request is merged into `dev`, the pipeline should build and deploy the application to the testing environment.

### Trigger

```text
Push / Merge -> dev
```

### Pipeline Steps

```text
1. Checkout code
2. Restore dependencies
3. Build solution
4. Run tests
5. Build Docker image
6. Push Docker image to container registry
7. Deploy to testing environment
```

### Deployment Target

The first deployment target can be:

```text
EC2 + Docker Compose
```

Later, the deployment target can become:

```text
Kubernetes
```

---

## 8. Container Registry

A container registry will be used to store Docker images.

Recommended option:

```text
GitHub Container Registry - GHCR
```

### Example Image Name

```text
ghcr.io/<username-or-org>/ticketing-api:<tag>
```

### Recommended Tags

```text
latest
dev
commit-sha
```

For safer deployment and rollback, commit SHA tags are preferred.

Example:

```text
ghcr.io/<username-or-org>/ticketing-api:8f3a21c
```

---

## 9. Deployment Strategy

The deployment strategy will be implemented in phases.

---

## 9.1 Phase 1: Simple Deployment

Initial deployment will use:

```text
EC2
Docker Compose
Backend Container
PostgreSQL Container
.env file
Security Group
```

### Purpose

This phase proves that the application can run on a real server using Docker.

---

## 9.2 Phase 2: Production-like AWS Deployment

The project will then be improved by adding important AWS services used in real companies.

Services:

```text
IAM
VPC
EC2
EBS
Security Groups
S3
Route 53
CloudWatch
```

### Purpose

This phase demonstrates understanding of core AWS infrastructure.

---

## 9.3 Phase 3: High Availability Setup

The deployment can then be improved using:

```text
Application Load Balancer
Launch Template
Auto Scaling Group
```

### Purpose

This phase demonstrates a more production-like architecture that supports scalability and availability.

---

## 9.4 Phase 4: Kubernetes Deployment

Kubernetes will be introduced during the later deployment stage.

Kubernetes resources may include:

```text
Deployment
Service
Ingress
ConfigMap
Secret
Persistent Volume if needed
```

### Purpose

This phase demonstrates container orchestration and production-grade deployment practices.

---

## 10. AWS Services Plan

The project will not use every AWS service. Instead, it will use the most important services that are commonly used in real company deployments.

---

## 10.1 Required AWS Services

| AWS Service | Usage in This Project |
|---|---|
| IAM | Manage secure access, users, roles, and least privilege permissions |
| VPC | Create a custom network for the application infrastructure |
| EC2 | Host the backend and database containers in the first deployment stage |
| EBS | Provide persistent storage for EC2 instances |
| Security Groups | Control inbound and outbound traffic |
| S3 | Store event images, banners, or uploaded files |
| Route 53 | Manage DNS records for the API domain |
| Application Load Balancer | Route HTTP/HTTPS traffic to backend instances |
| Launch Template | Define EC2 configuration for Auto Scaling |
| Auto Scaling Group | Maintain desired number of instances and allow scaling |
| CloudWatch | Monitor metrics, logs, and alarms |

---

## 10.2 Optional AWS Services

| AWS Service | Possible Usage |
|---|---|
| Lambda | Run scheduled jobs such as expired booking cleanup |
| RDS PostgreSQL | Replace PostgreSQL container with managed PostgreSQL later |
| ElastiCache Redis | Replace Redis container with managed Redis later |

---

## 11. AWS Architecture Plan

The production-like AWS architecture can be represented as:

```text
User
  |
Route 53
  |
Application Load Balancer
  |
Auto Scaling Group
  |
EC2 Instances
  |
Docker
  |
Backend API Container
  |
PostgreSQL Container
```

Supporting services:

```text
S3          -> Event images and uploaded files
CloudWatch  -> Logs, metrics, and alarms
IAM         -> Secure access control
VPC         -> Network isolation
EBS         -> EC2 persistent storage
```

---

## 12. IAM Plan

IAM will be used to manage secure access to AWS resources.

### Usage

- Create users or roles for deployment.
- Apply least privilege permissions.
- Allow access to S3 only where needed.
- Store AWS credentials securely in GitHub Secrets if needed for CI/CD.

### Interview Value

This demonstrates understanding of secure cloud access and least privilege principles.

---

## 13. VPC and Networking Plan

A custom VPC can be created for the project.

Initial networking components:

```text
VPC
Public Subnet
Internet Gateway
Route Table
Security Groups
```

### Notes

- EC2 can start in a public subnet for simplicity.
- Later, the database can be moved to a private subnet or replaced with RDS.
- Security Groups should only allow required ports.

---

## 14. Security Groups Plan

Security Groups will be used as a firewall for AWS resources.

### Allowed Inbound Ports

| Port | Purpose |
|---|---|
| 22 | SSH access |
| 80 | HTTP |
| 443 | HTTPS |

### Important Rule

PostgreSQL port `5432` should not be publicly exposed.

The database should only be accessible internally by the backend container or private network.

---

## 15. S3 Plan

S3 will be used for storing uploaded files such as:

```text
Event images
Event banners
Static uploaded files
```

### Why S3?

Storing uploaded files inside the application container is not recommended because containers can be recreated or replaced.

S3 provides durable object storage and is commonly used in production systems.

---

## 16. Route 53 Plan

Route 53 will be used to manage DNS records.

Example domain:

```text
api.ticketing-platform.com
```

The DNS record can point to:

```text
Application Load Balancer
```

or directly to:

```text
EC2 public IP
```

in the first simple deployment phase.

---

## 17. Load Balancer Plan

An Application Load Balancer will be used in the production-like phase.

### Purpose

- Receive HTTP/HTTPS traffic.
- Route traffic to backend EC2 instances.
- Support future scaling with Auto Scaling Group.
- Improve production-readiness.

---

## 18. Launch Template and Auto Scaling Group Plan

A Launch Template will define how EC2 instances should be created.

It may include:

```text
AMI
Instance type
Security group
Key pair
User data script
EBS volume configuration
```

An Auto Scaling Group will use the Launch Template to keep the desired number of instances running.

Example configuration:

```text
Minimum instances: 1
Desired instances: 1
Maximum instances: 2
```

### Purpose

- Keep the application available.
- Allow future scaling.
- Demonstrate production-like AWS deployment.

---

## 19. CloudWatch Plan

CloudWatch will be used for monitoring.

### Usage

```text
EC2 metrics
CPU usage
Disk usage
Application logs
Alarms
```

Serilog logs can be written to the console, and container logs can later be collected by CloudWatch.

---

## 20. Kubernetes Plan

Kubernetes will be used later during the deployment stage.

The project may include:

```text
Backend Deployment
Backend Service
Ingress
ConfigMap
Secret
PostgreSQL deployment for testing only
```

### Kubernetes Secrets

Kubernetes Secrets will be used for sensitive values such as:

```text
Database password
JWT secret
Connection strings
```

### Kubernetes ConfigMaps

ConfigMaps will be used for non-sensitive configuration such as:

```text
Environment name
Allowed origins
Logging settings
```

---

## 21. Secrets Management

Secrets must never be committed to Git.

---

## 21.1 Local Development

Use:

```text
.env
dotnet user-secrets
```

Examples:

```text
DATABASE_PASSWORD
JWT_SECRET
CONNECTION_STRING
```

---

## 21.2 Testing Server

Use:

```text
.env file on the server
```

The `.env` file should be created manually on the server and excluded from Git.

---

## 21.3 GitHub Actions

Use GitHub Repository Secrets.

Examples:

```text
SSH_PRIVATE_KEY
SERVER_HOST
SERVER_USER
GHCR_TOKEN
DATABASE_PASSWORD
JWT_SECRET
```

---

## 21.4 Kubernetes

Use:

```text
Kubernetes Secrets
Kubernetes ConfigMaps
```

---

## 22. Logging Plan

The backend will use Serilog for structured logging.

### Logging Strategy

```text
Serilog inside ASP.NET Core backend
Console logs for Docker and Kubernetes
File logs optional for local/server debugging
CloudWatch later for AWS monitoring
```

### Why Console Logs?

In containerized applications, writing logs to console is a best practice because Docker and Kubernetes can collect these logs easily.

### Useful Commands

```bash
docker logs ticketing-api
kubectl logs <pod-name>
```

---

## 23. Health Check Plan

The backend should expose a health endpoint.

```http
GET /health
```

### Purpose

- Check whether the API is running.
- Support Docker health checks.
- Support Load Balancer health checks.
- Support Kubernetes readiness/liveness probes later.

---

## 24. Backup Plan

For the first deployment phase using PostgreSQL container:

```text
PostgreSQL dump backups
EBS snapshots if data is stored on attached volumes
```

For future managed database deployment:

```text
RDS automated backups
Point-in-time recovery
```

---

## 25. Rollback Strategy

Docker image tags should allow rollback.

Recommended image tags:

```text
latest
dev
commit-sha
```

Rollback can be done by deploying a previous image tag.

Example:

```text
ghcr.io/<username-or-org>/ticketing-api:previous-sha
```

---

## 26. Final DevOps Roadmap

### Phase 1: Local Docker Setup

```text
Dockerfile
docker-compose.yml
Backend container
PostgreSQL container
.env
```

### Phase 2: GitHub Actions CI

```text
PR to dev
Restore
Build
Test
```

### Phase 3: Testing Deployment

```text
Merge to dev
Build Docker image
Push to GHCR
Deploy to EC2 using Docker Compose
```

### Phase 4: AWS Production-like Infrastructure

```text
IAM
VPC
EC2
EBS
Security Groups
S3
Route 53
CloudWatch
```

### Phase 5: High Availability

```text
Application Load Balancer
Launch Template
Auto Scaling Group
```

### Phase 6: Kubernetes Deployment

```text
Deployment
Service
Ingress
ConfigMap
Secret
Health checks
```

### Phase 7: Optional Enhancements

```text
Lambda for expired booking cleanup
RDS PostgreSQL
ElastiCache Redis
CloudWatch alarms
```

---

## 27. Interview Summary

A good summary of this DevOps plan:

```text
I containerized the ASP.NET Core backend and PostgreSQL database using Docker.
I used GitHub Actions to run build and test pipelines on pull requests to the dev branch.
After merging to dev, the pipeline builds a Docker image, pushes it to a container registry, and deploys it to a testing environment.
For AWS, I planned a production-like setup using IAM, VPC, EC2, EBS, Security Groups, S3, Route 53, Application Load Balancer, Launch Template, Auto Scaling Group, and CloudWatch.
I also planned to move the deployment to Kubernetes later using Deployments, Services, Ingress, ConfigMaps, and Secrets.
```
