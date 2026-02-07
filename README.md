# NightOut - Event Reservation Management System

[![Tests](https://img.shields.io/badge/tests-113%20passing-brightgreen)](https://github.com/Engjell02/NightOut-Event-Management)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

A full-stack event reservation management platform built with ASP.NET Core MVC featuring role-based access control, real-time revenue tracking, and a comprehensive 113-test QA suite.

**Live Application:** https://reservationmanagementappweb20260126154050-c5dec0ceayd5b3gg.francecentral-01.azurewebsites.net/

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Installation](#installation)
- [Testing Suite](#testing-suite)
- [Author](#author)
- [License](#license)

---

## Overview

NightOut is a production-ready event management platform that enables seamless reservation management for nightlife events. The application supports two user roles: customers who can browse and book events, and administrators who manage the entire platform including event creation, reservation approval, and revenue tracking.

### Key Capabilities
- Secure authentication with ASP.NET Core Identity
- Role-based authorization (Admin/User)
- Real-time revenue calculation from approved reservations
- External API integration for event imports
- Azure cloud deployment
- 113 automated tests with 100% pass rate

---

## Features

### User Features
- Browse upcoming nightlife events with filtering capabilities
- View detailed event information including date, location, performers, and pricing
- Make table reservations for 2-6 people per table
- View personal reservation history with status tracking (Pending/Approved/Rejected)
- Cancel reservations up to 24 hours before the event
- Real-time table availability checking

### Administrator Features
- Dashboard with real-time statistics:
  - Total events, locations, and performers
  - Pending and approved reservations count
  - Revenue calculation from approved bookings
- Reservation Management:
  - Approve or reject pending reservations
  - Filter reservations by event
  - View customer booking details
- Event Management (Full CRUD):
  - Create, edit, and delete events
  - Set pricing and capacity
  - Assign locations and performers
  - Import events from external API
- Location Management: Manage venues with capacity tracking
- Performer Management: Manage DJs, artists, and bands

### Security Features
- Password hashing with BCrypt
- Email confirmation workflow
- Role-based access control
- CSRF protection with anti-forgery tokens
- SQL injection prevention
- XSS protection
- Secure admin registration endpoint

---

## Technology Stack

### Backend
- **Framework:** ASP.NET Core 8.0 MVC
- **Language:** C# 12
- **ORM:** Entity Framework Core 8.0
- **Database:** SQL Server / Azure SQL
- **Authentication:** ASP.NET Core Identity with role management

### Frontend
- **UI Framework:** Bootstrap 5
- **Icons:** Bootstrap Icons
- **Styling:** Custom CSS with modern gradient themes

### Testing (113 Tests)
- **Framework:** xUnit 2.9.3
- **Mocking:** Moq 4.20.72
- **Assertions:** FluentAssertions 8.8.0
- **Integration Testing:** Microsoft.AspNetCore.Mvc.Testing 8.0
- **Test Data Generation:** Bogus 35.6.5
- **In-Memory Database:** EF Core InMemory 8.0
- **Browser Testing:** Selenium WebDriver 4.40.0

### Deployment
- **Cloud Platform:** Azure App Service
- **Database:** Azure SQL Database
- **Version Control:** Git & GitHub

---

## Architecture

### N-Tier Architecture
```
Presentation Layer (MVC)
    Controllers, Views, ViewModels, Areas
            ↓
Service Layer (Business Logic)
    ReservationService, EventService, LocationService, PerformerService
            ↓
Repository Layer (Data Access)
    Generic Repository Pattern + Entity Framework Core
            ↓
Domain Layer (Entities)
    Event, Reservation, Location, Performer, User
```

### Project Structure
```
NightOut/
├── Reservation_Management_App.Web/
│   ├── Controllers/         (MVC Controllers)
│   ├── Views/              (Razor Views)
│   ├── Models/             (ViewModels)
│   └── Areas/Identity/     (Authentication UI)
│
├── Reservation_Management_App.Domain/
│   ├── DomainModels/       (Entity Classes)
│   ├── Identity/           (User Entities)
│   └── Enums/              (ReservationStatus)
│
├── Reservation_Management_App.Repository/
│   ├── Interface/          (IRepository<T>)
│   └── ApplicationDbContext.cs
│
├── Reservation_Management_App.Service/
│   ├── Interface/          (Service Contracts)
│   └── Implementation/     (Business Logic)
│
└── Reservation_Management_App.Tests/  (113 TESTS)
    ├── UnitTests/          (43 tests)
    ├── IntegrationTests/   (20 tests)
    ├── ApiTests/           (15 tests)
    ├── SecurityTests/      (21 tests)
    ├── UITests/            (14 tests)
    └── TestUtilities/      (Test Helpers)
```

### Design Patterns
- **Repository Pattern** - Abstracts data access layer
- **Service Layer Pattern** - Encapsulates business logic
- **Dependency Injection** - Achieves loose coupling between components
- **MVC Pattern** - Separates presentation, business logic, and data

---

## Installation

### Prerequisites
- .NET 8.0 SDK
- SQL Server or SQL Server Express
- Visual Studio 2022 or VS Code
- Git

### Setup Instructions

1. Clone the repository
```bash
   git clone https://github.com/Engjell02/NightOut-Event-Management.git
   cd NightOut-Event-Management
```

2. Update the connection string in `appsettings.json`:
```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=NightOutDb;Trusted_Connection=True;"
     }
   }
```

3. Apply database migrations
```bash
   cd Reservation_Management_App.Web
   dotnet ef database update
```

4. Run the application
```bash
   dotnet run
```

5. Access the application
   - Navigate to `https://localhost:7XXX` in your browser
   - Register a regular user at `/Identity/Account/Register`
   - Create an admin account at `/Identity/Account/AdminRegister`

---

## Testing Suite

### Overview

The application includes a comprehensive QA testing suite with 113 automated tests covering all layers of the application.

| Test Category | Tests | Description |
|---------------|-------|-------------|
| Unit Tests | 43 | Service layer and business logic testing with mocking |
| Integration Tests | 20 | Database operations with EF Core InMemory |
| API Tests | 15 | Controller endpoints and HTTP responses |
| Security Tests | 21 | Authentication, authorization, and cryptography |
| UI Tests | 14 | Page loads, redirects, and HTTP validation |
| **TOTAL** | **113** | **100% Pass Rate** |

### Test Categories

#### 1. Unit Tests (43 tests)

Tests individual components in isolation using Moq for dependency injection.

**ReservationService Tests (14 tests):**
- CRUD operations
- Approve/Reject functionality
- Cancel reservation with 24-hour rule enforcement
- Validation (group size 2-6, available spots)

**EventService Tests (16 tests):**
- CRUD operations
- Get upcoming events
- Filter by location
- Order by date

**Revenue Calculation Tests (13 tests):**
- Revenue from approved reservations only
- Pending and rejected reservations excluded
- Multiple events aggregation
- Null handling

#### 2. Integration Tests (20 tests)

Tests repository operations with real Entity Framework Core InMemory database.

**ReservationRepository Tests (9 tests):**
- Insert, Update, Delete operations
- Get by ID
- Filter by user ID
- Database persistence

**EventRepository Tests (11 tests):**
- CRUD operations
- Filter by location, price, date
- Ordering
- Relationship preservation

#### 3. API/Controller Tests (15 tests)

Tests HTTP endpoints using mocked services.

**ReservationsController Tests:**
- User: My reservations, Reserve, Cancel
- Admin: Index with filtering, Approve/Reject
- Error handling and redirects

#### 4. Security Tests (21 tests)

Validates authentication, authorization, and cryptographic security.

**Authentication Tests (12 tests):**
- Password hashing with BCrypt
- Correct/incorrect password verification
- Salt uniqueness
- Weak password handling

**Authorization Tests (9 tests):**
- Controller-level authorization attributes
- Admin-only action verification
- User authentication requirements

#### 5. UI Tests (14 tests)

Tests HTTP responses using WebApplicationFactory.

- Homepage and main pages load correctly
- Login/Register pages
- Admin register page accessibility
- Dashboard redirects without authentication
- 404 error handling

### Running Tests

Run all tests:
```bash
cd Reservation_Management_App.Tests
dotnet test
```

Run specific category:
```bash
dotnet test --filter "FullyQualifiedName~UnitTests"
dotnet test --filter "FullyQualifiedName~IntegrationTests"
dotnet test --filter "FullyQualifiedName~SecurityTests"
```

Generate HTML report:
```bash
dotnet test --logger "html;logfilename=TestResults.html"
```

### Testing Technologies

| Technology | Purpose |
|------------|---------|
| xUnit 2.9.3 | Test framework |
| Moq 4.20.72 | Mocking dependencies |
| FluentAssertions 8.8.0 | Readable test assertions |
| Bogus 35.6.5 | Test data generation |
| EF Core InMemory 8.0.23 | Database testing |
| WebApplicationFactory 8.0.23 | Integration testing |

### Test Utilities

Custom helper classes for maintainable tests:
- **TestDataGenerator** - Generates realistic fake entities using Bogus
- **InMemoryDbContextFactory** - Creates isolated test databases
- **CustomWebApplicationFactory** - Hosts application for integration tests

---

## Author

**Engjell Vlashi**

- GitHub: [@Engjell02](https://github.com/Engjell02)
- Repository: [NightOut-Event-Management](https://github.com/Engjell02/NightOut-Event-Management)

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Last Updated:** February 2026
