# Ticketing Platform

## Overview

Ticketing Platform is an educational, production-wise backend project built to practice real-world backend, DevOps, Kubernetes, and AWS concepts.

The system is a ticketing marketplace where:

- **Users** browse events and book seats or tickets.
- **Event Owners** create and manage their own events.
- **Admins** monitor the platform.

The project is designed to start with a clear MVP, then grow step by step into a production-like system using Docker, CI/CD, Kubernetes, AWS, logging, monitoring, and real-time updates.

---

## Project Purpose

The goal of this project is not only to build a working ticketing API, but also to practice the full software development lifecycle before and after coding.

This includes:

- Requirements gathering
- High-level architecture
- Database design
- API design
- Architecture decisions
- DevOps planning
- Clean project structure
- MVP definition
- Development roadmap
- Dockerization
- CI/CD
- Kubernetes deployment
- AWS production-like infrastructure

---

## Development

This stage is the **application and backend work**: APIs, data model, authentication, and tests. It is separate from [DevOps & deployment](#devops--deployment) (containers, CI/CD, cloud, Kubernetes).

### Solution structure

- **Clean Architecture** solution: `Ticketing.API`, `Ticketing.Application`, `Ticketing.Domain`, `Ticketing.Infrastructure`
- **PostgreSQL** with Entity Framework Core and migrations
- **ASP.NET Core Identity** with JWT access tokens and refresh tokens
- **FluentValidation** and **Serilog** integrated in the API pipeline
- **Health checks** (including `/health` for orchestration)
- **CORS** configured for local web clients in Development

### Authentication (`api/Auth`)

- Register and login
- Refresh token rotation
- Email confirmation (link and token flows) and resend confirmation
- Forgot password and reset password

### Identity and seed data

- Roles: `User`, `EventOwner`, `Admin`
- Development seed users (see `IdentitySeeder`): admin, event owner, and normal user test accounts

### Event owner APIs (`api/event-owner`)

- Create, list, get, and update owned events
- Create, update, and delete sessions
- Create, update, and delete seats (seat-based events)
- Create, update, and delete ticket types (ticket-based events)
- Publish, cancel, and return events to draft
- Owner dashboard summary

### User APIs (`api/user`)

- Browse published events and filters
- Event details and sessions
- List seats or ticket types for a session
- Create seat-based or ticket-based bookings
- Cancel a booking
- List and view own booking details

### Tests

- `tests/Ticketing.IntegrationTests` — integration tests run in CI

### Still planned or partial (MVP remainder)

The domain model includes entities such as payments and notifications; **simulated payment confirmation**, **in-app notifications**, and **admin monitoring HTTP APIs** are not fully wired as user-facing endpoints yet. Those items remain on the roadmap below.

---

## DevOps & Deployment

This project includes a complete DevOps deployment workflow:

- Dockerized ASP.NET Core API
- PostgreSQL database using Docker Compose
- AWS networking and compute on a single EC2 host provisioned with Terraform (VPC, public subnet, Internet Gateway, route table, security group, EC2)
- GitHub Actions CI/CD pipeline
- Docker image publishing to GHCR
- Nginx reverse proxy on AWS EC2
- Health checks for deployment validation
- Kubernetes local deployment using Minikube
- Kubernetes Ingress, Services, PVC, ConfigMap, Secret
- Rolling update and rollback practice

- CD deploys to EC2 (SSH + Docker Compose) after a successful CI run on `dev`

For full details:

- [Architecture Documentation](Docs/Devops-Deployment/ARCHITECTURE.md)
- [Deployment Documentation](Docs/Devops-Deployment/DEPLOYMENT_DOCUMENTATION.md)

---

## Main Roles

### User

A normal user can:

- Register and login
- Browse published events
- View event details and sessions
- Book seats or ticket quantities
- Simulate payment
- View booking history
- Receive notifications

### Event Owner

An event owner can:

- Register and login
- Create and manage events
- Create event sessions
- Configure seat-based events
- Configure ticket-based events
- Track bookings for owned events

### Admin

An admin can:

- Login
- View users
- View event owners
- View events
- View bookings
- Monitor the platform
- Suspend events if needed

---

## Core features

**Product (MVP target):** authentication and JWT authorization, role-based access control, event and session management, seat-based and ticket-based booking, booking expiration, simulated payment, basic notifications, and admin monitoring. What is already built versus still in progress is spelled out under [Development](#development).

**Platform and operations:** Docker, GitHub Actions, GHCR, EC2 deployment, Nginx, health checks, and Kubernetes manifests are covered under [DevOps & Deployment](#devops--deployment).

**Later enhancements:** optional distributed caching if needed, SignalR real-time updates, richer media on S3, deeper CloudWatch usage, Application Load Balancer with Auto Scaling, managed RDS PostgreSQL, optional managed in-memory cache on AWS if you adopt one, and related production hardening.

---

## Tech Stack

### Backend

- ASP.NET Core Web API
- C#
- Entity Framework Core
- ASP.NET Core Identity
- JWT Authentication
- FluentValidation
- Serilog

### Database

- PostgreSQL

### DevOps

- Docker and Docker Compose (local and server)
- GitHub Actions (CI build, test, image push to GHCR; CD deploy to EC2)
- GitHub Container Registry

### Kubernetes

- Minikube-oriented manifests under `k8s/`
- Namespace, Deployment, Service, Ingress
- ConfigMap, Secret (see `Docs/Devops-Deployment/k8s-secret.example.yml`)
- PersistentVolumeClaim for PostgreSQL data
- Readiness and liveness style health checks wired to `/health`

### AWS

**In Terraform today** (`infra/aws/ec2`; see deployment docs): a minimal footprint only — **VPC**, **public subnet**, **Internet Gateway**, **route table** (with route to the IGW) **and association**, **security group**, and **EC2** (Ubuntu, Docker via user data). An **SSH key pair** is created for deploy access; the instance uses a normal **root EBS volume** (gp3) as part of the EC2 resource, not as a separate managed service layer.

**Future plan (not provisioned in this module):** IAM roles/instance profiles for the app, RDS or Aurora, optional managed in-memory cache if you introduce caching, S3 for uploads, Route 53 and ACM/TLS, Application Load Balancer, Auto Scaling groups, CloudWatch dashboards and alarms, Lambda, VPN/WAF, and other production-style add-ons.

---

## Architecture

The project follows Clean Architecture.

Recommended solution structure:

```text
src/
├── Ticketing.API
├── Ticketing.Application
├── Ticketing.Domain
└── Ticketing.Infrastructure
```

## Layer Responsibilities

| Layer | Responsibility |
|---|---|
| Ticketing.Domain | Core entities, enums, domain rules, and business concepts |
| Ticketing.Application | Use cases, DTOs, interfaces, validation, and application services |
| Ticketing.Infrastructure | EF Core, PostgreSQL, Identity, external services, storage, optional caching later if needed |
| Ticketing.API | Controllers, authentication setup, authorization, middleware, Swagger, health checks |

## Dependency Direction

```text
Ticketing.API
    -> Ticketing.Application
    -> Ticketing.Infrastructure

Ticketing.Application
    -> Ticketing.Domain

Ticketing.Infrastructure
    -> Ticketing.Application
    -> Ticketing.Domain
```

Important rules:

- Domain does not depend on any other layer.
- Application does not depend on API or Infrastructure implementation details.
- Infrastructure implements interfaces defined by the Application layer.
- API acts as the entry point of the system.

---

## Database Overview

The database is designed around the following main entities:

- Users
- Events
- Event Sessions
- Seats
- Ticket Types
- Bookings
- Booking Seats
- Booking Ticket Types
- Payments
- Notifications

The system supports two booking modes:

```text
Seats
Tickets
```

For seat-based events, users select specific seats.

For ticket-based events, users select a ticket type and quantity.

---

## Booking Flow

## Seat-Based Booking

```text
Event Owner creates event with BookingMode = Seats
Event Owner creates event session
Event Owner creates seats
User views event
User selects session
User selects available seats
System creates pending booking
System reserves selected seats
User simulates payment
System confirms booking
System marks seats as booked
```

## Ticket-Based Booking

```text
Event Owner creates event with BookingMode = Tickets
Event Owner creates event session
Event Owner creates ticket types
User views event
User selects session
User selects ticket type and quantity
System creates pending booking
System reserves quantity
User simulates payment
System confirms booking
System updates ticket availability
```

---

## Booking Statuses

```text
Pending
Confirmed
Cancelled
Expired
Failed
```

A booking starts as `Pending`.

If payment succeeds, the booking becomes `Confirmed`.

If the booking expires before payment, it becomes `Expired`.

If the user cancels the booking, it becomes `Cancelled`.

---

## Payment Simulation

The MVP uses payment simulation instead of a real payment gateway.

Payment simulation supports:

- Successful payment
- Failed payment

A successful payment confirms the booking and updates seat or ticket availability.

A real payment gateway can be added later.

---

## Delivery notes (DevOps vs product roadmap)

The **running system** today (containers, CI/CD, GHCR, Terraform EC2, server Compose with Nginx, Minikube manifests, probes) is described under [DevOps & Deployment](#devops--deployment) and in the linked architecture and deployment documents.

The **Project Roadmap** below is still the checklist for **application** work (payments, notifications, admin APIs, SignalR, optional caching, fuller AWS, and so on).

---

## Project Roadmap

## Phase 0: Pre-Development

- Requirements
- Architecture
- ERD
- API Design
- Architecture Decisions
- DevOps Planning
- Project Structure
- MVP Definition
- Development Roadmap
- README

## Phase 1: Project Initialization

- Create repository
- Create solution
- Create Clean Architecture projects
- Add documentation
- Setup PostgreSQL connection
- Setup EF Core

## Phase 2: Authentication and Authorization

- ASP.NET Core Identity
- JWT
- Roles
- Register/Login
- Admin seed

## Phase 3: Events Management

- Event CRUD
- Owner authorization
- Public event listing

## Phase 4: Event Sessions

- Session CRUD
- Public session listing
- Date/time validation

## Phase 5: Seats and Ticket Types

- Seat management
- Ticket type management
- Booking mode rules

## Phase 6: Booking System

- Seat-based booking
- Ticket-based booking
- Pending bookings
- Booking expiration
- Cancel booking

## Phase 7: Payment Simulation

- Simulated payment
- Confirm booking
- Fail payment
- Payment transaction handling

## Phase 8: Notifications and Admin

- Database notifications
- Admin monitoring
- Suspend event

## Phase 9: Testing

- Unit tests
- API tests
- Booking tests
- Authorization tests

## Phase 10: Docker

- Dockerfile
- Docker Compose
- Backend container
- PostgreSQL container

## Phase 11: CI/CD

- GitHub Actions
- Build/test pipeline
- Docker image build
- Push to GHCR
- Deploy to testing

## Phase 12: Kubernetes

- Deployment
- Service
- Ingress
- ConfigMap
- Secret
- Health probes

## Phase 13: SignalR and optional caching (planned)

- SignalR real-time updates
- Optional distributed caching and temporary reservation helpers (not in the codebase today)

## Phase 14: AWS (Terraform scope vs future)

**Done in this repo (Terraform):** VPC, public subnet, Internet Gateway, route table, security group, EC2.

**Future plan:** IAM patterns for workloads, S3, Route 53, CloudWatch, RDS, and other managed services beyond the single-instance layout.

## Phase 15: AWS load balancing and auto scaling (planned)

- Application Load Balancer
- Launch Template
- Auto Scaling Group
- Health checks
- CloudWatch alarms

---

## Documentation

Authoritative specs and design notes live under [`Docs/`](Docs/):

```text
Docs/
├── project_requirements.md
├── Ticketing_Architecture_Decisions.md
├── Ticketing_API_Design.md
├── Ticketing_Database_Design/
├── Ticketing_MVP_Definition.md
├── Ticketing_Project_Structure.md
└── Devops-Deployment/
    ├── ARCHITECTURE.md
    ├── DEPLOYMENT_DOCUMENTATION.md
    └── k8s-secret.example.yml
```

---

## Suggested local setup

From the repository root:

```bash
git clone <repository-url>
cd <repository-directory>
cp .env.example .env   # set DB and JWT values for Compose / local runs
docker compose up -d
dotnet restore
dotnet build
dotnet run --project src/Ticketing.API
```

With Compose running, the API is typically on port **8080** (see `docker-compose.yml`). You can also run the API on the host and point `ConnectionStrings__DefaultConnection` at the Compose PostgreSQL service.

---

## Environment Variables

Example environment variables:

```text
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=
JwtSettings__SecretKey=
JwtSettings__Issuer=
JwtSettings__Audience=
```

Docker Compose uses the same `JwtSettings__*` and `ConnectionStrings__DefaultConnection` style variables (see `docker-compose.yml`).

Sensitive values must not be committed to Git.

Use:

- `.env`
- `.NET user-secrets`
- GitHub Secrets
- Kubernetes Secrets

---

## Health Check

The API should expose:

```http
GET /health
```

This endpoint will be used for:

- Local health checks
- Docker health checks
- Kubernetes readiness/liveness probes
- Load Balancer health checks

---

## Testing

Test project layout:

```text
tests/
└── Ticketing.IntegrationTests
```

Important scenarios to cover (and to extend over time):

- User cannot book an already booked seat.
- User cannot book more tickets than available.
- Event owner cannot edit another owner's event.
- User cannot access another user's booking.
- Successful payment confirms booking (once the payment API exists).
- Expired booking releases reserved seats or ticket quantities.

---

## Interview Summary

This project demonstrates:

- Backend development with ASP.NET Core
- Clean Architecture
- PostgreSQL database design
- Authentication and authorization
- Booking flows; simulated payment confirmation and admin APIs are still on the roadmap
- Docker and containerization
- GitHub Actions CI/CD
- Kubernetes deployment
- AWS infrastructure basics
- Monitoring and logging
- Production-wise project planning

A concise explanation:

```text
I built a ticketing platform using ASP.NET Core, PostgreSQL, and Clean Architecture.
The system supports users, event owners, and admin roles in Identity, with event management, sessions, seat-based booking, and ticket-based booking implemented in the API.
Simulated payment, notifications, and admin monitoring endpoints are planned next on the product side.
On the operations side, the API is containerized with Docker, built and tested in GitHub Actions with images pushed to GHCR, deployed to EC2 (Compose and Nginx), and supported by Kubernetes manifests for local Minikube practice, with a path toward fuller AWS networking and scaling.
```

---

## Project Status

**Development:** Core APIs are in place for auth, event owner management, user discovery, and bookings. Simulated payment confirmation, notifications, and dedicated admin HTTP APIs are not finished yet (see [Development](#development)).

**DevOps & deployment:** Docker images, GitHub Actions (CI and CD to EC2), GHCR, Terraform for EC2, server-side Compose with Nginx, and Kubernetes manifests for Minikube are part of the current workflow (see [DevOps & Deployment](#devops--deployment)).

Next focus areas (see roadmap):

```text
MVP remainder: payment simulation endpoints, notifications, admin monitoring APIs
Product hardening: more tests, SignalR, optional caching if required
Cloud: ALB, Auto Scaling, RDS, and related production patterns
```
