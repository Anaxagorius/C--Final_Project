# Star Publications

A Windows desktop application for managing the classic **pubs** database. Built with C#, WPF, and Entity Framework Core following the MVVM pattern, the app provides three fully functional subsystems: Sales Order Management, Book Search, and Publisher Search.

---

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Database Setup](#database-setup)
- [Configuration](#configuration)
- [Building and Running](#building-and-running)
- [Usage](#usage)
- [Technology Stack](#technology-stack)

---

## Features

### Sales Order Management
- View all sales orders in a sortable grid with store name and title linked via navigation properties
- **Search** orders by order number or store ID
- **Add** new sales orders with validation (required fields, max lengths, duplicate composite-key detection)
- **Edit** existing orders (date, quantity, payment terms)
- **Delete** orders with a confirmation dialog
- Inline edit form that slides in/out without leaving the view

### Book Search
- Browse the full catalogue of titles on load
- **Search** by title text, genre/type, publisher name, or author name (partial matches supported)
- Select a title to see its **detail panel**: authors and stores where it has been sold
- Clear search returns to the full catalogue

### Publisher Search
- Browse all publishers on load
- **Search** by name, city, state, or country (partial matches supported)
- Select a publisher to see its **detail panel**: titles it has published (with prices) and PR information

### General
- Persistent status bar showing the result of the last operation across all views
- Busy/loading indicator during database operations
- Inline error messages for validation failures and database exceptions
- Navigation bar for instant switching between the three subsystems

---

## Architecture

The application uses the **Model-View-ViewModel (MVVM)** pattern:

| Layer | Responsibility |
|---|---|
| **Models** (`Models/`) | Plain C# classes decorated with EF Core data annotations, mapping 1-to-1 with database tables |
| **Data** (`Data/`) | `PubsDbContext` (EF Core DbContext) and `DbContextFactory` (creates contexts from `appsettings.json`) |
| **ViewModels** (`ViewModels/`) | Observable state, commands, and database operations for each view; extend `BaseViewModel` which implements `INotifyPropertyChanged` |
| **Views** (`Views/`) | XAML-only UI; bound exclusively to ViewModels via data binding — no code-behind logic |
| **Commands** (`Commands/`) | `RelayCommand` — a generic `ICommand` implementation that accepts execute/canExecute delegates |

`MainViewModel` owns the three child ViewModels and swaps them in and out of a `ContentControl` via WPF `DataTemplate` mappings, providing single-window navigation without frames or pages.

---

## Project Structure

```
C--Final_Project/
├── Database/
│   └── setup_pubs.sql          # SQL Server script to create and seed the pubs database
└── StarPublications/
    ├── Commands/
    │   └── RelayCommand.cs
    ├── Converters/              # WPF value converters (e.g. bool-to-visibility)
    ├── Data/
    │   ├── DbContextFactory.cs  # Reads connection string and creates PubsDbContext
    │   └── PubsDbContext.cs     # EF Core DbContext with Fluent API configuration
    ├── Models/
    │   ├── Author.cs
    │   ├── PubInfo.cs
    │   ├── Publisher.cs
    │   ├── Sale.cs              # Composite PK: stor_id + ord_num + title_id
    │   ├── Store.cs
    │   ├── Title.cs
    │   └── TitleAuthor.cs       # Composite PK: au_id + title_id
    ├── Resources/               # Styles, brushes, and control templates
    ├── ViewModels/
    │   ├── BaseViewModel.cs     # INotifyPropertyChanged base + SetProperty helper
    │   ├── BookSearchViewModel.cs
    │   ├── MainViewModel.cs     # Navigation and status aggregation
    │   ├── PublisherSearchViewModel.cs
    │   └── SalesOrderViewModel.cs
    ├── Views/
    │   ├── BookSearchView.xaml
    │   ├── MainWindow.xaml
    │   ├── PublisherSearchView.xaml
    │   └── SalesOrderView.xaml
    ├── appsettings.json         # Connection string configuration
    ├── App.xaml                 # Application resources and startup
    └── StarPublications.csproj
```

---

## Prerequisites

| Requirement | Version |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0 or later |
| Windows OS | Required (WPF target: `net8.0-windows`) |
| [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or SQL Server Express / LocalDB | Any recent version |

> **Note:** The default connection string targets `(localdb)\MSSQLLocalDB`, which ships with Visual Studio. If you are using a different SQL Server instance, update `appsettings.json` before running.

---

## Database Setup

1. Open **SQL Server Management Studio** (SSMS), **Azure Data Studio**, or `sqlcmd`.
2. Run the setup script against your SQL Server instance:

   ```sql
   -- Using sqlcmd:
   sqlcmd -S "(localdb)\MSSQLLocalDB" -i Database/setup_pubs.sql
   ```

   Or open `Database/setup_pubs.sql` in SSMS and execute it.

3. The script is idempotent — it creates the `pubs` database and all tables only if they do not already exist, and inserts sample data for publishers, titles, authors, stores, and sales.

---

## Configuration

The database connection string is stored in `StarPublications/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "PubsDatabase": "Server=(localdb)\\MSSQLLocalDB;Database=pubs;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Update the `Server` value to point to your SQL Server instance if it differs from the LocalDB default.

---

## Building and Running

### Visual Studio

1. Open `StarPublications.slnx` in Visual Studio 2022 or later.
2. Ensure the startup project is **StarPublications**.
3. Press **F5** (Debug) or **Ctrl+F5** (Run without debugging).

### .NET CLI

```bash
cd StarPublications
dotnet build
dotnet run
```

---

## Usage

When the application starts, it opens directly to the **Sales Orders** view and loads all existing orders.

### Navigating between views

Use the navigation bar at the top of the window to switch between:

- **🛒 Sales Orders** — create, read, update, and delete sales records
- **📖 Books** — search the book catalogue and inspect title details
- **🏢 Publishers** — search publishers and view their titles and PR info

### Sales Order Management

| Action | How |
|---|---|
| Search | Enter text in the Order Number or Store ID fields and click **Search** |
| Clear search | Click **Clear** to reset filters and reload all orders |
| Add new order | Click **Add New**, fill in the form, and click **Save** |
| Edit an order | Select a row, click **Edit**, modify the fields, and click **Save** |
| Delete an order | Select a row and click **Delete** (confirmation required) |
| Cancel editing | Click **Cancel** to discard changes |

### Book Search

| Action | How |
|---|---|
| Search | Enter any combination of title, type, publisher, or author and click **Search** |
| View details | Click any row in the results grid to show authors and store availability |
| Reset | Click **Clear** to clear filters and reload all titles |

### Publisher Search

| Action | How |
|---|---|
| Search | Enter any combination of name, city, state, or country and click **Search** |
| View details | Click any row to display the publisher's titles and PR description |
| Reset | Click **Clear** to clear filters and reload all publishers |

---

## Technology Stack

| Technology | Purpose |
|---|---|
| C# 12 / .NET 8 | Application language and runtime |
| WPF (Windows Presentation Foundation) | UI framework |
| Entity Framework Core 8 | ORM for SQL Server data access |
| Microsoft.Extensions.Configuration | `appsettings.json` configuration loading |
| MVVM pattern | Separation of UI and business logic |
| SQL Server / LocalDB | Relational database (classic pubs schema) |