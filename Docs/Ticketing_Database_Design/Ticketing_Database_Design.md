# Ticketing Platform - Database Design

## 1. Overview

This database design supports a ticketing platform with three main roles:

- **User**: browses events and creates bookings.
- **Event Owner**: creates and manages their own events, sessions, seats, and ticket types.
- **Admin**: monitors the platform without owning the booking flow.

The system supports two booking modes:

1. **Seat-based booking**: the user selects one or more specific seats.
2. **Ticket-based booking**: the user selects a ticket type and quantity, without choosing specific seats.

The design is built around `Events`, `EventSessions`, `Bookings`, and booking detail tables.

---

## 2. Core Business Rules

### Booking Mode Rule

Each event has one booking mode:

- `Seats`: bookings are created through `BookingSeats`.
- `Tickets`: bookings are created through `BookingTicketTypes`.

A single booking should not contain both `BookingSeats` and `BookingTicketTypes`.

### Event Session Rule

A user books a specific `EventSession`, not just the abstract event. This allows one event to have multiple dates, times, or locations.

### Price Snapshot Rule

`BookingSeats.Price` and `BookingTicketTypes.UnitPrice` store the price at the time of booking. This protects historical bookings if the event owner changes prices later.

### Seat Concurrency Rule

A seat cannot be part of more than one active booking at the same time. Active statuses are usually:

- `Pending`
- `Confirmed`

Expired, cancelled, or failed bookings should release the seat.

---

## 3. Entities

### Users

Stores platform users. Roles can be handled using ASP.NET Core Identity.

| Column | Type Suggestion | Notes |
|---|---|---|
| Id | uuid / bigint | Primary key |
| FirstName | varchar | Required |
| LastName | varchar | Required |
| Email | varchar | Required, unique |
| PasswordHash | text | If not fully delegated to ASP.NET Identity |
| PhoneNumber | varchar | Optional or unique depending on requirements |
| IsActive | boolean | Default true |
| CreatedAt | timestamp | Required |

---

### Events

Stores the event-level information created by an event owner.

| Column | Type Suggestion | Notes |
|---|---|---|
| Id | uuid / bigint | Primary key |
| OwnerId | FK | References `Users.Id` |
| Name | varchar | Required |
| Description | text | Optional |
| Status | enum/string | Draft, Published, Cancelled, Archived |
| BookingMode | enum/string | Seats, Tickets |
| CreatedAt | timestamp | Required |
| UpdatedAt | timestamp | Required |

Relationship:

```text
Users 1 ---- * Events
```

---

### EventSessions

Represents the actual bookable occurrence of an event.

| Column | Type Suggestion | Notes |
|---|---|---|
| Id | uuid / bigint | Primary key |
| EventId | FK | References `Events.Id` |
| StartDateTime | timestamp | Required |
| EndDateTime | timestamp | Required |
| Location | varchar/text | Required if the session is physical |
| Status | enum/string | Active, Cancelled, SoldOut, Completed |

Relationship:

```text
Events 1 ---- * EventSessions
```

---

### Seats

Used only when `Events.BookingMode = Seats`.

| Column | Type Suggestion | Notes |
|---|---|---|
| Id | uuid / bigint | Primary key |
| EventSessionId | FK | References `EventSessions.Id` |
| SeatNumber | varchar | Example: A1, A2, VIP-10 |
| SeatType | varchar | VIP, Regular, Premium |
| Price | decimal | Current price |
| Status | enum/string | Available, Reserved, Booked, Disabled |
| RowVersion | bytea / xmin / timestamp | Used for optimistic concurrency |

Important constraints:

```text
Unique(EventSessionId, SeatNumber)
Price >= 0
```

Relationship:

```text
EventSessions 1 ---- * Seats
```

---

### TicketTypes

Used only when `Events.BookingMode = Tickets`.

| Column | Type Suggestion | Notes |
|---|---|---|
| Id | uuid / bigint | Primary key |
| EventSessionId | FK | References `EventSessions.Id` |
| Name | varchar | VIP, Regular, Early Bird |
| Price | decimal | Current unit price |
| TotalQuantity | integer | Total available quantity |
| AvailableQuantity | integer | Remaining quantity |
| Status | enum/string | Active, Disabled, SoldOut |

Important constraints:

```text
Unique(EventSessionId, Name)
Price >= 0
TotalQuantity >= 0
AvailableQuantity >= 0
AvailableQuantity <= TotalQuantity
```

Relationship:

```text
EventSessions 1 ---- * TicketTypes
```

---

### Bookings

The main booking entity. It is similar to an order in an e-commerce system.

| Column | Type Suggestion | Notes |
|---|---|---|
| Id | uuid / bigint | Primary key |
| UserId | FK | References `Users.Id` |
| EventSessionId | FK | References `EventSessions.Id` |
| BookingNumber | varchar | Unique readable booking number |
| Status | enum/string | Pending, Confirmed, Cancelled, Expired, Failed |
| TotalAmount | decimal | Final calculated total |
| ExpiresAt | timestamp | Used for temporary reservations |
| CreatedAt | timestamp | Required |
| UpdatedAt | timestamp | Required |

Important constraints:

```text
Unique(BookingNumber)
TotalAmount >= 0
```

Relationships:

```text
Users 1 ---- * Bookings
EventSessions 1 ---- * Bookings
```

---

### BookingSeats

Booking details for seat-based events.

| Column | Type Suggestion | Notes |
|---|---|---|
| Id | uuid / bigint | Primary key |
| BookingId | FK | References `Bookings.Id` |
| SeatId | FK | References `Seats.Id` |
| Price | decimal | Seat price at booking time |

Important rules:

```text
Seat.EventSessionId must match Booking.EventSessionId
Seat cannot be linked to more than one active booking
Price >= 0
```

Relationship:

```text
Bookings 1 ---- * BookingSeats
Seats 1 ---- 0..1 active BookingSeats
```

---

### BookingTicketTypes

Booking details for ticket-based events.

| Column | Type Suggestion | Notes |
|---|---|---|
| Id | uuid / bigint | Primary key |
| BookingId | FK | References `Bookings.Id` |
| TicketTypeId | FK | References `TicketTypes.Id` |
| Quantity | integer | Required, greater than zero |
| UnitPrice | decimal | Ticket price at booking time |

Important rules:

```text
TicketType.EventSessionId must match Booking.EventSessionId
Quantity > 0
UnitPrice >= 0
TicketTypes.AvailableQuantity >= Quantity
```

Relationships:

```text
Bookings 1 ---- * BookingTicketTypes
TicketTypes 1 ---- * BookingTicketTypes
```

---

### Payments

Stores payment attempts for a booking. One booking can have multiple payment attempts.

| Column | Type Suggestion | Notes |
|---|---|---|
| Id | uuid / bigint | Primary key |
| BookingId | FK | References `Bookings.Id` |
| Amount | decimal | Payment amount |
| Status | enum/string | Pending, Paid, Failed, Refunded |
| PaymentMethod | varchar | Card, Wallet, Cash, Gateway |
| TransactionReference | varchar | Gateway reference, nullable |
| CreatedAt | timestamp | Required |

Important constraints:

```text
Amount >= 0
```

Relationship:

```text
Bookings 1 ---- * Payments
```

---

### Notifications

Stores notifications sent to users.

| Column | Type Suggestion | Notes |
|---|---|---|
| Id | uuid / bigint | Primary key |
| UserId | FK | References `Users.Id` |
| Title | varchar | Required |
| Message | text | Required |
| Type | varchar | Booking, Payment, Event, System |
| IsSeen | boolean | Default false |
| CreatedAt | timestamp | Required |
| SeenAt | timestamp | Nullable |

Relationship:

```text
Users 1 ---- * Notifications
```

---

## 4. Recommended Indexes

```text
Users(Email) UNIQUE
Events(OwnerId)
Events(Status)
Events(BookingMode)
EventSessions(EventId)
EventSessions(StartDateTime)
Seats(EventSessionId)
Seats(EventSessionId, SeatNumber) UNIQUE
Seats(Status)
TicketTypes(EventSessionId)
TicketTypes(EventSessionId, Name) UNIQUE
Bookings(UserId)
Bookings(EventSessionId)
Bookings(Status)
Bookings(BookingNumber) UNIQUE
BookingSeats(BookingId)
BookingSeats(SeatId)
BookingTicketTypes(BookingId)
BookingTicketTypes(TicketTypeId)
Payments(BookingId)
Payments(Status)
Notifications(UserId)
Notifications(IsSeen)
```

---

## 5. Notes for Implementation

### ASP.NET Core Identity

If ASP.NET Core Identity is used, the actual user and role tables may be generated by Identity, such as:

- `AspNetUsers`
- `AspNetRoles`
- `AspNetUserRoles`

In that case, the custom `Users` table in this design represents the application user concept, but the implementation can map it to Identity's `ApplicationUser`.

### PostgreSQL Concurrency

For seat booking concurrency, use a database transaction and either:

- optimistic concurrency with a `RowVersion`/concurrency token, or
- row-level locks during reservation, or
- a partial unique strategy for active booking states.

### Optional caching later

A distributed cache may be added later for faster temporary seat reservation checks and availability reads, but PostgreSQL should remain the source of truth. **No external cache is required or used in the current codebase.**

