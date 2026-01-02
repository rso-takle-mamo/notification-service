# Notification Service

## Overview

The Notification Service handles sending notifications to users for the appointments system. It consumes booking events from Kafka and sends email notifications to customers. Currently, it uses a mock email implementation that logs notification details, which can be replaced with a real email service integration in production.

## Database

### Tables and Schema

#### Tenants Table
**NOTE:** This table is replicated from the Users service via Kafka events.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | UUID | Primary Key | Tenant identifier |
| `BusinessName` | VARCHAR(255) | Required | Business name |
| `Email` | VARCHAR(255) | Nullable | Business email |
| `Phone` | VARCHAR(50) | Nullable | Business phone |
| `Address` | VARCHAR(500) | Nullable | Business address |
| `TimeZone` | VARCHAR(50) | Nullable | Time zone for the tenant |
| `BufferBeforeMinutes` | INTEGER | Required | Buffer time before appointments (default: 0) |
| `BufferAfterMinutes` | INTEGER | Required | Buffer time after appointments (default: 0) |
| `CreatedAt` | TIMESTAMPTZ | Required | Creation timestamp |
| `UpdatedAt` | TIMESTAMPTZ | Required | Last update timestamp |

#### Users Table
**NOTE:** This table is replicated from the Users service via Kafka events.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | UUID | Primary Key | User identifier |
| `TenantId` | UUID | Foreign Key | Reference to tenant |
| `FirstName` | VARCHAR(100) | Required | User's first name |
| `LastName` | VARCHAR(100) | Required | User's last name |
| `Email` | VARCHAR(255) | Required, Unique | User's email address |
| `PhoneNumber` | VARCHAR(20) | Nullable | User's phone number |
| `Role` | INTEGER | Required | User role (0=Provider, 1=Customer) |
| `IsActive` | BOOLEAN | Required | Whether the user is active |
| `CreatedAt` | TIMESTAMPTZ | Required | Creation timestamp |
| `UpdatedAt` | TIMESTAMPTZ | Required | Last update timestamp |

#### Bookings Table
**NOTE:** This table is replicated from the Booking service via Kafka events.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | UUID | Primary Key | Booking identifier |
| `TenantId` | UUID | Required, Foreign Key | Reference to tenant |
| `StartDateTime` | TIMESTAMPTZ | Required | Start time of booking |
| `EndDateTime` | TIMESTAMPTZ | Required | End time of booking |
| `CreatedAt` | TIMESTAMPTZ | Required | Creation timestamp |
| `UpdatedAt` | TIMESTAMPTZ | Required | Last update timestamp |

**Note:** The Booking table in the notification service only stores the minimal information needed for tracking bookings. Additional booking details (OwnerId, ServiceId, Status) are received via Kafka events but are not persisted.

### Database Relationships

1. **Users → Tenants:** Many-to-one via `TenantId` (users belong to one tenant)
2. **Bookings → Tenants:** Many-to-one via `TenantId` (bookings belong to one tenant)

### Foreign Key Constraints

- `FK_Users_Tenants_TenantId` → `Tenants(Id)` (ON DELETE CASCADE)
- `FK_Bookings_Tenants_TenantId` → `Tenants(Id)` (ON DELETE CASCADE)

## Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `DATABASE_CONNECTION_STRING` | Yes | PostgreSQL connection string |
| `JWT_SECRET_KEY` | Yes | JWT signing key (minimum 128 bits) |
| `ASPNETCORE_ENVIRONMENT` | No | Environment (Development/Production) |
| `KAFKA__BOOTSTRAPSERVERS` | Yes | Kafka bootstrap servers |
| `KAFKA__USEREVENTSTOPIC` | Yes | User events topic (consumer) |
| `KAFKA__PROVIDEREVENTSTOPIC` | Yes | Provider events topic (consumer) |
| `KAFKA__TENANTEVENTSTOPIC` | Yes | Tenant events topic (consumer) |
| `KAFKA__BOOKINGEVENTSTOPIC` | Yes | Booking events topic (consumer) |
| `KAFKA__CONSUMERGROUPID` | Yes | Kafka consumer group ID |
| `KAFKA__ENABLEAUTOCOMMIT` | Yes | Kafka auto-commit setting |
| `KAFKA__AUTOOFFSETRESET` | Yes | Kafka auto offset reset |

## Health Checks

- `GET /health` - Complete health check including database
- `GET /health/live` - Basic service liveness check
- `GET /health/ready` - Readiness check for dependencies

## Kafka Events

The Notification Service acts as a **Kafka Consumer** only (for user, tenant, provider, and booking events).

### Consumed Events

| Event Type | Topic | Handler | Description |
|------------|-------|---------|-------------|
| `UserCreatedEvent` | `user-events` | `UserEventService` | Creates user in local database |
| `UserUpdatedEvent` | `user-events` | `UserEventService` | Updates user in local database |
| `TenantCreatedEvent` | `tenant-events` | `TenantEventService` | Creates tenant in local database |
| `TenantUpdatedEvent` | `tenant-events` | `TenantEventService` | Updates tenant in local database |
| `ProviderCreatedEvent` | `provider-events` | `UserEventService` | Creates provider (user) in local database |
| `BookingCreatedEvent` | `booking-events` | `BookingEventService` | Creates booking record and sends booking confirmation email |
| `BookingCancelledEvent` | `booking-events` | `BookingEventService` | Deletes booking record and sends cancellation email |

### Consumer Configuration

- **Auto Commit:** `false` (manual offset management)
- **Offset Reset:** `Earliest` (read from beginning)
- **Consumer Groups:**
  - `notification-service-user-events` (for user events)
  - `notification-service-provider-events` (for provider events)
  - `notification-service-tenant-events` (for tenant events)
  - `notification-service-booking-events` (for booking events)

## Email Notifications

### Mock Email Implementation

The service currently uses a mock email implementation that logs email details to the service logs. In production, this should be replaced with a real email service integration.

### Booking Created Email

Sent to customers when a booking is successfully created:

```
To: {FirstName} <{UserId}>
Subject: Your booking has been confirmed!

Dear {FirstName},

Your booking has been successfully created!
Booking ID: {BookingId}
Service ID: {ServiceId}
Start Time: {StartDateTime}
End Time: {EndDateTime}
Notes: {Notes}

Thank you for your booking!
```

### Booking Cancelled Email

Sent to customers when a booking is cancelled:

```
To: {FirstName} <{UserId}>
Subject: Your booking has been cancelled

Dear {FirstName},

Your booking has been cancelled as requested.
Booking ID: {BookingId}
Service ID: {ServiceId}

We hope to serve you again soon!
```