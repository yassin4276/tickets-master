# Ticketing Platform - Architecture Decisions

## Overview

This document records the main architecture decisions for the Ticketing Platform.

The goal of this document is to explain why specific technologies, patterns, and design choices were selected before starting development.

The project is a ticketing platform with three main roles:

- **User**: Browses events and books seats or tickets.
- **Event Owner**: Creates and manages events, sessions, seats, ticket types, and tracks bookings.
- **Admin**: Monitors the platform and manages system-level moderation.

The system includes important business workflows such as:

- Event management
- Seat-based booking
- Ticket quantity-based booking
- Booking expiration
- Payment simulation
- Real-time availability updates
- Role-based access control

---

# ADR-001: Use Clean Architecture

## Status

Accepted

## Context

The system contains multiple business rules around events, sessions, seats, ticket types, bookings, payments, notifications, and roles.

These business rules should not be tightly coupled to the database, API controllers, optional caching layers, SignalR, or any external infrastructure.

Examples of business rules include:

- A seat cannot be booked by more than one active booking.
- A ticket type cannot be booked if the available quantity is not enough.
- A booking must expire if payment is not completed within the allowed time.
- Event owners can only manage their own events.
- Admins can monitor the entire platform.

## Decision

Use Clean Architecture and split the project into clear layers:

```text
src/
├── Ticketing.API
├── Ticketing.Application
├── Ticketing.Domain
└── Ticketing.Infrastructure
```

### Layer Responsibilities

| Layer | Responsibility |
|---|---|
| Domain | Core entities, enums, domain rules, and business concepts |
| Application | Use cases, services, DTOs, validation, interfaces |
| Infrastructure | Database, optional caching, SignalR implementation, external services |
| API | Controllers, authentication, authorization, request/response handling |

## Consequences

### Positive

- Clear separation of concerns
- Easier testing of business logic
- Easier to replace infrastructure details later
- Better maintainability as the project grows
- Suitable for a portfolio-level backend project

### Negative

- More initial setup
- More folders and abstractions
- Slower to start compared to a simple CRUD structure

---

# ADR-002: Use PostgreSQL as the Main Database

## Status

Accepted

## Context

The platform depends heavily on relational data and transactional consistency.

Important relationships include:

- Users and events
- Events and sessions
- Sessions and seats
- Sessions and ticket types
- Users and bookings
- Bookings and payments
- Bookings and booking items

The booking workflow requires strong consistency to avoid double booking and incorrect ticket quantities.

## Decision

Use PostgreSQL as the main relational database.

PostgreSQL will store the source of truth for:

- Users
- Events
- Event sessions
- Seats
- Ticket types
- Bookings
- Payments
- Notifications

## Consequences

### Positive

- Strong relational modeling
- Supports transactions
- Supports constraints and indexes
- Good support for concurrency control
- Reliable for booking and payment workflows

### Negative

- Requires proper database design
- Requires careful transaction handling for booking operations

---

# ADR-003: Optional distributed cache for caching and temporary reservation state (planned)

## Status

Planned — **not implemented**. The codebase relies on PostgreSQL only today; this ADR records a possible future direction.

## Context

The platform may have many users viewing the same event session and seat availability at the same time.

Seat availability and ticket availability can become performance-sensitive, especially during popular events.

The system also needs temporary reservation behavior when a booking is pending.

Example:

- A user selects seats.
- The seats become temporarily reserved.
- The user has a limited time to complete payment.
- If payment is not completed, the seats become available again.

## Decision

If a distributed cache is introduced later, it could be used for:

- Caching frequently accessed event/session availability data
- Temporary seat reservation locks
- Temporary ticket quantity reservation
- Supporting SignalR scale-out in the future if needed

PostgreSQL remains the source of truth.

A cache must not be the only place where permanent booking data is stored.

## Consequences

### Positive

- Faster availability lookups when a cache is present
- Useful for temporary locks and expiration
- Reduces database load at high read volume
- Helpful for real-time booking scenarios

### Negative

- Adds infrastructure complexity when adopted
- Requires cache invalidation strategy
- Requires careful handling to avoid mismatch between cache and PostgreSQL

---

# ADR-004: Use SignalR for Real-Time Updates

## Status

Accepted for later stage

## Context

In a ticketing system, multiple users may view the same event session at the same time.

When one user reserves or books a seat, other users should see the updated seat status quickly.

Without real-time updates, the frontend would need frequent polling, which increases API load and creates a worse user experience.

## Decision

Use SignalR for real-time communication.

SignalR will be used for:

- Real-time seat status updates
- Real-time ticket availability updates
- Booking status notifications
- User notifications

## Consequences

### Positive

- Better user experience
- Reduces the need for polling
- Useful for live seat maps
- Fits well with ASP.NET Core

### Negative

- More complexity in deployment
- Requires connection management
- Scaling may require a distributed backplane (for example Azure SignalR Service or another compatible message bus) if self-hosted hubs outgrow a single node

---

# ADR-005: Use JWT Authentication

## Status

Accepted

## Context

The platform exposes APIs that may be consumed by web or mobile clients.

Authentication should be stateless and suitable for REST APIs.

The system also needs to support multiple roles:

- User
- EventOwner
- Admin

## Decision

Use JWT Bearer Authentication.

Authenticated requests will include:

```http
Authorization: Bearer <access_token>
```

The JWT will contain user identity and role claims.

## Consequences

### Positive

- Stateless authentication
- Works well with REST APIs
- Suitable for web and mobile clients
- Supports role-based authorization

### Negative

- Token expiration and refresh handling must be designed carefully
- Token storage on the client must be secure
- Revoking tokens requires additional strategy if needed

---

# ADR-006: Use Role-Based Access Control

## Status

Accepted

## Context

The system has three different user types with different permissions.

Examples:

- A normal user can create bookings.
- An event owner can create and manage their own events.
- An admin can monitor events, bookings, and users.
- An event owner must not modify another owner's events.

## Decision

Use Role-Based Access Control with the following roles:

```text
User
EventOwner
Admin
```

Authorization will be enforced at API level and application service level.

## Consequences

### Positive

- Clear permission model
- Protects event owner resources
- Separates public, owner, and admin features
- Easy to understand and document

### Negative

- Requires careful authorization checks
- Ownership checks are still needed, not only role checks

---

# ADR-007: Use Pending Bookings with Expiration

## Status

Accepted

## Context

When a user selects seats or ticket quantities, the system should not instantly confirm the booking before payment.

However, the selected seats or quantities should be temporarily unavailable to other users while the first user completes payment.

Without expiration, users could reserve seats forever without paying.

## Decision

Use a booking lifecycle with expiration.

Main booking statuses:

```text
Pending
Confirmed
Cancelled
Expired
Failed
```

A new booking starts as `Pending`.

The booking receives an `ExpiresAt` value.

If the user completes payment before expiration, the booking becomes `Confirmed`.

If the user does not complete payment before expiration, the booking becomes `Expired`, and reserved seats or ticket quantities are released.

## Consequences

### Positive

- Prevents seats from being locked forever
- Supports payment workflow
- Makes booking lifecycle clear
- Better user experience during checkout

### Negative

- Requires background job or scheduled cleanup
- Requires careful handling of race conditions
- Requires seat/ticket release logic

---

# ADR-008: Use Docker for Containerization

## Status

Accepted

## Context

The system includes multiple services:

- ASP.NET Core API
- PostgreSQL
- Optional distributed cache or background workers later, if needed

The development and deployment environments should be consistent.

## Decision

Use Docker to containerize the application and supporting services.

Docker will be used for:

- Local development
- Running PostgreSQL (and any optional supporting services you add later, such as a cache)
- Packaging the ASP.NET Core API
- Preparing the project for CI/CD and Kubernetes deployment

## Consequences

### Positive

- Consistent environments
- Easier local setup
- Easier deployment
- Prepares the project for Kubernetes
- Better CI/CD integration

### Negative

- Requires Docker knowledge
- Requires Dockerfile and docker-compose maintenance

---

# ADR-009: Use Kubernetes in Later Deployment Stage

## Status

Planned

## Context

The MVP can run using Docker Compose or a simple VPS deployment.

However, the project is intended to demonstrate production-grade DevOps practices.

Kubernetes provides orchestration features that become valuable as the system grows.

## Decision

Use Kubernetes in a later deployment stage.

Kubernetes will be used for:

- API deployment
- Service discovery
- Rolling updates
- Self-healing
- Scaling
- ConfigMaps and Secrets
- Future production-readiness

Kubernetes is not required for the first MVP release.

## Consequences

### Positive

- Production-grade deployment model
- Supports scaling
- Supports rolling updates
- Useful for DevOps learning and portfolio value

### Negative

- Adds operational complexity
- Requires Kubernetes manifests or Helm charts
- May be overkill for early MVP

---

# ADR-010: Use AWS for Cloud Hosting

## Status

Planned

## Context

The platform needs a cloud environment for production deployment.

AWS provides managed services that can host the API, database, cache, files, and monitoring tools.

## Decision

Use AWS as the target cloud provider.

Potential AWS services:

| Purpose | AWS Service |
|---|---|
| API hosting | EKS or EC2 |
| PostgreSQL database | Amazon RDS PostgreSQL |
| Optional cache layer | Managed in-memory cache on AWS (only if caching is adopted) |
| File storage | Amazon S3 |
| Monitoring/logging | Amazon CloudWatch |
| DNS | Route 53 |
| Load balancing | Elastic Load Balancer |

## Consequences

### Positive

- Mature cloud ecosystem
- Managed database and cache options
- Good integration with Kubernetes
- Suitable for production deployment

### Negative

- Requires cost control
- Requires cloud security knowledge
- Requires IAM, networking, and monitoring setup

---

# ADR-011: Use Payment Simulation for MVP

## Status

Accepted for MVP

## Context

Real payment integration adds external dependencies, compliance considerations, and gateway-specific behavior.

The MVP should focus on the core booking flow first:

- Create pending booking
- Reserve seats or ticket quantities
- Simulate payment
- Confirm booking after successful payment
- Release reservation if payment fails or expires

## Decision

Use payment simulation in the MVP.

A simulated payment endpoint will be used:

```http
POST /api/payments/simulate
```

Real payment gateway integration can be added later.

## Consequences

### Positive

- Faster MVP development
- Allows testing the full booking lifecycle
- Avoids early dependency on payment providers
- Keeps focus on business logic

### Negative

- Not production-ready for real transactions
- Requires replacement with real payment gateway later

---

# ADR-012: Keep PostgreSQL as the Source of Truth

## Status

Accepted

## Context

Optional caching and SignalR may be used later for performance and real-time updates, but they should not replace the main persistent database.

Booking, payment, and event data must be reliable and recoverable.

## Decision

PostgreSQL will remain the source of truth for all persistent data.

Any optional cache may store temporary or derived data, but permanent state must be stored in PostgreSQL.

## Consequences

### Positive

- Reliable data persistence
- Easier recovery after cache loss or eviction
- Clear system ownership of data
- Better consistency for important workflows

### Negative

- Requires synchronization between cache and database
- Requires clear cache invalidation rules

---

# ADR-013: Use Conventional Commits

## Status

Accepted

## Context

The project will use Git and may include CI/CD pipelines later.

A consistent commit style makes project history easier to read and supports automation.

## Decision

Use Conventional Commits.

Examples:

```text
feat: add booking endpoint
fix: resolve seat concurrency issue
docs: add API design document
refactor: improve booking service structure
test: add booking validation tests
chore: update docker compose
```

## Consequences

### Positive

- Cleaner Git history
- Easier collaboration
- Useful for changelog generation
- Helpful for CI/CD workflows

### Negative

- Requires discipline while committing

---

# Summary

The platform architecture is designed to be:

- Maintainable
- Scalable
- Testable
- Suitable for real-time booking scenarios
- Suitable for DevOps and cloud deployment learning

The MVP will focus on:

- ASP.NET Core API
- Clean Architecture
- PostgreSQL
- JWT Authentication
- Role-Based Authorization
- Events
- Sessions
- Bookings
- Payment Simulation

Later stages will add:

- Optional distributed caching (planned)
- SignalR
- Docker deployment
- CI/CD
- Kubernetes
- AWS
