# Ticketing Platform - API Design

## 1. Overview

This document defines the initial API design for the Ticketing Platform MVP.

The system supports three main user roles:

- **User**: Browses events, views sessions, books seats or tickets, pays, and receives notifications.
- **Event Owner**: Creates and manages events, sessions, seats, ticket types, and tracks bookings.
- **Admin**: Monitors users, events, bookings, and can moderate or suspend entities.

The API is designed around the following core modules:

- Authentication
- Public Events
- Event Owner Management
- Event Sessions
- Seats
- Ticket Types
- Bookings
- Payments
- Notifications
- Admin Monitoring

---

## 2. Base URL

```http
/api
```

Example:

```http
GET /api/events
```

---

## 3. Authentication

The API uses JWT Bearer Authentication.

Authenticated requests should include:

```http
Authorization: Bearer <access_token>
```

---

## 4. Roles and Authorization Rules

| Role | Main Permissions |
|---|---|
| User | Browse events, create bookings, pay, view own bookings, view notifications |
| EventOwner | Create and manage own events, sessions, seats, ticket types, and view bookings for own events |
| Admin | Monitor users, event owners, events, bookings, and suspend or approve resources |

---

## 5. Common Status Codes

| Status Code | Meaning |
|---|---|
| 200 OK | Request completed successfully |
| 201 Created | Resource created successfully |
| 400 Bad Request | Invalid request body or validation error |
| 401 Unauthorized | User is not authenticated |
| 403 Forbidden | User does not have permission |
| 404 Not Found | Resource was not found |
| 409 Conflict | Business conflict such as seat already booked or insufficient quantity |
| 422 Unprocessable Entity | Valid request format but invalid business rule |
| 500 Internal Server Error | Unexpected server error |

---

## 6. Common Error Response Format

### General Error

```json
{
  "message": "Seat is already booked"
}
```

### Validation Error

```json
{
  "message": "Validation failed",
  "errors": [
    {
      "field": "email",
      "message": "Email is required"
    }
  ]
}
```

---

# 7. Auth Endpoints

## 7.1 Register User

```http
POST /api/auth/register
```

### Description

Creates a new user account.

### Authorization

Public

### Request Body

```json
{
  "firstName": "Ahmed",
  "lastName": "Ali",
  "email": "ahmed@example.com",
  "password": "Password123!",
  "phoneNumber": "+201000000000",
  "role": "User"
}
```

### Notes

Allowed roles during registration may be limited depending on the business rules.

For example:

- Normal users can register publicly.
- Event owners may require admin approval.

### Response 201

```json
{
  "id": "user-id",
  "firstName": "Ahmed",
  "lastName": "Ali",
  "email": "ahmed@example.com",
  "roles": ["User"]
}
```

### Status Codes

- 201 Created
- 400 Bad Request
- 409 Conflict

---

## 7.2 Login

```http
POST /api/auth/login
```

### Description

Authenticates the user and returns an access token.

### Authorization

Public

### Request Body

```json
{
  "email": "ahmed@example.com",
  "password": "Password123!"
}
```

### Response 200

```json
{
  "accessToken": "jwt-token",
  "expiresIn": 3600,
  "user": {
    "id": "user-id",
    "fullName": "Ahmed Ali",
    "email": "ahmed@example.com",
    "roles": ["User"]
  }
}
```

### Status Codes

- 200 OK
- 400 Bad Request
- 401 Unauthorized

---

## 7.3 Get Current User

```http
GET /api/auth/me
```

### Description

Returns the currently authenticated user profile.

### Authorization

Authenticated User

### Response 200

```json
{
  "id": "user-id",
  "firstName": "Ahmed",
  "lastName": "Ali",
  "email": "ahmed@example.com",
  "phoneNumber": "+201000000000",
  "roles": ["User"]
}
```

### Status Codes

- 200 OK
- 401 Unauthorized

---

# 8. Public Event Endpoints

## 8.1 Get Events

```http
GET /api/events
```

### Description

Returns a paginated list of published events.

### Authorization

Public or Authenticated User

### Query Parameters

| Name | Type | Required | Description |
|---|---|---|---|
| page | int | No | Page number |
| pageSize | int | No | Number of records per page |
| search | string | No | Search by event name |
| status | string | No | Event status |
| bookingMode | string | No | Seats or Tickets |

### Response 200

```json
{
  "page": 1,
  "pageSize": 10,
  "totalCount": 25,
  "items": [
    {
      "id": "event-id",
      "name": "Tech Conference",
      "description": "Annual technology conference",
      "status": "Published",
      "bookingMode": "Seats",
      "createdAt": "2026-04-27T10:00:00Z"
    }
  ]
}
```

### Status Codes

- 200 OK

---

## 8.2 Get Event Details

```http
GET /api/events/{eventId}
```

### Description

Returns the details of a specific event.

### Authorization

Public or Authenticated User

### Response 200

```json
{
  "id": "event-id",
  "name": "Tech Conference",
  "description": "Annual technology conference",
  "status": "Published",
  "bookingMode": "Seats",
  "owner": {
    "id": "owner-id",
    "name": "Event Owner Name"
  },
  "createdAt": "2026-04-27T10:00:00Z",
  "updatedAt": "2026-04-27T10:00:00Z"
}
```

### Status Codes

- 200 OK
- 404 Not Found

---

## 8.3 Get Event Sessions

```http
GET /api/events/{eventId}/sessions
```

### Description

Returns available sessions for a specific event.

### Authorization

Public or Authenticated User

### Response 200

```json
[
  {
    "id": "session-id",
    "eventId": "event-id",
    "startDateTime": "2026-05-01T18:00:00Z",
    "endDateTime": "2026-05-01T22:00:00Z",
    "location": "Cairo",
    "status": "Active"
  }
]
```

### Status Codes

- 200 OK
- 404 Not Found

---

# 9. Event Owner Endpoints

## 9.1 Create Event

```http
POST /api/owner/events
```

### Description

Creates a new event owned by the authenticated event owner.

### Authorization

EventOwner

### Request Body

```json
{
  "name": "Tech Conference",
  "description": "Annual technology conference",
  "bookingMode": "Seats"
}
```

### Response 201

```json
{
  "id": "event-id",
  "ownerId": "owner-id",
  "name": "Tech Conference",
  "description": "Annual technology conference",
  "status": "Draft",
  "bookingMode": "Seats",
  "createdAt": "2026-04-27T10:00:00Z"
}
```

### Status Codes

- 201 Created
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden

---

## 9.2 Get My Events

```http
GET /api/owner/events
```

### Description

Returns events created by the authenticated event owner.

### Authorization

EventOwner

### Response 200

```json
[
  {
    "id": "event-id",
    "name": "Tech Conference",
    "status": "Draft",
    "bookingMode": "Seats",
    "createdAt": "2026-04-27T10:00:00Z"
  }
]
```

### Status Codes

- 200 OK
- 401 Unauthorized
- 403 Forbidden

---

## 9.3 Update Event

```http
PUT /api/owner/events/{eventId}
```

### Description

Updates an event owned by the authenticated event owner.

### Authorization

EventOwner

### Request Body

```json
{
  "name": "Updated Tech Conference",
  "description": "Updated description",
  "status": "Published"
}
```

### Response 200

```json
{
  "id": "event-id",
  "name": "Updated Tech Conference",
  "description": "Updated description",
  "status": "Published",
  "bookingMode": "Seats",
  "updatedAt": "2026-04-27T12:00:00Z"
}
```

### Status Codes

- 200 OK
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found

---

## 9.4 Delete Event

```http
DELETE /api/owner/events/{eventId}
```

### Description

Deletes or archives an event owned by the authenticated event owner.

### Authorization

EventOwner

### Response 200

```json
{
  "message": "Event deleted successfully"
}
```

### Status Codes

- 200 OK
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found

---

# 10. Event Session Endpoints

## 10.1 Create Event Session

```http
POST /api/owner/events/{eventId}/sessions
```

### Description

Creates a new session for an event.

### Authorization

EventOwner

### Request Body

```json
{
  "startDateTime": "2026-05-01T18:00:00Z",
  "endDateTime": "2026-05-01T22:00:00Z",
  "location": "Cairo"
}
```

### Response 201

```json
{
  "id": "session-id",
  "eventId": "event-id",
  "startDateTime": "2026-05-01T18:00:00Z",
  "endDateTime": "2026-05-01T22:00:00Z",
  "location": "Cairo",
  "status": "Active"
}
```

### Status Codes

- 201 Created
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found

---

## 10.2 Update Event Session

```http
PUT /api/owner/sessions/{sessionId}
```

### Description

Updates a session owned by the authenticated event owner.

### Authorization

EventOwner

### Request Body

```json
{
  "startDateTime": "2026-05-01T19:00:00Z",
  "endDateTime": "2026-05-01T23:00:00Z",
  "location": "Cairo International Center",
  "status": "Active"
}
```

### Response 200

```json
{
  "id": "session-id",
  "startDateTime": "2026-05-01T19:00:00Z",
  "endDateTime": "2026-05-01T23:00:00Z",
  "location": "Cairo International Center",
  "status": "Active"
}
```

### Status Codes

- 200 OK
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found

---

# 11. Seat Endpoints

## 11.1 Create Seats for Session

```http
POST /api/owner/sessions/{sessionId}/seats
```

### Description

Creates seats for a seat-based event session.

### Authorization

EventOwner

### Request Body

```json
{
  "seats": [
    {
      "seatNumber": "A1",
      "seatType": "VIP",
      "price": 500
    },
    {
      "seatNumber": "A2",
      "seatType": "VIP",
      "price": 500
    }
  ]
}
```

### Response 201

```json
{
  "message": "Seats created successfully",
  "createdCount": 2
}
```

### Status Codes

- 201 Created
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found
- 409 Conflict

---

## 11.2 Get Session Seats

```http
GET /api/sessions/{sessionId}/seats
```

### Description

Returns seats for a session.

### Authorization

Public or Authenticated User

### Response 200

```json
[
  {
    "id": "seat-id-1",
    "seatNumber": "A1",
    "seatType": "VIP",
    "price": 500,
    "status": "Available"
  },
  {
    "id": "seat-id-2",
    "seatNumber": "A2",
    "seatType": "VIP",
    "price": 500,
    "status": "Reserved"
  }
]
```

### Status Codes

- 200 OK
- 404 Not Found

---

# 12. Ticket Type Endpoints

## 12.1 Create Ticket Type

```http
POST /api/owner/sessions/{sessionId}/ticket-types
```

### Description

Creates a ticket type for a ticket-based event session.

### Authorization

EventOwner

### Request Body

```json
{
  "name": "VIP",
  "price": 500,
  "totalQuantity": 100
}
```

### Response 201

```json
{
  "id": "ticket-type-id",
  "eventSessionId": "session-id",
  "name": "VIP",
  "price": 500,
  "totalQuantity": 100,
  "availableQuantity": 100,
  "status": "Active"
}
```

### Status Codes

- 201 Created
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found
- 409 Conflict

---

## 12.2 Get Session Ticket Types

```http
GET /api/sessions/{sessionId}/ticket-types
```

### Description

Returns ticket types for a session.

### Authorization

Public or Authenticated User

### Response 200

```json
[
  {
    "id": "ticket-type-id",
    "name": "VIP",
    "price": 500,
    "totalQuantity": 100,
    "availableQuantity": 80,
    "status": "Active"
  }
]
```

### Status Codes

- 200 OK
- 404 Not Found

---

# 13. Booking Endpoints

## 13.1 Create Seat-Based Booking

```http
POST /api/bookings/seats
```

### Description

Creates a pending booking for selected seats.

### Authorization

User

### Request Body

```json
{
  "eventSessionId": "session-id",
  "seatIds": [
    "seat-id-1",
    "seat-id-2"
  ]
}
```

### Response 201

```json
{
  "bookingId": "booking-id",
  "bookingNumber": "BK-2026-000001",
  "status": "Pending",
  "totalAmount": 1000,
  "expiresAt": "2026-04-27T15:30:00Z"
}
```

### Business Rules

- Event booking mode must be `Seats`.
- All selected seats must belong to the same event session.
- All selected seats must be available.
- Seats should be locked or reserved during the pending booking period.
- If payment is not completed before `expiresAt`, the booking should expire.

### Status Codes

- 201 Created
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found
- 409 Conflict

---

## 13.2 Create Ticket-Based Booking

```http
POST /api/bookings/tickets
```

### Description

Creates a pending booking for selected ticket types and quantities.

### Authorization

User

### Request Body

```json
{
  "eventSessionId": "session-id",
  "items": [
    {
      "ticketTypeId": "vip-ticket-type-id",
      "quantity": 2
    },
    {
      "ticketTypeId": "regular-ticket-type-id",
      "quantity": 1
    }
  ]
}
```

### Response 201

```json
{
  "bookingId": "booking-id",
  "bookingNumber": "BK-2026-000002",
  "status": "Pending",
  "totalAmount": 1200,
  "expiresAt": "2026-04-27T15:30:00Z"
}
```

### Business Rules

- Event booking mode must be `Tickets`.
- Ticket types must belong to the same event session.
- Quantity must be greater than zero.
- Available quantity must be enough.
- Reserved quantities should be released if payment expires.

### Status Codes

- 201 Created
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found
- 409 Conflict

---

## 13.3 Get My Bookings

```http
GET /api/bookings/my
```

### Description

Returns bookings created by the authenticated user.

### Authorization

User

### Response 200

```json
[
  {
    "id": "booking-id",
    "bookingNumber": "BK-2026-000001",
    "eventName": "Tech Conference",
    "sessionStartDateTime": "2026-05-01T18:00:00Z",
    "status": "Confirmed",
    "totalAmount": 1000,
    "createdAt": "2026-04-27T10:00:00Z"
  }
]
```

### Status Codes

- 200 OK
- 401 Unauthorized

---

## 13.4 Get Booking Details

```http
GET /api/bookings/{bookingId}
```

### Description

Returns booking details.

### Authorization

Booking owner, EventOwner of the event, or Admin

### Response 200

```json
{
  "id": "booking-id",
  "bookingNumber": "BK-2026-000001",
  "status": "Confirmed",
  "totalAmount": 1000,
  "expiresAt": "2026-04-27T15:30:00Z",
  "event": {
    "id": "event-id",
    "name": "Tech Conference"
  },
  "session": {
    "id": "session-id",
    "startDateTime": "2026-05-01T18:00:00Z",
    "location": "Cairo"
  },
  "seats": [
    {
      "seatId": "seat-id-1",
      "seatNumber": "A1",
      "seatType": "VIP",
      "price": 500
    }
  ],
  "ticketTypes": []
}
```

### Status Codes

- 200 OK
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found

---

## 13.5 Cancel Booking

```http
POST /api/bookings/{bookingId}/cancel
```

### Description

Cancels a pending or confirmed booking depending on business rules.

### Authorization

Booking owner

### Response 200

```json
{
  "message": "Booking cancelled successfully"
}
```

### Status Codes

- 200 OK
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found
- 409 Conflict

---

## 13.6 Get Bookings for Owner Event

```http
GET /api/owner/events/{eventId}/bookings
```

### Description

Returns bookings for an event owned by the authenticated event owner.

### Authorization

EventOwner

### Response 200

```json
[
  {
    "id": "booking-id",
    "bookingNumber": "BK-2026-000001",
    "userName": "Ahmed Ali",
    "status": "Confirmed",
    "totalAmount": 1000,
    "createdAt": "2026-04-27T10:00:00Z"
  }
]
```

### Status Codes

- 200 OK
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found

---

# 14. Payment Endpoints

## 14.1 Simulate Payment

```http
POST /api/payments/simulate
```

### Description

Simulates payment for a pending booking.

### Authorization

Booking owner

### Request Body

```json
{
  "bookingId": "booking-id",
  "paymentMethod": "Card"
}
```

### Response 200

```json
{
  "paymentId": "payment-id",
  "bookingId": "booking-id",
  "status": "Paid",
  "amount": 1000,
  "transactionReference": "SIM-123456",
  "createdAt": "2026-04-27T15:20:00Z"
}
```

### Business Rules

- Booking must exist.
- Booking must belong to the authenticated user.
- Booking must be pending.
- Booking must not be expired.
- If payment succeeds, booking status becomes `Confirmed`.
- If payment fails, payment status becomes `Failed`.

### Status Codes

- 200 OK
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found
- 409 Conflict

---

# 15. Notification Endpoints

## 15.1 Get Notifications

```http
GET /api/notifications
```

### Description

Returns notifications for the authenticated user.

### Authorization

Authenticated User

### Response 200

```json
[
  {
    "id": "notification-id",
    "title": "Booking Confirmed",
    "message": "Your booking BK-2026-000001 has been confirmed.",
    "type": "BookingConfirmed",
    "isSeen": false,
    "createdAt": "2026-04-27T15:20:00Z"
  }
]
```

### Status Codes

- 200 OK
- 401 Unauthorized

---

## 15.2 Mark Notification as Seen

```http
POST /api/notifications/{notificationId}/mark-as-seen
```

### Description

Marks a notification as seen.

### Authorization

Notification owner

### Response 200

```json
{
  "message": "Notification marked as seen"
}
```

### Status Codes

- 200 OK
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found

---

## 15.3 Mark All Notifications as Seen

```http
POST /api/notifications/mark-all-as-seen
```

### Description

Marks all notifications for the authenticated user as seen.

### Authorization

Authenticated User

### Response 200

```json
{
  "message": "All notifications marked as seen"
}
```

### Status Codes

- 200 OK
- 401 Unauthorized

---

# 16. Admin Endpoints

## 16.1 Get Users

```http
GET /api/admin/users
```

### Description

Returns users in the system.

### Authorization

Admin

### Response 200

```json
[
  {
    "id": "user-id",
    "fullName": "Ahmed Ali",
    "email": "ahmed@example.com",
    "roles": ["User"],
    "isActive": true
  }
]
```

### Status Codes

- 200 OK
- 401 Unauthorized
- 403 Forbidden

---

## 16.2 Get Events

```http
GET /api/admin/events
```

### Description

Returns all events for monitoring.

### Authorization

Admin

### Response 200

```json
[
  {
    "id": "event-id",
    "name": "Tech Conference",
    "ownerName": "Event Owner Name",
    "status": "Published",
    "bookingMode": "Seats"
  }
]
```

### Status Codes

- 200 OK
- 401 Unauthorized
- 403 Forbidden

---

## 16.3 Suspend Event

```http
POST /api/admin/events/{eventId}/suspend
```

### Description

Suspends an event.

### Authorization

Admin

### Request Body

```json
{
  "reason": "Policy violation"
}
```

### Response 200

```json
{
  "message": "Event suspended successfully"
}
```

### Status Codes

- 200 OK
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found

---

## 16.4 Get Bookings

```http
GET /api/admin/bookings
```

### Description

Returns bookings for admin monitoring.

### Authorization

Admin

### Response 200

```json
[
  {
    "id": "booking-id",
    "bookingNumber": "BK-2026-000001",
    "userName": "Ahmed Ali",
    "eventName": "Tech Conference",
    "status": "Confirmed",
    "totalAmount": 1000,
    "createdAt": "2026-04-27T10:00:00Z"
  }
]
```

### Status Codes

- 200 OK
- 401 Unauthorized
- 403 Forbidden

---

# 17. Important Business Rules

## Booking Mode Rules

If `Event.BookingMode = Seats`:

- Booking must use `/api/bookings/seats`.
- Booking details must be stored in `BookingSeats`.
- `BookingTicketTypes` must not be used.

If `Event.BookingMode = Tickets`:

- Booking must use `/api/bookings/tickets`.
- Booking details must be stored in `BookingTicketTypes`.
- `BookingSeats` must not be used.

---

## Seat Booking Rules

- Seat must belong to the selected event session.
- Seat must be available before reservation.
- A seat cannot belong to more than one active booking.
- Seat status should be changed to `Reserved` during pending booking.
- Seat status should be changed to `Booked` after successful payment.
- Seat status should return to `Available` if booking expires or is cancelled.

---

## Ticket Quantity Rules

- Ticket type must belong to the selected event session.
- Quantity must be greater than zero.
- Available quantity must be enough.
- Available quantity should decrease during reservation.
- Available quantity should return if booking expires or is cancelled.

---

## Payment Rules

- Payment can only be created for pending bookings.
- Expired bookings cannot be paid.
- Successful payment confirms the booking.
- Failed payment does not confirm the booking.
- Multiple payment attempts may exist for the same booking.

---

# 18. MVP Endpoint Summary

## Auth

```http
POST /api/auth/register
POST /api/auth/login
GET  /api/auth/me
```

## Public Events

```http
GET /api/events
GET /api/events/{eventId}
GET /api/events/{eventId}/sessions
GET /api/sessions/{sessionId}/seats
GET /api/sessions/{sessionId}/ticket-types
```

## Event Owner

```http
POST   /api/owner/events
GET    /api/owner/events
PUT    /api/owner/events/{eventId}
DELETE /api/owner/events/{eventId}

POST /api/owner/events/{eventId}/sessions
PUT  /api/owner/sessions/{sessionId}

POST /api/owner/sessions/{sessionId}/seats
POST /api/owner/sessions/{sessionId}/ticket-types

GET /api/owner/events/{eventId}/bookings
```

## Bookings

```http
POST /api/bookings/seats
POST /api/bookings/tickets
GET  /api/bookings/my
GET  /api/bookings/{bookingId}
POST /api/bookings/{bookingId}/cancel
```

## Payments

```http
POST /api/payments/simulate
```

## Notifications

```http
GET  /api/notifications
POST /api/notifications/{notificationId}/mark-as-seen
POST /api/notifications/mark-all-as-seen
```

## Admin

```http
GET  /api/admin/users
GET  /api/admin/events
POST /api/admin/events/{eventId}/suspend
GET  /api/admin/bookings
```
