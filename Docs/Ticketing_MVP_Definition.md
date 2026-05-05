# Ticketing Platform - MVP Definition

## 1. Overview

This document defines the MVP and the extended learning scope for the Ticketing Platform.

The project is educational and portfolio-oriented.  
The main goal is not only to build a working ticketing system, but also to practice building a production-wise backend project from planning to deployment.

The project should start with a clear MVP, then expand gradually to include more production-ready features such as Redis, SignalR, Docker, CI/CD, Kubernetes, AWS services, monitoring, and advanced deployment practices.

---

## 2. Project Goal

The goal of the Ticketing Platform is to build a system where:

- Event owners can create and manage their own events.
- Users can browse events and book seats or tickets.
- Admins can monitor the platform.
- The backend follows clean architecture and production-ready practices.
- The project demonstrates real-world backend, DevOps, cloud, and deployment skills.

The system is similar in concept to a marketplace model:

- Event owners act like sellers.
- Users act like buyers.
- Admins monitor the platform.

---

## 3. MVP Goal

The MVP should prove the core business idea:

```text
An event owner can create an event.
A user can view the event.
A user can book a seat or ticket.
A user can simulate payment.
The booking becomes confirmed.
```

The MVP does not need to include every advanced feature from the beginning.

However, since the project is educational, the full project roadmap will include advanced features after the MVP is completed.

---

## 4. Main Roles

## 4.1 User

The User can:

- Register and login.
- Browse events.
- View event details.
- View event sessions.
- View available seats or ticket types.
- Create bookings.
- Simulate payment.
- View booking history.
- Receive basic notifications.

---

## 4.2 Event Owner

The Event Owner can:

- Register and login.
- Create events.
- Update events.
- Delete or archive events.
- Create event sessions.
- Configure seat-based events.
- Configure ticket-based events.
- Track bookings for owned events.

---

## 4.3 Admin

The Admin can:

- Login.
- View users.
- View event owners.
- View events.
- View bookings.
- Monitor the platform.
- Suspend or approve events if needed.

---

# 5. MVP Included Features

## 5.1 Authentication and Authorization

The MVP includes:

- Register
- Login
- JWT Authentication
- Role-Based Authorization

Roles:

```text
User
EventOwner
Admin
```

Important rules:

- Users can only access their own bookings.
- Event owners can only manage their own events.
- Admins can monitor platform-level data.

---

## 5.2 Event Management

The MVP includes event management for event owners.

Event owners can:

- Create event.
- Update event.
- Delete or archive event.
- View owned events.
- Publish event.

Users can:

- View published events.
- View event details.

---

## 5.3 Event Sessions

The MVP includes event sessions.

A single event can have one or more sessions.

Examples:

- Same event on different days.
- Same event at different times.
- Same event in different locations.

Event owners can:

- Create event sessions.
- Update event sessions.
- View event sessions.

Users can:

- View sessions for a published event.

---

## 5.4 Booking Mode

Each event should support one booking mode:

```text
Seats
Tickets
```

### Seats Mode

Used when users select specific seats.

Example:

```text
A1
A2
B1
VIP Seat
Regular Seat
```

### Tickets Mode

Used when users select a ticket type and quantity.

Example:

```text
VIP Ticket x 2
Regular Ticket x 3
```

This design allows the platform to support different types of events.

---

## 5.5 Seat-Based Booking

For events with booking mode `Seats`, the MVP includes:

Event owner can:

- Create seats for a session.
- Set seat number.
- Set seat type.
- Set seat price.

User can:

- View available seats.
- Select one or more seats.
- Create a pending booking.
- Simulate payment.
- Confirm booking after successful payment.

Seat statuses:

```text
Available
Reserved
Booked
```

---

## 5.6 Ticket-Based Booking

For events with booking mode `Tickets`, the MVP includes:

Event owner can:

- Create ticket types.
- Set ticket type name.
- Set price.
- Set total quantity.

User can:

- View ticket types.
- Select ticket type and quantity.
- Create a pending booking.
- Simulate payment.
- Confirm booking after successful payment.

Ticket quantity rules:

- Quantity must be greater than zero.
- Available quantity must be enough.
- Reserved or booked quantity should be tracked.

---

## 5.7 Booking System

The booking system is one of the core parts of the MVP.

The MVP includes:

- Create seat-based booking.
- Create ticket-based booking.
- View booking details.
- View my bookings.
- Cancel booking.
- Booking expiration.

Booking statuses:

```text
Pending
Confirmed
Cancelled
Expired
Failed
```

Important rules:

- A booking starts as `Pending`.
- A pending booking has an expiration time.
- If payment succeeds, the booking becomes `Confirmed`.
- If payment is not completed before expiration, the booking becomes `Expired`.
- Cancelled bookings should release seats or ticket quantities if applicable.

---

## 5.8 Payment Simulation

The MVP includes payment simulation instead of real payment integration.

Payment simulation should support:

- Successful payment.
- Failed payment.

After successful payment:

- Booking status becomes `Confirmed`.
- Seat status becomes `Booked`, if booking mode is seats.
- Ticket quantity is confirmed, if booking mode is tickets.
- A notification is created for the user.

Payment statuses:

```text
Pending
Paid
Failed
Refunded
```

Real payment gateway integration is not required in the MVP.

---

## 5.9 Notifications

The MVP includes basic database-based notifications.

Examples:

- Booking confirmed.
- Booking cancelled.
- Booking expired.
- Payment failed.

The MVP does not require real-time notifications.

SignalR can be added later.

---

## 5.10 Admin Monitoring

The MVP includes basic admin monitoring.

Admin can:

- View users.
- View event owners.
- View events.
- View bookings.
- Suspend event if needed.

The admin role is mainly for monitoring and moderation.

---

# 6. MVP Excluded Features

The following features are excluded from the first MVP, but planned for later stages.

```text
Redis
SignalR
Kubernetes
Terraform
Advanced AWS setup
Real payment gateway
Email notifications
SMS notifications
Advanced analytics dashboard
Coupons and discounts
Refund system
Reviews and ratings
Advanced seat map UI
Multi-language support
Mobile application
```

These features are important, but they should be added after the core MVP is stable.

---

# 7. Educational Extended Scope

Since this project is educational and production-wise, the final project may include more than a normal MVP.

The extended scope includes:

## 7.1 Backend Production Practices

- Clean Architecture
- ASP.NET Core Identity
- JWT Authentication
- Role-Based Authorization
- FluentValidation
- Global exception handling
- Serilog structured logging
- Health checks
- Pagination
- Filtering
- Sorting
- API documentation using Swagger
- Unit and integration testing

---

## 7.2 Database Production Practices

- PostgreSQL
- Proper relationships
- Primary keys and foreign keys
- Constraints
- Indexes
- Transactions
- Concurrency handling
- Migrations
- Backup plan

---

## 7.3 DevOps Practices

- Dockerfile
- Docker Compose
- GitHub Actions CI/CD
- Build and test pipeline
- Docker image build
- Push image to container registry
- Deploy to testing environment
- Environment variables
- Secrets management
- Rollback using Docker image tags

---

## 7.4 Cloud and AWS Practices

The project may include important AWS services commonly used by companies:

```text
IAM
VPC
EC2
EBS
Security Groups
S3
Route 53
Application Load Balancer
Launch Template
Auto Scaling Group
CloudWatch
```

Optional AWS services:

```text
Lambda
RDS PostgreSQL
ElastiCache Redis
```

---

## 7.5 Kubernetes Practices

Kubernetes will be added later in the deployment stage.

Planned Kubernetes resources:

```text
Deployment
Service
Ingress
ConfigMap
Secret
Health checks
Readiness probe
Liveness probe
```

---

## 7.6 Real-Time and Caching Practices

Later stages may include:

```text
Redis
SignalR
Seat availability caching
Temporary reservation locks
Real-time seat updates
Real-time notifications
```

---

# 8. MVP User Flows

## 8.1 Flow 1: Seat-Based Event Booking

```text
EventOwner logs in.
EventOwner creates an event with BookingMode = Seats.
EventOwner creates an event session.
EventOwner creates seats for the session.
EventOwner publishes the event.
User views published events.
User opens event details.
User selects a session.
User views available seats.
User selects seats.
User creates a booking.
System creates a pending booking.
System reserves selected seats.
User simulates payment.
System confirms booking if payment succeeds.
System marks seats as booked.
System creates notification for the user.
```

---

## 8.2 Flow 2: Ticket-Based Event Booking

```text
EventOwner logs in.
EventOwner creates an event with BookingMode = Tickets.
EventOwner creates an event session.
EventOwner creates ticket types.
EventOwner publishes the event.
User views published events.
User opens event details.
User selects a session.
User views available ticket types.
User selects ticket type and quantity.
User creates a booking.
System creates a pending booking.
System reserves ticket quantity.
User simulates payment.
System confirms booking if payment succeeds.
System updates ticket availability.
System creates notification for the user.
```

---

## 8.3 Flow 3: Admin Monitoring

```text
Admin logs in.
Admin views users.
Admin views event owners.
Admin views events.
Admin views bookings.
Admin suspends an event if needed.
```

---

# 9. MVP Success Criteria

The MVP is considered complete when:

```text
User can register and login.
EventOwner can register and login.
Admin can login.
EventOwner can create and manage events.
EventOwner can create event sessions.
EventOwner can configure seats for seat-based events.
EventOwner can configure ticket types for ticket-based events.
User can view events and event details.
User can create seat-based booking.
User can create ticket-based booking.
User can simulate payment.
Successful payment confirms booking.
Cancelled or expired booking releases seats or ticket quantities.
User can view booking history.
Notifications are created for booking actions.
Admin can view users, events, and bookings.
```

---

# 10. Development Priority

Recommended development order:

## Phase 1: Core Setup

```text
Solution structure
Clean Architecture projects
PostgreSQL setup
ASP.NET Core Identity
JWT Authentication
Roles
```

## Phase 2: Events

```text
Event entity
Event CRUD for owner
Public event listing
Event details
```

## Phase 3: Sessions

```text
Event sessions
Session CRUD for owner
Public session listing
```

## Phase 4: Seats and Ticket Types

```text
Seat management
Ticket type management
Booking mode rules
```

## Phase 5: Bookings

```text
Seat-based booking
Ticket-based booking
Booking status
Booking expiration
Cancel booking
```

## Phase 6: Payments

```text
Payment simulation
Confirm booking after payment
Failed payment handling
```

## Phase 7: Notifications and Admin

```text
Database notifications
Admin monitoring endpoints
Suspend event
```

## Phase 8: Production-Wise Enhancements

```text
Docker
GitHub Actions
Testing environment deployment
Serilog
Health checks
Swagger documentation
```

## Phase 9: Advanced Learning Scope

```text
Redis
SignalR
AWS services
Kubernetes
Monitoring
Auto Scaling
Load Balancer
S3 uploads
CloudWatch
```

---

# 11. Final MVP Summary

The MVP is the first working version of the Ticketing Platform.

It should focus on:

```text
Auth
Roles
Events
Sessions
Seats
Ticket Types
Bookings
Payment Simulation
Notifications
Admin Monitoring
```

The extended project will then add production-wise and DevOps features to demonstrate real-world skills.

The final goal is not only to build a working API, but to build a complete project that can be discussed confidently in backend, DevOps, and cloud interviews.
