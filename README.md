ELRakhawy.Web
🏭 Factory Web App (v1.5)

An internal factory management system built with .NET Core 8, Razor Views, and Onion Architecture.
The application provides full support for stocks management, sales & procurement, authentication, and administration.
🚀 Tech Stack

    Backend: ASP.NET Core 8 (Onion Architecture, Repository & Unit of Work Pattern)
    Frontend: Razor Views (Server-side rendered, responsive, Arabic-first design)
    Database: Microsoft SQL Server (Cloud-hosted with backups & selective encryption)
    Authentication: Identity + Client-side certificate (2FA support)
    Deployment: AWS
    Features: Arabic & Indic number formatting, export to Excel/PDF/Print, WhatsApp integration

📂 Project Architecture (Onion)

├── Core # Entities, Interfaces, Specifications ├── Application # Services, Business logic ├── Infrastructure# Data access (EF Core, Repositories, Unit of Work) ├── WebUI # ASP.NET Core MVC with Razor Views

This layered design ensures separation of concerns, testability, and scalability.
✨ Features
🔐 General

    Full Arabic layout with Indic number support
    Role-based authentication & authorization
    Admin panel for managing accounts, groups, and logs
    Session handling with no concurrent logins per user
    Auto logout after inactivity

📦 Stocks Management

    Manage raw materials, yarns, fabrics, waste, finished products
    DBs and Forms for:
        Manufacturers
        Packaging Styles
        Financial Transaction Types
        Stakeholder Types & Info
        Fabric Styles, Colors, Designs, Studio
    Transactions for Raw, Yarn, Fabric (Inbound, Outbound, Balances)
    Overview tables with export to Excel/PDF/Print
    Inline search in dropdowns & contextual text direction

💰 Sales & Procurement

    Manage suppliers & clients for yarn, raw materials, fabrics
    Define prices per product/company/style
    Payment & service transactions
    Support for invoices, returns, payments, and deductions
    Balance calculations (Credit/Debit)

📑 Modules Overview

    General Requirements
        Database, Authentication, Administration
        Calendar control with full Arabic support
        User session & security policies

    Stocks Management
        Supporting DBs (Manufacturers, Stakeholders, Packaging, Fabric Management)
        Main DBs (Raw, Yarn, Fabric items & transactions)
        Rich form features (overview, search, filtering, extraction, WhatsApp integration)

    Sales & Procurement
        Price management for Yarn, Raw, Fabric
        Supplier & client transactions
        Payment types & services
        Sales & procurement forms with invoice support

📌 Endpoints (API + Razor Actions)

Some actions are exposed as endpoints for integration:

    /api/rawtransactions → Manage raw item transactions
    /api/yarntransactions → Manage yarn item transactions
    /api/fabrictransactions → Manage fabric item transactions
    /api/sales → Sales & procurement endpoints
    /api/auth → Authentication & session handling

(Endpoints can be extended for mobile app / third-party integration)
⚙️ Getting Started
Prerequisites

    .NET 8 SDK
    SQL Server (local or Azure/AWS)
    Visual Studio 2022 / VS Code

Installation

# Clone repository
git clone https://github.com/your-org/factory-web-app.git
cd factory-web-app

# Setup database (update connection string in appsettings.json)
dotnet ef database update

# Run the application
dotnet run --project WebUI
Then open:
👉 https://localhost:5001



## 🛡️ Security Notes  

All API endpoints are secured with JWT + role-based access.

Database uses restricted access and selective encryption.

Transactions are versioned for audit trails.



## License

This project is proprietary and intended for internal factory use only.
