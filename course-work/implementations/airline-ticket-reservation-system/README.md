# Airline Ticket Reservation System

## Student

- Name: Mihail Zhekov
- Faculty number: 2401321043
- Project: Airline Ticket Reservation System

## Description

Airline Ticket Reservation System is a distributed coursework project with two connected applications:

- ASP.NET Core Web API backend
- ASP.NET Core MVC frontend client

The system manages users, airports, aircraft, flights, reservations, tickets, and payments. A reservation can contain multiple passenger tickets. Flight available seats are calculated from aircraft seat capacity and active tickets, not stored as a database column.

## Technology Stack

- .NET 10
- ASP.NET Core Web API
- ASP.NET Core MVC
- Entity Framework Core 10
- SQL Server
- JWT Bearer authentication
- Swagger/OpenAPI
- Serilog
- Repository and Unit of Work pattern

## Project Structure

```text
AirlineReservation.Api                  HTTP API, controllers, JWT, Swagger, middleware
AirlineReservation.Web                  MVC frontend and Admin area
AirlineReservation.Contracts            Request/response DTOs and paging models
AirlineReservation.Data                 EF Core entities, enums, DbContext
AirlineReservation.Repositories         Generic repository and UnitOfWork
AirlineReservation.ApplicationServices  Business logic, validation, filtering, mapping
```

Main request flow:

```text
MVC/Swagger client -> API controller -> Application service -> UnitOfWork/Repository -> EF Core DbContext -> SQL Server
```

## Database

Default connection string:

```text
Server=.;Database=AirlineReservationDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True
```

If SQL Server uses SQL authentication, update:

```text
AirlineReservation.Api/appsettings.json
```

Apply migrations:

```text
dotnet ef database update --project AirlineReservation.Api --startup-project AirlineReservation.Api
```

The API seeds demo data on startup when the database is empty.

Default admin account:

```text
Email: admin@airline.local
Password: Admin123!
```

## Run

Restore and build:

```text
dotnet restore
dotnet build AirlineTicketReservation.sln
```

Run API:

```text
dotnet run --project AirlineReservation.Api --urls http://localhost:5235
```

Run MVC client:

```text
dotnet run --project AirlineReservation.Web --urls http://localhost:5000
```

Open:

```text
MVC:     http://localhost:5000
Swagger: http://localhost:5235/swagger
```

## Frontend Pages

Customer flow:

- Register/login
- Search flights with filters and sorting
- Create reservation with multiple passenger tickets
- View reservation details with flight route, tickets, and payments
- Record payment attempts

Admin area:

```text
http://localhost:5000/Admin
```

Admin features:

- CRUD for Users
- CRUD for Airports
- CRUD for Aircraft
- CRUD for Flights
- CRUD flow for Reservations through customer reservation creation plus admin listing, details, status editing, and deletion
- CRUD for Tickets
- CRUD for Payments

Presentation checklist:

```text
DEMO-CHECKLIST.md
```

## Coursework Criteria Coverage

- 3+ related database tables: users, airports, aircraft, flights, reservations, tickets, payments
- 6+ columns per main table: implemented in all domain entities
- CRUD operations: implemented in API and exposed through the MVC frontend/admin area
- Required fields and max length validation: DTO annotations and EF Core configuration
- Filtering by multiple criteria: implemented for list endpoints
- Sorting and pagination: implemented through `PagedRequest` and `PagedResult`
- Async operations: service/repository/controller methods are async
- Global exception handling: API middleware returns structured errors
- Security: JWT authentication for API, MVC session stores JWT for API calls
- Frontend validation: MVC forms use validation attributes and validation scripts
- Database validation: EF Core model configuration includes constraints and relationships
