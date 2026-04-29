# Product Management API

A production-style RESTful API built with **.NET 9**, designed using a clean 3-layer architecture (**Repository → Service → Controller**). This project focuses on mastering real-world backend design patterns and best practices before scaling to more complex systems.

---

## 🚀 Tech Stack

| Technology            | Purpose                                         |
| --------------------- | ----------------------------------------------- |
| .NET 8                | Web API framework                               |
| Entity Framework Core | ORM for SQL Server                              |
| FluentValidation      | Request validation                              |
| AutoMapper            | DTO ↔ domain model mapping                      |
| Serilog               | Structured logging (console + rolling files)    |
| JWT Bearer            | Authentication (configured, currently disabled) |

---

## 🏗️ Architecture Overview

The project follows a strict separation of concerns:

```
ProductManagement/
├── ProductManagement.Repository/
│   ├── Data/                   # EF Core DbContext
│   ├── Models/Domain/          # Core domain entities
│   └── Services/               # Repository interfaces & implementations
│
├── ProductManagement.Services/
│   ├── BusinessLogic/          # Service layer (business rules)
│   ├── DTOs/                   # Request/response contracts
│   └── Mappings/               # AutoMapper profiles
│
├── ProductManagement.API/
│   ├── Controllers/            # API endpoints
│   ├── Middleware/             # Exception + request handling
│   ├── Models/Responses/       # Standardized error responses
│   └── Validators/             # FluentValidation rules
│
└── docs/sql/
    └── Database_Setup.sql      # DB schema + seed data
```

---

## 📡 API Endpoints

| Method | Route                | Description                                |
| ------ | -------------------- | ------------------------------------------ |
| GET    | `/api/products`      | Retrieve all products (`?onlyActive=true`) |
| GET    | `/api/products/{id}` | Retrieve a product by ID                   |
| POST   | `/api/products`      | Create a new product                       |
| PUT    | `/api/products/{id}` | Update an existing product                 |
| DELETE | `/api/products/{id}` | Soft delete a product                      |

---

## ⚙️ Getting Started

### 1. Database Setup

Run the SQL script:

```
docs/sql/Database_Setup.sql
```

This will:

* Create the database
* Create the `Products` table
* Insert sample data

---

### 2. Configure Connection String

Update `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=ProductManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

---

### 3. Run the Application

```bash
dotnet build
dotnet run --project ProductManagement.API
```

---

### 4. Swagger UI

Navigate to:

```
https://localhost:{port}/swagger
```

---

## 🧠 Core Design Decisions

### 1. Exception Handling Strategy

* Exceptions are **not handled in Repository or Service layers**
* They bubble up to the Controller
* Controllers translate them into HTTP responses
* A global middleware acts as a fallback for unhandled exceptions (returns 500)

---

### 2. Soft Delete Pattern

* Records are never physically deleted
* `DELETE` sets `IsActive = false`
* Use:

  ```
  ?onlyActive=false
  ```

  to include inactive records

---

### 3. Standardized Error Response

All errors follow a consistent structure:

```json
{
  "errors": [
    {
      "type": "NOT_FOUND_ERROR",
      "code": "1900001",
      "message": "Product with ID '...' was not found."
    }
  ]
}
```

This ensures predictable error handling for frontend consumers.

---

### 4. Transaction ID Tracing

* Each request generates a unique `transactionId`
* Passed through Controller → Service → Repository
* Logged via Serilog for full request traceability

---

## 🔐 Authentication

JWT authentication is fully implemented but currently disabled using `[AllowAnonymous]`.

To enable authentication:

1. Remove `[AllowAnonymous]` from controllers
2. Replace the `SecretKey` in `appsettings.json` with a secure value
3. Never commit real credentials to source control

---

## 🎯 Purpose of This Project

This project is built as **deliberate practice** to:

* Master 3-layer architecture in .NET
* Build production-grade API patterns
* Understand layering, validation, logging, and error handling deeply
* Prepare for real-world enterprise backend systems

---

## 📌 Next Improvements (Optional)

* Enable authentication & role-based authorization
* Add pagination & filtering
* Introduce caching (Redis)
* Implement API versioning

---

## 📄 License

This project is for educational and practice purposes.
