# Ticketing Platform - Project Structure Planning

## 1. Overview

This document defines the planned project structure for the Ticketing Platform.

The project will follow **Clean Architecture** to keep the business logic separated from external concerns such as API controllers, database access, authentication, file storage, Redis, SignalR, and deployment details.

The solution will be organized into four main projects:

```text
Ticketing.API
Ticketing.Application
Ticketing.Domain
Ticketing.Infrastructure
```

The goal is to make the project:

- Maintainable
- Testable
- Scalable
- Easy to understand
- Suitable for real-world backend development
- Suitable for future DevOps deployment

---

## 2. Solution Structure

Recommended root structure:

```text
TicketingPlatform/
│
├── src/
│   ├── Ticketing.API/
│   ├── Ticketing.Application/
│   ├── Ticketing.Domain/
│   └── Ticketing.Infrastructure/
│
├── tests/
│   ├── Ticketing.Application.Tests/
│   └── Ticketing.API.Tests/
│
├── docs/
│   ├── Project Requirements.md
│   ├── High-Level Architecture.md
│   ├── Database Design.md
│   ├── API Design.md
│   ├── Architecture Decisions.md
│   ├── DevOps Planning.md
│   └── Project Structure.md
│
├── docker-compose.yml
├── Dockerfile
├── README.md
├── .gitignore
└── TicketingPlatform.sln
```

---

## 3. Layer Responsibilities

## 3.1 Ticketing.Domain

The Domain layer contains the core business concepts of the system.

It should not depend on any other project.

### Responsibilities

- Entities
- Enums
- Value Objects if needed
- Domain rules
- Base entity classes
- Core business concepts

### Should Not Contain

The Domain layer should not contain:

```text
Controllers
Entity Framework DbContext
Database migrations
HTTP logic
Redis logic
SignalR logic
S3 logic
External service implementation
```

### Suggested Structure

```text
Ticketing.Domain/
│
├── Common/
│   └── BaseEntity.cs
│
├── Entities/
│   ├── Event.cs
│   ├── EventSession.cs
│   ├── Seat.cs
│   ├── TicketType.cs
│   ├── Booking.cs
│   ├── BookingSeat.cs
│   ├── BookingTicketType.cs
│   ├── Payment.cs
│   └── Notification.cs
│
├── Enums/
│   ├── UserRole.cs
│   ├── EventStatus.cs
│   ├── BookingMode.cs
│   ├── SessionStatus.cs
│   ├── SeatStatus.cs
│   ├── TicketTypeStatus.cs
│   ├── BookingStatus.cs
│   ├── PaymentStatus.cs
│   └── NotificationType.cs
│
└── ValueObjects/
    └── Money.cs
```

### Notes

`ValueObjects/Money.cs` is optional for the MVP.

For the first version, using decimal values directly for prices and amounts is acceptable.

---

## 3.2 Ticketing.Application

The Application layer contains the use cases and application business logic.

It depends only on the Domain layer.

### Responsibilities

- DTOs
- Application services
- Use cases
- Interfaces
- Validation
- Authorization-related application rules
- Result wrappers
- Pagination models

### Should Not Contain

The Application layer should not contain:

```text
Controllers
EF Core DbContext implementation
PostgreSQL implementation
Redis implementation
SignalR implementation
S3 implementation
```

### Suggested Structure

```text
Ticketing.Application/
│
├── Common/
│   ├── Result.cs
│   ├── PaginatedResult.cs
│   └── Error.cs
│
├── DTOs/
│   ├── Auth/
│   │   ├── RegisterRequest.cs
│   │   ├── LoginRequest.cs
│   │   └── AuthResponse.cs
│   │
│   ├── Events/
│   │   ├── CreateEventRequest.cs
│   │   ├── UpdateEventRequest.cs
│   │   └── EventResponse.cs
│   │
│   ├── Sessions/
│   │   ├── CreateEventSessionRequest.cs
│   │   └── EventSessionResponse.cs
│   │
│   ├── Seats/
│   │   ├── CreateSeatsRequest.cs
│   │   └── SeatResponse.cs
│   │
│   ├── TicketTypes/
│   │   ├── CreateTicketTypeRequest.cs
│   │   └── TicketTypeResponse.cs
│   │
│   ├── Bookings/
│   │   ├── CreateSeatBookingRequest.cs
│   │   ├── CreateTicketBookingRequest.cs
│   │   ├── BookingResponse.cs
│   │   └── BookingDetailsResponse.cs
│   │
│   ├── Payments/
│   │   ├── SimulatePaymentRequest.cs
│   │   └── PaymentResponse.cs
│   │
│   └── Notifications/
│       └── NotificationResponse.cs
│
├── Interfaces/
│   ├── IApplicationDbContext.cs
│   ├── ICurrentUserService.cs
│   ├── IAuthService.cs
│   ├── IEventService.cs
│   ├── IBookingService.cs
│   ├── IPaymentService.cs
│   ├── INotificationService.cs
│   └── IFileStorageService.cs
│
├── Services/
│   ├── AuthService.cs
│   ├── EventService.cs
│   ├── BookingService.cs
│   ├── PaymentService.cs
│   └── NotificationService.cs
│
├── Validators/
│   ├── RegisterRequestValidator.cs
│   ├── LoginRequestValidator.cs
│   ├── CreateEventRequestValidator.cs
│   ├── CreateSeatBookingRequestValidator.cs
│   └── CreateTicketBookingRequestValidator.cs
│
└── DependencyInjection.cs
```

---

## 3.3 Ticketing.Infrastructure

The Infrastructure layer contains implementations for external services and persistence.

It depends on:

```text
Ticketing.Application
Ticketing.Domain
```

### Responsibilities

- PostgreSQL database access
- EF Core DbContext
- Entity configurations
- Migrations
- ASP.NET Core Identity implementation
- Repository implementations if used
- File storage implementation
- Redis implementation later
- SignalR infrastructure later
- External services

### Suggested Structure

```text
Ticketing.Infrastructure/
│
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   │   ├── EventConfiguration.cs
│   │   ├── EventSessionConfiguration.cs
│   │   ├── SeatConfiguration.cs
│   │   ├── TicketTypeConfiguration.cs
│   │   ├── BookingConfiguration.cs
│   │   ├── BookingSeatConfiguration.cs
│   │   ├── BookingTicketTypeConfiguration.cs
│   │   ├── PaymentConfiguration.cs
│   │   └── NotificationConfiguration.cs
│   └── Migrations/
│
├── Identity/
│   ├── ApplicationUser.cs
│   └── IdentityService.cs
│
├── Storage/
│   └── S3FileStorageService.cs
│
├── Caching/
│   └── RedisCacheService.cs
│
├── Realtime/
│   └── SignalRNotificationService.cs
│
└── DependencyInjection.cs
```

### MVP Notes

For the MVP, `Caching/` and `Realtime/` can be created later.

Initial implementation can focus on:

```text
Persistence
Identity
DependencyInjection.cs
```

---

## 3.4 Ticketing.API

The API layer is the entry point of the application.

It depends on:

```text
Ticketing.Application
Ticketing.Infrastructure
```

### Responsibilities

- HTTP controllers
- Request handling
- Authentication setup
- Authorization setup
- Swagger setup
- Health checks
- Exception middleware
- CORS configuration
- Dependency injection registration
- Application startup

### Suggested Structure

```text
Ticketing.API/
│
├── Controllers/
│   ├── AuthController.cs
│   ├── EventsController.cs
│   ├── OwnerEventsController.cs
│   ├── EventSessionsController.cs
│   ├── SeatsController.cs
│   ├── TicketTypesController.cs
│   ├── BookingsController.cs
│   ├── PaymentsController.cs
│   ├── NotificationsController.cs
│   └── AdminController.cs
│
├── Middlewares/
│   └── ExceptionHandlingMiddleware.cs
│
├── Extensions/
│   ├── AuthenticationExtensions.cs
│   ├── AuthorizationExtensions.cs
│   ├── SwaggerExtensions.cs
│   ├── CorsExtensions.cs
│   └── HealthCheckExtensions.cs
│
├── Services/
│   └── CurrentUserService.cs
│
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── appsettings.Testing.json
```

---

## 4. Project Dependencies

Dependency direction must follow Clean Architecture rules.

### Allowed Dependencies

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

### Important Rule

The Domain layer must not depend on any other layer.

The Application layer must not depend on API or Infrastructure implementation details.

---

## 5. Dependency Diagram

```text
                ┌──────────────────────┐
                │    Ticketing.API      │
                └──────────┬───────────┘
                           │
                           ▼
                ┌──────────────────────┐
                │ Ticketing.Application │
                └──────────┬───────────┘
                           │
                           ▼
                ┌──────────────────────┐
                │   Ticketing.Domain    │
                └──────────────────────┘


                ┌──────────────────────┐
                │ Ticketing.Infrastructure │
                └──────────┬───────────┘
                           │
                           ▼
                ┌──────────────────────┐
                │ Ticketing.Application │
                └──────────┬───────────┘
                           │
                           ▼
                ┌──────────────────────┐
                │   Ticketing.Domain    │
                └──────────────────────┘
```

### Practical Dependency Setup

In the `.NET solution`:

```text
Ticketing.API references:
- Ticketing.Application
- Ticketing.Infrastructure

Ticketing.Application references:
- Ticketing.Domain

Ticketing.Infrastructure references:
- Ticketing.Application
- Ticketing.Domain
```

---

## 6. Entity ID Strategy

The project should use `Guid` identifiers.

Recommended approach:

```csharp
public Guid Id { get; set; }
```

For ASP.NET Core Identity, use:

```csharp
public class ApplicationUser : IdentityUser<Guid>
{
}
```

This keeps identity IDs consistent with the rest of the system.

---

## 7. Configuration Files

The API project should contain environment-specific configuration files:

```text
appsettings.json
appsettings.Development.json
appsettings.Testing.json
```

### appsettings.json

Contains shared non-sensitive configuration.

### appsettings.Development.json

Contains local development configuration.

### appsettings.Testing.json

Contains testing environment configuration.

### Secrets

Sensitive values must not be stored in appsettings files committed to Git.

Examples of sensitive values:

```text
Database password
JWT secret
AWS access keys
S3 secret key
```

Use:

```text
.env
dotnet user-secrets
GitHub Secrets
Kubernetes Secrets
```

---

## 8. Testing Structure

The solution should include a `tests/` folder.

Recommended test projects:

```text
tests/
├── Ticketing.Application.Tests/
└── Ticketing.API.Tests/
```

### Ticketing.Application.Tests

Focuses on application logic and business rules.

Examples:

- Creating seat-based booking.
- Preventing duplicate seat booking.
- Preventing booking with insufficient ticket quantity.
- Expiring pending bookings.
- Simulating payment.

### Ticketing.API.Tests

Focuses on API behavior.

Examples:

- Auth endpoints.
- Authorization rules.
- Booking endpoint responses.
- Validation errors.
- Status codes.

---

## 9. Documentation Structure

All documentation files should be stored inside the `docs/` folder.

Recommended structure:

```text
docs/
├── Project Requirements.md
├── High-Level Architecture.md
├── Database Design.md
├── API Design.md
├── Architecture Decisions.md
├── DevOps Planning.md
└── Project Structure.md
```

---

## 10. Naming Conventions

### Projects

Use PascalCase:

```text
Ticketing.API
Ticketing.Application
Ticketing.Domain
Ticketing.Infrastructure
```

### Classes

Use PascalCase:

```text
BookingService
CreateEventRequest
BookingResponse
ApplicationDbContext
```

### Interfaces

Start interfaces with `I`:

```text
IBookingService
IPaymentService
ICurrentUserService
IFileStorageService
```

### DTOs

Use clear suffixes:

```text
CreateEventRequest
UpdateEventRequest
EventResponse
BookingDetailsResponse
```

### Controllers

Use plural resource names where possible:

```text
EventsController
BookingsController
PaymentsController
NotificationsController
```

For owner-specific resources:

```text
OwnerEventsController
```

---

## 11. MVP Project Structure

For the MVP, the project can start with the following minimum structure:

```text
src/
├── Ticketing.API/
│   ├── Controllers/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── appsettings.Testing.json
│
├── Ticketing.Application/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Services/
│   └── Common/
│
├── Ticketing.Domain/
│   ├── Entities/
│   ├── Enums/
│   └── Common/
│
└── Ticketing.Infrastructure/
    ├── Persistence/
    ├── Identity/
    └── DependencyInjection.cs
```

---

## 12. Future Folders

The following folders can be added later when needed:

```text
Ticketing.Infrastructure/Caching
Ticketing.Infrastructure/Realtime
Ticketing.Infrastructure/Storage
Ticketing.API/Hubs
Ticketing.API/BackgroundJobs
```

### Future Features

- Redis caching
- SignalR real-time updates
- S3 file storage
- Background job for expired bookings
- Cloud deployment configuration
- Kubernetes manifests

---

## 13. Final Notes

The structure should not be over-engineered at the beginning.

The MVP should start simple, but the solution should be organized in a way that allows future growth.

The most important rules are:

- Keep business logic out of controllers.
- Keep database implementation out of the Domain layer.
- Keep external services behind interfaces.
- Keep dependencies pointing inward.
- Keep documentation updated as the project changes.
