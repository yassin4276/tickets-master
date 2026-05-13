# Project Requirements

## 1. Project Overview

### Project Name
Real-Time Event Booking & Ticketing System

### Project Description
A marketplace-style event booking system similar to a mini Ticketmaster with an Amazon-like ownership model. Event owners can create and manage their own events, users can browse events and book seats, and admins monitor the whole platform without directly owning or managing event content by default.

The system focuses on solving real-world backend challenges such as concurrency, real-time updates, performance optimization, caching, deployment, and DevOps automation.

### Main Goal
Build a production-style backend project using ASP.NET Core and modern DevOps practices to demonstrate strong backend engineering, system design, ownership-based authorization, marketplace logic, and deployment skills.

### Target Users
The system has three main user types:

#### 1. User
A normal customer who can browse events, view seats, reserve seats, create bookings, and simulate payments.

#### 2. Event Owner
A seller-like user who can create events, manage their own events, manage seats for their events, and track bookings related only to their events.

This role is similar to an Amazon seller who lists products, while customers purchase from those listings.

#### 3. Admin
A platform-level user who monitors the system, users, event owners, events, bookings, and platform activity. The admin does not normally create events. The admin mainly supervises, reviews, audits, and can take moderation actions when needed.

### Project Type
Modular Monolith using Clean Architecture.

### Why Modular Monolith?
- Lower complexity than microservices.
- Easier to develop and maintain during the MVP phase.
- Clear separation between modules.
- Can be migrated to microservices later if needed.

### Core Modules
- Users
- Event Owners
- Events
- Seats
- Booking
- Payments
- Notifications
- Admin Monitoring

---

## 2. Functional Requirements

### 2.1 Authentication & Authorization

#### User Registration
The system shall allow normal users to create an account using basic personal information.

#### Event Owner Registration
The system shall allow event owners to create an account or request an event owner profile.

#### User Login
The system shall allow users, event owners, and admins to log in using email and password.

#### JWT Authentication
The system shall issue a JWT token after successful login.

#### Role-Based Access
The system shall support at least three roles:
- User
- EventOwner
- Admin

#### Ownership-Based Authorization
The system shall ensure that event owners can only manage events, seats, and bookings related to their own events.

For example, Event Owner A must not be able to update, delete, or view private booking details for Event Owner B's events.

#### Admin Access
Admins shall have platform-level monitoring access. Admins can review users, event owners, events, bookings, and system activity. Admins may also have moderation permissions such as suspending events or accounts if needed.

---

### 2.2 Event Owner Management

#### Create Event Owner Profile
The system shall allow a user to become an event owner or allow the platform to create an event owner account.

#### View Owner Dashboard
The system shall allow event owners to view a dashboard for their own events, bookings, revenue simulation, and seat availability.

#### Track Owner Events
The system shall allow event owners to track only the events they created.

#### Track Owner Bookings
The system shall allow event owners to view bookings related only to their own events.

---

### 2.3 Event Management

#### Create Event
The system shall allow event owners to create new events.

#### Update Event
The system shall allow event owners to update only their own events.

#### Delete or Deactivate Event
The system shall allow event owners to delete or deactivate only their own events based on business rules.

#### List Events
The system shall allow users to view a list of available published events.

#### Event Details
The system shall allow users to view event details, including date, venue, description, event owner information, and available seats.

#### Admin Event Monitoring
The system shall allow admins to monitor all events across the platform.

#### Event Moderation
The system may allow admins to suspend, hide, or reject events if they violate platform rules.

---

### 2.4 Seat Management

#### Seat Creation
The system shall allow event owners to define seats for their own events.

#### Seat Availability
The system shall allow users to view available, reserved, and booked seats for an event.

#### Seat Status
Each seat shall have a status such as:
- Available
- Reserved
- Booked

#### Prevent Double Booking
The system shall prevent two users from booking the same seat at the same time.

#### Owner Seat Management
Event owners shall only manage seats that belong to their own events.

---

### 2.5 Booking System

#### Create Booking
The system shall allow users to create a booking for one or more seats.

#### Temporary Seat Reservation
The system shall temporarily reserve selected seats while the user completes the booking process.

#### Booking Confirmation
The system shall confirm the booking after successful payment simulation.

#### Booking Cancellation
The system shall allow users to cancel a booking based on business rules.

#### View User Bookings
The system shall allow users to view their previous and active bookings.

#### Event Owner Booking Tracking
The system shall allow event owners to view bookings made for their own events only.

#### Admin Booking Monitoring
The system shall allow admins to monitor all bookings across the platform.

---

### 2.6 Payment Simulation

#### Simulate Payment
The system shall simulate a payment process without integrating a real payment gateway in the MVP.

#### Payment Status
The system shall track payment status such as:
- Pending
- Paid
- Failed

#### Booking-Payment Link
Each confirmed booking shall be linked to a payment record.

#### Owner Revenue Simulation
The system may calculate simulated revenue for event owners based on confirmed bookings.

---

### 2.7 Real-Time Seat Updates

#### SignalR Updates
The system shall use SignalR to notify connected clients when a seat status changes.

#### Live Seat Availability
When a user reserves or books a seat, other users shall see the updated seat status in real time.

#### Owner Live Tracking
Event owners may see real-time updates for bookings and seat status related to their own events.

---

### 2.8 Optional distributed caching (planned)

#### Seat availability cache (future)

If a cache is introduced later, it may speed up seat availability reads. **This project does not use an external cache service today.**

#### Cache invalidation (future)

If caching is added, cached seat data must be updated or invalidated when seat status changes.

#### Event listing cache (future)

Public event listings may be cached later for performance.

---

### 2.9 Admin Monitoring

#### Monitor Users
Admins shall be able to view registered users.

#### Monitor Event Owners
Admins shall be able to view event owner accounts and their activity.

#### Monitor Events
Admins shall be able to monitor events created by all event owners.

#### Monitor Bookings
Admins shall be able to monitor bookings across the whole platform.

#### Moderation Actions
Admins may be able to:
- Suspend an event.
- Suspend a user account.
- Suspend an event owner account.
- Review suspicious booking activity.

---

## 3. Non-Functional Requirements

### 3.1 Performance
- The system should respond quickly to event and seat availability requests.
- An optional distributed cache may be introduced later to reduce database load for frequent seat lookups; it is **not** part of the current implementation.
- PostgreSQL indexes should be added on frequently queried columns.

### 3.2 Scalability
- The system should be containerized using Docker.
- The system should be deployable to Kubernetes.
- The architecture should support future scaling of API instances.

### 3.3 Reliability
- The booking process should use database transactions.
- The system should handle concurrent booking attempts safely.
- Failed payment simulations should not confirm bookings.

### 3.4 Maintainability
- The backend should follow Clean Architecture.
- Business logic should be separated from infrastructure concerns.
- Code should be organized into clear modules and layers.

### 3.5 Security
- Passwords must be hashed before storage.
- APIs must be protected using JWT authentication.
- Admin endpoints must require admin authorization.
- Event owner endpoints must require ownership validation.
- Secrets must not be stored in source control.

### 3.6 Authorization Rules
- Users can only manage their own bookings.
- Event owners can only manage their own events, seats, and event-related bookings.
- Admins can monitor platform data and perform moderation actions.
- Public users can only view published events.

### 3.7 Observability
- The system should include structured logging.
- A health check endpoint should be provided.
- Monitoring can be added later using tools such as Prometheus and Grafana.

### 3.8 DevOps & Deployment
- The system should include a Dockerfile.
- Docker Compose should be used for local development.
- GitHub Actions should be used for CI/CD.
- Kubernetes manifests should be added later.
- Terraform should be used later for AWS infrastructure.

### 3.9 Documentation
- The project should include clear README documentation.
- API endpoints should be documented in Markdown.
- Architecture decisions should be recorded.
- Diagrams should be created for architecture and database design.

---

## 4. MVP Scope

### MVP Features
The first version shall include:
- Authentication
- Three roles: User, EventOwner, Admin
- Event owner event creation
- Event listing and event details
- Seat listing
- Seat reservation
- Booking creation
- Payment simulation
- Preventing double booking using SQL transactions and concurrency handling
- Ownership validation for event owner actions
- Basic admin monitoring endpoints

### Post-MVP Features
The following features can be added after the MVP:
- Optional distributed caching (planned, not in current codebase)
- SignalR real-time updates
- Event owner dashboard analytics
- Admin moderation workflow
- Docker CI/CD pipeline
- Kubernetes deployment
- AWS deployment
- Terraform infrastructure
- Monitoring and logging stack

---

## 5. Assumptions

- The system will initially be backend-focused.
- Payment will be simulated, not connected to a real payment provider.
- The frontend can be added later or tested through Postman.
- Event owners are responsible for creating and managing their own events.
- Admins mainly monitor and moderate the platform.
- The first deployment can be done using Docker Compose before Kubernetes.
- PostgreSQL will be the main source of truth.
- If a distributed cache is adopted later, it will be auxiliary only — PostgreSQL remains the primary database and source of truth.

---

## 6. Success Criteria

The project is considered successful when:
- Users can register and log in.
- Event owners can create and manage their own events.
- Users can browse published events.
- Users can view seats for an event.
- Users can reserve and book seats.
- The system prevents double booking.
- Event owners can track bookings for their own events.
- Admins can monitor users, event owners, events, and bookings.
- Ownership-based authorization is applied correctly.
- The project has clear documentation, diagrams, and deployment planning.
- The system is prepared for Docker, CI/CD, Kubernetes, AWS, and Terraform in later stages.

