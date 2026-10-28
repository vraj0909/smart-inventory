# Smart Inventory & Stock Management System
**Domain:** Small Business / Retail Management  
**Architecture:** ASP.NET Core MVC (.NET 10 / 8 compatible)  
**Database:** MySQL integration via `Pomelo.EntityFrameworkCore.MySql` (with automated seeding and resilient demo fallback)

---

## 🎯 Real-World Problem Solved
Small retail stores frequently struggle with:
1. **Stock Shortages & Out-of-Stock Lost Sales:** Lack of automated reorder warnings causes stockouts on high-demand merchandise.
2. **Unorganized Supplier Data:** Contact records, order leads, and supplier-product associations are scattered across disconnected spreadsheets.
3. **Tracking Low Inventory Levels:** Manual stock counts fail to alert store managers before critical thresholds are breached.
4. **Session-Based Alerts & Audit Handovers:** Staff shifts lack clear session-based tracking of recent adjustments, drafts, and inventory mutations.

This system provides an end-to-end retail management solution that resolves each of these challenges.

---

## 📋 Rubric Compliance & Feature Breakdown

| Rubric Requirement | Implementation Detail | Location |
| :--- | :--- | :--- |
| **MVC Architecture** | Strict separation of Models, Views, and Controllers. Business logic in Controllers, typed ViewModels, Razor Views. | `Models/`, `ViewModels/`, `Controllers/`, `Views/` |
| **Database & EF Core** | `Pomelo.EntityFrameworkCore.MySql` integration with foreign keys, indexes, decimal precisions, and fluent configurations. Includes ready-to-run `Database_Script.sql`. | `Data/InventoryDbContext.cs`, `Data/DbInitializer.cs`, `Program.cs`, `Database_Script.sql` |
| **Full CRUD** | Complete Create, Read, Update, Delete for Products, Stock Movements, Categories, and Suppliers. Includes search, filter, and pagination. | `Controllers/ProductsController.cs`, `Views/Products/` |
| **Form Handling & Model Validation** | Client & Server validation with DataAnnotations (`[Required]`, `[Range]`, `[StringLength]`, `[RegularExpression]`, `[EmailAddress]`, `[Phone]`) + `_ValidationScriptsPartial.cshtml`. | `Models/Product.cs`, `Models/Supplier.cs`, `Views/Products/Create.cshtml`, `Edit.cshtml` |
| **State Management** | `HttpContext.Session` tracks live session action count, last-action timestamp, and a Restock Order Draft cart; `TempData` powers dismissible flash alerts. | `Common/SessionExtensions.cs`, `Controllers/RestockController.cs`, `Views/Shared/_NotificationPartial.cshtml`, `_Layout.cshtml` |
| **Responsive UI** | Clean Bootstrap 5.3 interface with KPI cards, status badges (In Stock / Low Stock / Out of Stock), category breakdown, stock level gauges, and modals. | `wwwroot/css/site.css`, `Views/Shared/_Layout.cshtml`, `Views/Home/Index.cshtml` |

---

## 🚀 Quick Start Guide

### 1. Prerequisites
- **.NET SDK** (.NET 8.0 or 10.0) — verified with `dotnet --version`.
- **VS Code** with C# Dev Kit or OmniSharp.
- **MySQL Server** (via XAMPP, MySQL Workbench, or Docker) on default port `3306`.

### 2. Configure MySQL Database
The connection string in [`appsettings.json`](file:///c:/Users/Admin/OneDrive/Desktop/smart%20inventory%20and%20stock/appsettings.json) defaults to:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=smart_inventory_db;User=root;Password=;CharSet=utf8mb4;"
}
```
- If using **XAMPP**: Start the MySQL service from the XAMPP Control Panel.
- If using **MySQL Workbench / CLI**: Ensure root user credentials match or update `appsettings.json`.
- A standalone SQL script [`Database_Script.sql`](file:///c:/Users/Admin/OneDrive/Desktop/smart%20inventory%20and%20stock/Database_Script.sql) is also provided in the root directory for direct import into phpMyAdmin or MySQL Workbench.
- *Note:* If MySQL is not running when the app starts, the app features an automatic fallback to an In-Memory demo store so you can evaluate the UI and flows immediately without crashes.

### 3. Run the Application
Open terminal in the project directory:
```bash
dotnet run
```
Navigate in your browser to:
**`http://localhost:5279`**

---

## 📦 Initial Pre-Seeded Dataset
The application automatically seeds realistic retail items to showcase inventory management:
- **Categories:** Electronics & Gadgets, Office & Stationery, Hardware & Tools, Snacks & Refreshments, Packaging Materials.
- **Suppliers:** Apex Tech Distribution, Evergreen Office Supply Co., Vanguard Tools & Fasteners, Metro Wholesale Refreshments.
- **Products:** 12 retail items with varied stock statuses (healthy stock, low stock alert items, and out-of-stock items) to test the alert systems immediately.
- **Stock Audit Ledger:** Pre-populated historical stock in/out transactions with staff attribution.

---

## 🛠️ Key User Flows
1. **Executive Dashboard (`/`):** KPI metrics, critical low-stock priority watchlist, category distribution, and live session state telemetry.
2. **Products Catalog (`/Products`):** Multi-filter search (search keywords, category dropdown, supplier dropdown, quick stock pills) + CSV export + CRUD.
3. **Stock Movements (`/Stock`):** Comprehensive audit ledger of all stock intakes and dispatches.
4. **Low Stock Alerts (`/Stock/LowStock`):** Dedicated page identifying inventory deficits with 1-click restock additions.
5. **Session Restock Plan (`/Restock`):** Session-backed purchase order planner where items can be reviewed, adjusted, and fulfilled into live inventory.
