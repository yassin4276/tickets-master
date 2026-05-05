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

## Main Roles

## User

A normal user can:

- Register and login
- Browse published events
- View event details and sessions
- Book seats or ticket quantities
- Simulate payment
- View booking history
- Receive notifications

## Event Owner

An event owner can:

- Register and login
- Create and manage events
- Create event sessions
- Configure seat-based events
- Configure ticket-based events
- Track bookings for owned events

## Admin

An admin can:

- Login
- View users
- View event owners
- View events
- View bookings
- Monitor the platform
- Suspend events if needed

---

## Core Features

The MVP includes:

- Authentication and JWT authorization
- Role-based access control
- Event management
- Event sessions
- Seat-based booking
- Ticket-based booking
- Booking expiration
- Payment simulation
- Basic notifications
- Admin monitoring

Advanced planned features include:

- Redis caching
- SignalR real-time updates
- Docker deployment
- GitHub Actions CI/CD
- Kubernetes deployment
- AWS production-like infrastructure
- S3 file storage
- CloudWatch monitoring
- Load Balancer and Auto Scaling

---

## Tech Stack

## Backend

- ASP.NET Core Web API
- C#
- Entity Framework Core
- ASP.NET Core Identity
- JWT Authentication
- FluentValidation
- Serilog

## Database

- PostgreSQL

## DevOps

- Docker
- Docker Compose
- GitHub Actions
- GitHub Container Registry

## Kubernetes

- Kubernetes
- ConfigMaps
- Secrets
- Services
- Deployments
- Ingress
- Health probes

## AWS

Planned AWS services:

- IAM
- VPC
- EC2
- EBS
- Security Groups
- S3
- Route 53
- Application Load Balancer
- Launch Template
- Auto Scaling Group
- CloudWatch

Optional later:

- Lambda
- RDS PostgreSQL
- ElastiCache Redis

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
| Ticketing.Infrastructure | EF Core, PostgreSQL, Identity, external services, storage, Redis later |
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

## DevOps Plan

The project starts with two containers:

```text
Backend API Container
PostgreSQL Database Container
```

Redis can be added later.

## Environments

```text
Development
Testing
```

## CI/CD

GitHub Actions will be used.

Pull request to `dev`:

```text
Restore dependencies
Build solution
Run tests
```

Merge to `dev`:

```text
Restore dependencies
Build solution
Run tests
Build Docker image
Push Docker image to GHCR
Deploy to testing environment
```

---

## Kubernetes Plan

Kubernetes will be introduced before AWS.

The project will practice:

- Namespace
- Deployment
- Service
- Ingress
- ConfigMap
- Secret
- PersistentVolume
- PersistentVolumeClaim
- Readiness probe
- Liveness probe

Possible local/testing clusters:

- Minikube
- Kind
- K3s

---

## AWS Plan

After Kubernetes, the project will move to AWS production-like infrastructure.

Planned AWS architecture:

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
Docker / Kubernetes
  |
Backend API
  |
PostgreSQL
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

## Phase 13: Redis and SignalR

- Redis caching
- Temporary reservation locks
- SignalR real-time updates

## Phase 14: AWS Production-like Deployment

- IAM
- VPC
- EC2
- EBS
- Security Groups
- S3
- Route 53
- CloudWatch

## Phase 15: AWS Load Balancing and Auto Scaling

- Application Load Balancer
- Launch Template
- Auto Scaling Group
- Health checks
- CloudWatch alarms

---

## Documentation

Project documentation is available in the `docs/` folder.

```text
docs/
├── Project Requirements.md
├── High-Level Architecture.md
├── Database Design.md
├── API Design.md
├── Architecture Decisions.md
├── DevOps Planning.md
├── Project Structure.md
├── MVP Definition.md
└── Development Roadmap.md
```

---

## Suggested Local Setup

This section will be updated during development.

Expected setup:

```bash
git clone <repository-url>
cd TicketingPlatform
docker compose up -d
dotnet restore
dotnet build
dotnet run --project src/Ticketing.API
```

---

## Environment Variables

Example environment variables:

```text
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=
Jwt__Secret=
Jwt__Issuer=
Jwt__Audience=
```

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

Planned test projects:

```text
tests/
├── Ticketing.Application.Tests
└── Ticketing.API.Tests
```

Important test cases:

- User cannot book already booked seat.
- User cannot book already reserved seat.
- User cannot book more tickets than available.
- Event owner cannot edit another owner's event.
- User cannot access another user's booking.
- Successful payment confirms booking.
- Expired booking releases reserved seats or ticket quantities.

---

## Interview Summary

This project demonstrates:

- Backend development with ASP.NET Core
- Clean Architecture
- PostgreSQL database design
- Authentication and authorization
- Booking and payment simulation workflows
- Docker and containerization
- GitHub Actions CI/CD
- Kubernetes deployment
- AWS infrastructure basics
- Monitoring and logging
- Production-wise project planning

A concise explanation:

```text
I built a ticketing platform using ASP.NET Core, PostgreSQL, and Clean Architecture.
The system supports users, event owners, and admins.
It includes event management, sessions, seat-based booking, ticket-based booking, payment simulation, notifications, and admin monitoring.
After implementing the backend, I containerized it with Docker, added GitHub Actions CI/CD, deployed it to Kubernetes, and planned a production-like AWS setup using IAM, VPC, EC2, S3, Route 53, Load Balancer, Auto Scaling, and CloudWatch.
```

---

## Project Status

Current status:

```text
Pre-Development Stage
```

Next step:

```text
Start Phase 1: Project Initialization
```
