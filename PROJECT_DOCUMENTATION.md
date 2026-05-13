# Ticketing Platform — Project Documentation

This document summarizes solution structure, layers, functional areas, booking behavior, realtime updates, concurrency, configuration, and operations.

---

## 1. Overview

A ticketing platform for **events** with:

- **End user (`User`):** Browse published events, sessions, seats or ticket types; create bookings; cancel; list “my bookings.”
- **Event owner (`EventOwner`):** Manage events, sessions, seats, ticket types, publish/cancel workflows, dashboard.
- **Authentication:** ASP.NET Core Identity + JWT (access and refresh tokens).
- **Database:** PostgreSQL for dev/production; **SQLite in-memory** for integration tests.
- **Realtime:** SignalR notifies clients in the same **event session** when seat or ticket availability changes (after a successful booking or cancellation).

**Stack:** .NET 10  
**Architecture:** Clean separation of `Domain`, `Application`, `Infrastructure`, and `API`.

---

## 2. Solution Structure

| Path | Description |
|------|-------------|
| `TicketingPlatform.slnx` | Solution file |
| `src/Ticketing.Domain` | Entities, enums; no EF or web dependencies |
| `src/Ticketing.Application` | Service contracts, DTOs, shared responses (`ApiResponse`, `ApiPagedResponse`) |
| `src/Ticketing.Infrastructure` | EF Core, Identity, service implementations, email, SignalR |
| `src/Ticketing.API` | ASP.NET Core Web API, `Program.cs`, controllers |
| `tests/Ticketing.IntegrationTests` | API integration tests with in-memory SQLite |

---

## 3. Domain Layer

### 3.1 Entities (`Ticketing.Domain/Entities`)

| Entity | Summary |
|--------|---------|
| `Event` | Event: name, description, `EventStatus`, `EventBookingMode` (seats vs tickets), owner |
| `EventSession` | Session under an event: times, location, session status |
| `Seat` | Seat in a session: number, type, `SeatStatus`, price |
| `TicketType` | Ticket type in a session: price, quantities, type status |
| `Booking` | Booking: user, session, amount, booking status, booking number |
| `BookingTicketSeat` | Booking ↔ seat link (price at booking time) |
| `BookingTicketType` | Booking ↔ ticket type link (quantity, unit price) |
| `Payment` | Payment (placeholder for extension) |
| `Notification` | Notification (placeholder for extension) |

### 3.2 Important Enums (`Ticketing.Domain/Enums`)

- `EventStatus` — e.g. Draft, Published, Cancelled  
- `EventBookingMode` — `Seats` | `Tickets`  
- `SessionStatus` — e.g. Scheduled  
- `SeatStatus` — Available, Reserved, Booked, Disabled  
- `TicketTypeStatus` — e.g. Active  
- `BookingStatus` — e.g. Confirmed, Pending, Cancelled  
- `PaymentStatus`, `PaymentMethod` — reserved for payments  

---

## 4. Application Layer

### 4.1 Interfaces

- **`IAuthService`** — Register, login, refresh token, email confirmation, password reset.
- **`IUserService`** — Published events (filtering + paging), sessions, seats, ticket types.
- **`IBookingService`** — Seat/ticket booking, cancel, my bookings (paging + filters), booking details.
- **`IEventOwnerService`** — Owner CRUD for events, sessions, seats, ticket types, publish/cancel, dashboard.
- **`IUnitOfWork`** — Repositories, `SaveChanges`, transactions, **atomic booking helpers** (see §7).
- **`IEmailService`**, **`IJwtTokenGenerator`** — Email and token issuance.

### 4.2 Standard Responses

- **`ApiResponse<T>`** — `Success`, `Message`, `Data`, `Errors`.
- **`ApiPagedResponse<T>`** — `PageNumber`, `PageSize`, `TotalCount`, `Items`.

### 4.3 DTO Groups (high level)

- **Auth:** `RegisterDto`, `LoginDto`, `AuthTokensDto`, `RefreshTokenRequestDto`, etc.
- **Booking:** `CreateSeatTypeBookingDto`, `CreateTicketTypeBookingDto`, `MyBookingsFilterDto`, `BookingDto`, `BookingDetailsDto`, …
- **User browsing:** `EventDto`, `UserEventsFilterDto`, `SessionsDto`, `SeatsDto`, `TicketTypeDto`
- **EventOwner:** Create/update event, sessions, seats, tickets, dashboard filters, …

---

## 5. Infrastructure Layer

### 5.1 Persistence

- **`ApplicationDbContext`** — `DbSet` for all entities + Identity.
- **Configurations:** `Persistence/Configurations/*.cs` (table names, relationships, indexes, decimal precision).
- **`UnitOfWork`** — Wraps `DbContext`, exposes `IBaseRepository<T>` per aggregate set, manages transactions.
- **`BaseRepository<T>`** — Basic CRUD over `DbSet`.
- **Migrations:** `Persistence/Migrations/*` — applied on startup except in the **`Testing`** environment.

### 5.2 Atomic Booking Operations (`UnitOfWork`)

These run inside the **same EF transaction** started by `BookingService`:

1. **`TryAtomicallyBookSeatsAsync(seatIds, eventSessionId)`**  
   - Uses `ExecuteUpdateAsync` to set seats from `Available` → `Booked` **only if** they belong to the session and are available.  
   - Succeeds only when **updated row count equals** the number of **distinct** requested seat IDs.  
   - Prevents two users from successfully booking the same seat at the same time (row-level conditional update).

2. **`TryAtomicallyReserveTicketTypesAsync(eventSessionId, lines)`**  
   - For each `(TicketTypeId, Quantity)` line: atomic update subtracts `AvailableQuantity` **only if** the type is active, belongs to the session, and stock is sufficient.  
   - If any line does not update **exactly one** row, returns `false`.  
   - Prevents overselling when concurrent requests compete for the same inventory.

### 5.3 Implemented Services

- **`AuthService`** — Identity, roles, JWT, confirmation/reset emails via `IEmailService`.
- **`UserService`** — Queries for published events, sessions, seats, ticket types (status and booking-mode rules).
- **`BookingService`** — Full booking flow, SignalR after success, cancellation with inventory restore.
- **`EventOwnerService`** — Owner-facing management.
- **`EmailService`** — MailKit.
- **`JwtTokenGenerator`** — Access/refresh tokens per settings.

### 5.4 SignalR (Realtime)

- **`BookingNotificationHub`**  
  - Route: **`/hubs/booking`** (`BookingNotificationHub.Path`).  
  - **`[Authorize]`** — JWT required.  
  - Browser clients often connect with `.../hubs/booking?access_token=<JWT>` (wired in `Program.cs` via `JwtBearerEvents.OnMessageReceived`).  
  - **`JoinSession(sessionId)` / `LeaveSession(sessionId)`** — Join group `session-{sessionId}` to receive updates for that session only.

- **Server → client messages (after successful `Commit`):**
  - **`ReceiveSeatBookingUpdate`** — Payload: `SeatBookingRealtimeMessage` (`SessionId`, `SeatIds`, string `Status` e.g. `Booked` / `Available`).
  - **`ReceiveTicketAvailabilityUpdate`** — Payload: `TicketAvailabilityRealtimeMessage` (`SessionId`, `Updates` with `TicketTypeId`, `AvailableQuantity`).

- **Message types:** `Infrastructure/RealTime/BookingRealtimeMessages.cs`.

### 5.5 Dependency Injection

- **`AddInfrastructure`** — Npgsql, Identity, JWT/URL/email options, **SignalR**, service registrations, `IUnitOfWork`.  
- **No external cache service** in the current codebase; any caching layer remains optional and planned only.

---

## 6. API Layer (`Ticketing.API`)

### 6.1 Pipeline (`Program.cs`)

1. `AddApplication` + `AddInfrastructure`
2. Controllers, Swagger, JWT Bearer (+ `access_token` query for the Hub path)
3. CORS policy **`WebClient`** — from `Cors:AllowedOrigins`, or localhost in Development, or `AllowAnyOrigin` in Production when no origins are configured
4. `Migrate` + role/user/test data seeding (skipped in **`Testing`**)
5. `UseCors` → `UseAuthentication` → `UseAuthorization`
6. `MapControllers`
7. `MapHub<BookingNotificationHub>(Path).RequireCors("WebClient")`

### 6.2 Controllers

| Controller | Base route | Notes |
|------------|------------|--------|
| `AuthController` | `api/auth` | Mostly anonymous auth endpoints |
| `EventOwnerController` | `api/event-owner` | `[Authorize(Roles = "EventOwner")]` |
| `UserController` | `api/user` | `[Authorize(Roles = "User")]` — browse + book |

**`UserController` examples:**

- `GET api/user/events` — Published events, **paging + filters** (`UserEventsFilterDto`)
- `GET api/user/events/{id}`, `.../sessions`, `sessions/{id}/seats`, `sessions/{id}/ticket-types`
- `POST api/user/bookings/seat-type`, `POST .../ticket-type`
- `DELETE api/user/bookings/{bookingId}` — cancel
- `GET api/user/bookings` — my bookings, **paging + filters** (`MyBookingsFilterDto`)
- `GET api/user/bookings/{id}` — details

### 6.3 Configuration (`appsettings`)

- **`ConnectionStrings:DefaultConnection`** — PostgreSQL
- **`JwtSettings`** — SecretKey, Issuer, Audience, access lifetime, refresh settings
- **`AppUrls:PublicBaseUrl`** — Links in emails/pages
- **`EmailSettings`** — SMTP
- **`Cors:AllowedOrigins`** — Optional string array for SPA origins

---

## 7. Booking Module — Detailed Behavior

### 7.1 Transactions (All-or-Nothing)

- Each booking/cancel flow starts with **`BeginTransactionAsync`**.
- On success: **`SaveChangesAsync`** then **`CommitTransactionAsync`**.
- On failure or exception: **`RollbackTransactionAsync`** — no partial committed state for that unit of work.

### 7.2 Seat booking (`CreateSeatTypeBookingAsync`)

1. Validate session/event (published, scheduled, not started, seats mode, …).
2. **`TryAtomicallyBookSeatsAsync`** — on failure, return “not available.”
3. Reload seats (e.g. `AsNoTracking`) for prices (rows are already `Booked`).
4. Create `Booking` and `BookingTicketSeat` rows.
5. Save and commit.
6. **SignalR** to the session group with updated seats.

### 7.3 Ticket booking (`CreateTicketTypeBookingAsync`)

1. Validate session and DTO (no duplicate types, positive quantities).
2. **`TryAtomicallyReserveTicketTypesAsync`** — atomic decrement; on failure, not available/inactive.
3. Reload ticket types for prices and post-decrement quantities.
4. Create `Booking` and `BookingTicketType` (FK via `TicketTypesId`).
5. Save and commit.
6. **SignalR** with per-type availability updates.

### 7.4 Cancellation (`CancelBookingAsync`)

- Rules: booking belongs to user, not already cancelled, session not started, status allows cancel.
- Restore seats to `Available` or add back `AvailableQuantity` on ticket types.
- Commit, then **SignalR** (seat available or ticket quantities).

### 7.5 Concurrency — Summary

- **Same seat:** Two concurrent requests cannot both succeed; one conditional update wins, the other fails and rolls back.
- **Last ticket (quantity 1):** Same idea via atomic decrement with `AvailableQuantity >= quantity`.

### 7.6 Realtime — Client Requirements

1. Connect to the Hub with JWT (`access_token` in the query when needed).
2. After connect: call **`JoinSession(sessionId)`** for the session being viewed.
3. Handle **`ReceiveSeatBookingUpdate`** and **`ReceiveTicketAvailabilityUpdate`** and update the UI (no full page refresh required).

---

## 8. Identity Roles

- **`User`** — `api/user` surface.
- **`EventOwner`** — `api/event-owner` surface.
- **`Admin`** — Seeded; extend with explicit policies as needed.

Registration can specify a role (default `User` in `RegisterDto`).

---

## 9. Docker

- **`docker-compose.yml`:** `ticketing-postgres` + `ticketing-api` (depends on PostgreSQL).
- Environment variables for DB, JWT, email from an env file or host (`DB_CONNECTION_STRING`, `JWT_SECRET_KEY`, `EMAIL_*`, …).
- **`Dockerfile`:** Build and publish `Ticketing.API` on port 8080.

---

## 10. Tests

- **`Ticketing.IntegrationTests`** — `WebApplicationFactory`, **`Testing`** environment, in-memory SQLite, **no** automatic migrations from `Program` (uses `EnsureCreated`).
- Covers auth and owner/user API paths as implemented in the test suite.

---

## 11. Other Repository Docs

- **`Docs/`** and **`README.md`** may describe planned or historical features. **Current code behavior** is the source of truth alongside this file.

---

## 12. Quick Start for New Developers

1. Run `src/Ticketing.API` with PostgreSQL and JWT/email settings configured.
2. **Domain** lives in `Ticketing.Domain` — no web references.
3. **Contracts and DTOs** in `Ticketing.Application`.
4. **Implementations** in `Ticketing.Infrastructure`, including atomic booking in `UnitOfWork` and SignalR.
5. **Safe concurrency** = conditional `ExecuteUpdateAsync` inside a transaction, not read-then-update in memory alone.
6. **Realtime** = Hub + per-session groups + notifications only **after** commit.

---

*Last updated to match the current repository state: no external cache service; SignalR enabled; atomic seat and ticket-type booking as described above.*
