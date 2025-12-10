# 🏭 ELRakhawy Factory Management System

A comprehensive internal factory management system built with **ASP.NET Core 8**, **Razor Views**, and **Clean Onion Architecture**. This enterprise-grade application provides complete support for textile factory operations including **stocks management**, **sales & procurement**, **authentication**, and **administration**.

[![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Proprietary-red.svg)](LICENSE)

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Architecture](#-architecture)
- [Technology Stack](#-technology-stack)
- [Project Structure](#-project-structure)
- [Key Features](#-key-features)
- [Database Schema](#-database-schema)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [Security](#-security)
- [API Documentation](#-api-documentation)
- [Development](#-development)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 Overview

ELRakhawy Factory Management System is a specialized web application designed for textile manufacturing operations. It manages the complete lifecycle of factory materials from raw materials to finished fabrics, including yarn processing, weaving operations, and financial transactions.

**Key Capabilities:**
- 📦 **Stock Management**: Raw materials, yarn items, fabric inventory, and full warp beam tracking
- 💰 **Financial Operations**: Sales, procurement, payments, and transaction tracking
- 👥 **Stakeholder Management**: Manufacturers, suppliers, clients, and stakeholder information
- 🔐 **Security**: Role-based authentication with session management and single-login enforcement
- 📊 **Reporting**: Excel/PDF export capabilities for all major entities
- 🌐 **Arabic-First Design**: Full RTL support with Arabic and Indic number formatting

---

## 🏗️ Architecture

The application follows **Clean Onion Architecture** principles with clear separation of concerns across three main layers:

```
┌─────────────────────────────────────────────────────┐
│           ELRakhawy.Web (Presentation)              │
│   Controllers, Views, ViewModels, Middleware        │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│      ELRakhawy.DAL (Data Access Layer)              │
│   DbContext, Repositories, UnitOfWork, Services     │
│   Migrations, Persistence, Security                 │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│        ELRakhawy.EL (Entity Layer / Core)           │
│   Models, Interfaces, ViewModels                    │
└─────────────────────────────────────────────────────┘
```

### Layer Responsibilities

1. **ELRakhawy.Web** (Presentation Layer)
   - ASP.NET Core MVC Controllers
   - Razor Views with Bootstrap 5 (RTL)
   - Authentication & Authorization
   - Session Management Middleware
   - Helper utilities

2. **ELRakhawy.DAL** (Data Access Layer)
   - Entity Framework Core 8.0.19
   - Generic Repository Pattern
   - Unit of Work Pattern
   - Database Migrations (26+ migrations)
   - Security implementations (Password Hashing, Single Session Middleware)
   - Database seeding

3. **ELRakhawy.EL** (Entity Layer)
   - Domain Models (21+ entities)
   - Repository Interfaces
   - ViewModels for data transfer
   - Business contracts

---

## 🚀 Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **Language**: C# 12 (.NET 8)
- **ORM**: Entity Framework Core 8.0.19
- **Database**: Microsoft SQL Server
- **Authentication**: Cookie-based Authentication with custom middleware
- **Patterns**: Repository Pattern, Unit of Work, Dependency Injection

### Frontend
- **View Engine**: Razor Pages
- **CSS Framework**: Bootstrap 5 (RTL support)
- **JavaScript**: jQuery, jQuery Validation
- **Icons**: Bootstrap Icons / Font Awesome

### Key NuGet Packages
```xml
<!-- Web Layer -->
<PackageReference Include="EPPlus" Version="8.1.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.19" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.19" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.19" />

<!-- DAL Layer -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.Cookies" Version="2.3.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.19" />
```

### Development Tools
- **Entity Framework CLI**: dotnet-ef 9.0.8
- **IDE**: Visual Studio 2022 / VS Code
- **Version Control**: Git

---

## 📂 Project Structure

```
ELRakhawy.Web/
├── .git/                           # Git repository
├── .gitignore                      # Git ignore rules
├── ELRakhawy.Web.sln              # Solution file
├── README.md                       # This file
│
├── ELRakhawy.Web/                 # 🌐 Web Application (Presentation Layer)
│   ├── Controllers/               # MVC Controllers (20 controllers)
│   │   ├── AuthController.cs      # Authentication & Login
│   │   ├── UserController.cs      # User management
│   │   ├── YarnTransactionsController.cs
│   │   ├── RawTransactionsController.cs
│   │   ├── FabricStudiosController.cs
│   │   ├── FullWarpBeamTransationsController.cs
│   │   └── ... (16+ more controllers)
│   │
│   ├── Views/                     # Razor Views (22 view folders)
│   │   ├── Auth/                  # Login, Register views
│   │   ├── Shared/                # Layout, partials
│   │   ├── YarnTransactions/      # Yarn transaction views
│   │   ├── RawTransactions/       # Raw material views
│   │   ├── FabricStudios/         # Fabric management
│   │   └── ... (18+ more view folders)
│   │
│   ├── Models/                    # View-specific models
│   ├── Helper/                    # Helper classes
│   ├── wwwroot/                   # Static files (CSS, JS, libraries)
│   │   ├── css/                   # Custom stylesheets
│   │   ├── js/                    # Custom JavaScript
│   │   └── lib/                   # Bootstrap, jQuery, etc.
│   │
│   ├── Program.cs                 # Application entry point
│   ├── appsettings.json          # Configuration (git-ignored)
│   └── ELRakhawy.Web.csproj      # Project file
│
├── ELRakhawy.DAL/                 # 💾 Data Access Layer
│   ├── Data/
│   │   └── AppDBContext.cs        # EF Core DbContext (16+ DbSets)
│   │
│   ├── Implementations/           # Repository implementations
│   │   ├── GenericRepository.cs   # Generic CRUD operations
│   │   ├── UnitOfWork.cs          # Transaction management
│   │   └── UserRepository.cs      # User-specific operations
│   │
│   ├── Services/
│   │   └── AuthService.cs         # Authentication business logic
│   │
│   ├── Security/
│   │   ├── PasswordHasher.cs      # Password hashing utility
│   │   └── SingleSessionMiddleware.cs  # Prevent concurrent logins
│   │
│   ├── Persistence/
│   │   └── DbSeeder.cs            # Database seeding
│   │
│   ├── Migrations/                # EF Core Migrations (26+ files)
│   └── ELRakhawy.DAL.csproj      # Project file
│
└── ELRakhawy.EL/                  # 🎯 Entity Layer (Domain)
    ├── Models/                    # Domain entities (21 models)
    │   ├── AppUser.cs             # User entity
    │   ├── YarnItem.cs            # Yarn inventory
    │   ├── YarnTransaction.cs     # Yarn operations
    │   ├── RawItem.cs             # Raw materials
    │   ├── RawTransaction.cs      # Raw material operations
    │   ├── FabricItem.cs          # Fabric inventory
    │   ├── FabricStudio.cs        # Fabric production details
    │   ├── FullWarpBeam.cs        # Warp beam management
    │   ├── Manufacturers.cs       # Manufacturer data
    │   ├── StakeholdersInfo.cs    # Stakeholder records
    │   └── ... (11+ more models)
    │
    ├── Interfaces/                # Repository contracts
    │   ├── IGenericRepository.cs  # Generic CRUD interface
    │   ├── IUnitOfWork.cs         # UoW interface
    │   └── IUserRepository.cs     # User repository interface
    │
    ├── ViewModels/                # Data transfer objects (15+ ViewModels)
    │   ├── LoginViewModel.cs
    │   ├── RegisterViewModel.cs
    │   ├── YarnTransactionViewModel.cs
    │   ├── RawTransactionViewModel.cs
    │   └── ... (11+ more ViewModels)
    │
    └── ELRakhawy.EL.csproj        # Project file
```

---

## ✨ Key Features

### 🔐 Authentication & Authorization

- **Cookie-based Authentication** with ASP.NET Core Identity
- **Role-Based Access Control** (SuperAdmin, Admin, User roles)
- **Single Session Enforcement**: Prevents concurrent logins per user
- **Session Management**: 
  - 30-minute idle timeout
  - 20-minute authentication expiration
  - Sliding expiration enabled
- **Password Security**: 
  - Secure password hashing (PasswordHasher)
  - Admin-only password reset capability
- **Auto-Logout**: Automatic logout after inactivity

**User Roles:**
- `SuperAdmin`: Full system access, user management, password resets
- `Admin`: Management operations, reporting
- `User`: Standard operational access

### 📦 Inventory Management

#### Yarn Management
- **Yarn Items**: Track yarn types with multiple manufacturers
- **Yarn Transactions**: 
  - Inbound (receiving) transactions
  - Outbound (consumption) transactions
  - Balance calculations
  - Origin yarn tracking (derived yarn relationships)
- **Full Warp Beam Management**: Track warp beams with transactions
- **Excel Export**: Export yarn inventory and transactions

#### Raw Materials Management
- **Raw Items**: Catalog of raw materials
- **Raw Transactions**: 
  - Inbound raw material receiving
  - Outbound consumption tracking
  - Real-time balance calculations
- **Transaction History**: Complete audit trail
- **PDF/Excel Export**: Generate reports

#### Fabric Management
- **Fabric Items**: Complete fabric inventory
- **Fabric Studios**: Detailed fabric production records including:
  - Design and color information
  - Measurements (width, length, weight)
  - Quality specifications
  - Production tracking
- **Fabric Attributes**:
  - Colors (FabricColor)
  - Designs (FabricDesign)
  - Styles (FabricStyle)
- **Rich filtering and search capabilities**

### 🏭 Manufacturing Support

- **Manufacturers Database**: Manage manufacturer information with contact details
- **Packaging Styles**: Define packaging types and forms
- **Form Styles**: Customizable form templates
- **Financial Transaction Types**: Categorize transactions (Credit/Debit)

### 👥 Stakeholder Management

- **Stakeholder Types**: Categorize stakeholders (Supplier, Client, etc.)
- **Stakeholder Information**: Complete contact and business details
- **Form-Based Management**: Dynamic forms based on stakeholder type
- **Relationship Tracking**: Link stakeholders to transactions

### 📊 Reporting & Export

- **Excel Export**: Generate comprehensive reports using EPPlus
- **PDF Generation**: Create printable documents
- **Print Support**: Browser-based printing
- **Overview Dashboards**: 
  - Transaction summaries
  - Balance sheets
  - Inventory levels

### 🌐 Internationalization

- **Arabic-First Interface**: Complete RTL (Right-to-Left) support
- **Indic Number System**: Display numbers in Arabic-Indic format
- **Contextual Text Direction**: Automatic text direction based on content
- **Date Formatting**: Hijri calendar support
- **Bootstrap RTL**: Full RTL CSS framework

### 🔧 Technical Features

- **Responsive Design**: Mobile-friendly interface
- **AJAX Operations**: Dynamic form updates without page refresh
- **Inline Search**: Quick filtering in dropdowns
- **Form Validation**: Client and server-side validation
- **Error Handling**: Comprehensive error pages
- **Logging**: Built-in ASP.NET Core logging

---

## 🗄️ Database Schema

### Core Entities

#### User Management
- **AppUser**: User accounts with role-based access
  - Fields: Id, FirstName, LastName, Email, PasswordHash, Role, CurrentSessionToken, CreatedAt

#### Inventory Entities
- **YarnItem**: Yarn inventory with manufacturer relationships
  - Self-referential: OriginYarn → DerivedYarns
  - Many-to-many: Manufacturers ↔ YarnItems
- **YarnTransaction**: Yarn movement tracking
- **RawItem**: Raw material catalog
- **RawTransaction**: Raw material operations
- **FabricItem**: Fabric inventory
- **FabricStudio**: Detailed fabric production records
- **FullWarpBeam**: Warp beam tracking
- **FullWarpBeamTransaction**: Warp beam operations

#### Support Entities
- **Manufacturers**: Factory and supplier information
- **PackagingStyles**: Packaging definitions
- **FormStyle**: Form templates
- **PackagingStyleForms**: Many-to-many relationship
- **FinancialTransactionType**: Transaction categorization (seeded: دائن/مدين)

#### Stakeholder Entities
- **StakeholderType**: Stakeholder categories
- **StakeholdersInfo**: Stakeholder contact information
- **StakeholderTypeForm**: Form-type relationships
- **StakeholderInfoType**: Information classification

#### Fabric Entities
- **FabricColor**: Color catalog
- **FabricDesign**: Design patterns
- **FabricStyle**: Style definitions

### Entity Relationships

```
AppUser (1) → (N) YarnTransaction
YarnItem (1) → (N) YarnTransaction
YarnItem (1) → (N) YarnItem (OriginYarn relationship)
YarnItem (N) ↔ (N) Manufacturers
PackagingStyle (N) ↔ (N) FormStyle (via PackagingStyleForms)
StakeholderType (N) ↔ (N) FormStyle (via StakeholderTypeForm)
FabricStudio → FabricItem, FabricColor, FabricDesign, FabricStyle
```

---

## 🚀 Getting Started

### Prerequisites

- **.NET 8 SDK** or later ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **SQL Server** (2019 or later recommended)
  - SQL Server Express (local development)
  - Azure SQL Database (cloud deployment)
  - AWS RDS SQL Server (cloud deployment)
- **Visual Studio 2022** or **VS Code** with C# extension
- **Git** for version control

### Installation

1. **Clone the Repository**
   ```bash
   git clone https://github.com/Ammar-Yasser8/ELRakhawy.Web.git
   cd ELRakhawy.Web
   ```

2. **Configure Database Connection**
   
   Create `appsettings.json` in `ELRakhawy.Web` folder:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=ELRakhawyDB;Trusted_Connection=True;TrustServerCertificate=True;"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*"
   }
   ```

3. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

4. **Apply Database Migrations**
   ```bash
   cd ELRakhawy.Web
   dotnet ef database update --project ../ELRakhawy.DAL
   ```
   
   This will:
   - Create the database
   - Apply all 26+ migrations
   - Seed initial data (FinancialTransactionTypes, default users)

5. **Run the Application**
   ```bash
   dotnet run --project ELRakhawy.Web
   ```
   
   Or using Visual Studio:
   - Open `ELRakhawy.Web.sln`
   - Set `ELRakhawy.Web` as startup project
   - Press F5 to run

6. **Access the Application**
   ```
   🌐 HTTPS: https://localhost:5001
   🌐 HTTP: http://localhost:5000
   ```

### Default Login Credentials

After database seeding, use these credentials:
```
Email: admin@elrakhawy.com
Password: Admin@123
```

*(Credentials are created by DbSeeder.cs - check the file for exact values)*

---

## ⚙️ Configuration

### Application Settings

The application is configured through `appsettings.json` (git-ignored for security):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=ELRakhawyDB;User Id=user;Password=pass;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Session Configuration

Configured in `Program.cs`:
```csharp
builder.Services.AddSession(builder =>
{
    builder.IdleTimeout = TimeSpan.FromMinutes(30);
    builder.Cookie.HttpOnly = true;
    builder.Cookie.IsEssential = true;
    builder.Cookie.Name = "ELRakhawy.Session";
});
```

### Authentication Configuration

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Denied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.Cookie.IsEssential = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });
```

---

## 🛡️ Security

### Authentication & Authorization

- **Password Hashing**: Industry-standard password hashing using `PasswordHasher` utility
- **Cookie Security**: 
  - HttpOnly cookies (XSS protection)
  - SameSite=Strict (CSRF protection)
  - Secure flag in production
- **Single Session Enforcement**: `SingleSessionMiddleware` prevents concurrent logins
- **Session Tokens**: Unique session tokens stored in database

### Role-Based Access Control

```csharp
[Authorize(Roles = "SuperAdmin")]  // SuperAdmin only
[Authorize(Roles = "Admin,SuperAdmin")]  // Admins and SuperAdmins
[Authorize]  // All authenticated users
```

### Data Protection

- **Connection String Security**: Stored in appsettings.json (git-ignored)
- **SQL Injection Prevention**: EF Core parameterized queries
- **XSS Protection**: Razor automatic encoding
- **CSRF Protection**: Built-in antiforgery tokens

### Best Practices Implemented

✅ HTTPS enforcement in production  
✅ Secure password policies  
✅ Session timeout management  
✅ Input validation (client and server)  
✅ Error handling without information disclosure  
✅ Logging for audit trails  
✅ Database connection pooling  

---

## 📡 API Documentation

### Controller Overview

The application has 20 controllers organized by domain:

#### Authentication
- **AuthController**: Login, Register, Logout, Access Denied

#### User Management
- **UserController**: User CRUD, Role management, Password reset

#### Yarn Management
- **YarnItemsController**: Yarn inventory management
- **YarnTransactionsController**: Inbound/Outbound operations, Balance overview
- **FullWarpBeamController**: Warp beam management
- **FullWarpBeamTransationsController**: Warp beam transactions

#### Raw Materials
- **RawItemsController**: Raw material catalog
- **RawTransactionsController**: Raw material transactions

#### Fabric Management
- **FabricItemsController**: Fabric inventory
- **FabricStudiosController**: Fabric production details
- **FabricColorsController**: Color management
- **FabricDesignsController**: Design management
- **FabricStylesController**: Style management

#### Support Data
- **ManufacturersController**: Manufacturer management
- **PackagingStylesController**: Packaging definitions
- **FormStylesController**: Form template management
- **StakeholderTypesController**: Stakeholder categories
- **StakeholdersInfoController**: Stakeholder information

#### System
- **HomeController**: Dashboard and home page

### Common Routes Pattern

```
GET    /{Controller}/Index              - List all items
GET    /{Controller}/Create             - Create form
POST   /{Controller}/Create             - Create action
GET    /{Controller}/Edit/{id}          - Edit form
POST   /{Controller}/Edit/{id}          - Update action
POST   /{Controller}/Delete/{id}        - Delete action
GET    /{Controller}/Details/{id}       - View details
GET    /{Controller}/Export             - Export to Excel
```

### Special Routes

```
# Authentication
GET/POST  /Auth/Login                    - User login
GET/POST  /Auth/Register                 - User registration
GET       /Auth/Logout                   - User logout
GET       /Auth/Denied                   - Access denied page

# Yarn Transactions
GET       /YarnTransactions/Inbound      - Inbound transaction form
POST      /YarnTransactions/Inbound      - Process inbound
GET       /YarnTransactions/Outbound     - Outbound transaction form
POST      /YarnTransactions/Outbound     - Process outbound
GET       /YarnTransactions/Overview     - Transaction overview

# Similar patterns for RawTransactions, FullWarpBeamTransations
```

---

## 💻 Development

### Building the Solution

```bash
# Restore dependencies
dotnet restore

# Build all projects
dotnet build

# Build specific project
dotnet build ELRakhawy.Web/ELRakhawy.Web.csproj

# Build for production
dotnet build -c Release
```

### Database Operations

```bash
# Add new migration
dotnet ef migrations add MigrationName --project ELRakhawy.DAL --startup-project ELRakhawy.Web

# Update database
dotnet ef database update --project ELRakhawy.DAL --startup-project ELRakhawy.Web

# Rollback migration
dotnet ef database update PreviousMigrationName --project ELRakhawy.DAL --startup-project ELRakhawy.Web

# Drop database
dotnet ef database drop --project ELRakhawy.DAL --startup-project ELRakhawy.Web
```

### Code Organization Guidelines

1. **Controllers**: Keep controllers thin, delegate business logic to services
2. **ViewModels**: Use ViewModels for data transfer, never expose entities directly
3. **Repository Pattern**: All data access through repositories
4. **Unit of Work**: Use UnitOfWork for transaction management
5. **Dependency Injection**: Constructor injection for all dependencies

### Naming Conventions

- **Controllers**: `{Entity}Controller.cs` (e.g., `YarnItemsController.cs`)
- **Views**: `/{Controller}/{Action}.cshtml`
- **Models**: `{Entity}.cs` in ELRakhawy.EL/Models
- **ViewModels**: `{Entity}ViewModel.cs` in ELRakhawy.EL/ViewModels
- **Repositories**: `I{Entity}Repository.cs` (interface), `{Entity}Repository.cs` (implementation)

### Development Environment Setup

1. **Visual Studio 2022**:
   - Install ASP.NET and web development workload
   - Install .NET 8 SDK
   - Install SQL Server Express LocalDB

2. **VS Code**:
   - Install C# extension
   - Install .NET Core Test Explorer
   - Install SQL Server (mssql) extension

3. **Database Tools**:
   - SQL Server Management Studio (SSMS)
   - Azure Data Studio
   - DB Browser for connection testing

---

## 🤝 Contributing

This is a proprietary project. For internal contributions:

1. **Branch Naming**: `feature/{feature-name}` or `bugfix/{bug-name}`
2. **Commit Messages**: Use descriptive commit messages
3. **Code Review**: All changes require code review
4. **Testing**: Test thoroughly before submitting
5. **Documentation**: Update documentation for new features

---

## 📄 License

**Proprietary Software**

© 2024 Ammar Yasser & VecorEG. All rights reserved.

This software is proprietary and confidential. Unauthorized copying, distribution, or use of this software, via any medium, is strictly prohibited.

---

## 👨‍💻 Author & Contact

**Ammar Yasser**  
- LinkedIn: [Ammar Yasser](https://www.linkedin.com/in/ammar-yasser-a01772250/)
- Organization: VecorEG

---

## 📈 Project Statistics

- **Total C# Files**: 101+
- **Controllers**: 20
- **Domain Models**: 21
- **ViewModels**: 15+
- **Migrations**: 26+
- **View Folders**: 22
- **.NET Version**: 8.0
- **Entity Framework Core**: 8.0.19

---

## 🎯 Roadmap

### Planned Features
- [ ] API endpoints for mobile integration
- [ ] Advanced reporting dashboard
- [ ] Email notifications
- [ ] WhatsApp integration enhancement
- [ ] Multi-language support (English/Arabic toggle)
- [ ] Barcode/QR code support for inventory
- [ ] Advanced analytics and charts
- [ ] Automated backup system
- [ ] Audit log viewer

---

**Last Updated**: December 2024  
**Version**: 1.3

