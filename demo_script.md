# Star Publications — 10–15 Minute Demo Script

**Audience:** Instructor  
**Format:** Poster-based system demonstration  
**Goal:** Explain the implementation, design decisions, and answer questions.

---

## Before You Start

- Have the application **running** and open to the **Sales Orders** view.
- Have the **poster** visible beside the screen.
- Have `SalesOrderViewModel.cs` open in Visual Studio ready to show.

---

## Step 1 — Introduction (2 min)

> *Point to the poster header and architecture diagram.*

"This is **Star Publications** — a Windows desktop application for managing the classic *pubs* database.
It's built with **C# 12, WPF, and Entity Framework Core 8** following the **MVVM pattern**.

The app has three subsystems:
- **Sales Order Management** — full create, read, update, and delete
- **Book Search** — browse and filter the title catalogue
- **Publisher Search** — browse publishers and their titles

As you can see on the architecture diagram, the UI layer talks exclusively to ViewModels through data binding.
ViewModels execute commands that hit EF Core, which queries SQL Server, and the results flow back to the View automatically."

---

## Step 2 — Sales Order Management Demo (3 min)

> *Already on the Sales Orders view.*

1. **Load** — "The app loaded all orders automatically on startup — store name and book title are resolved through EF Core navigation properties."

2. **Search** — Type a partial order number (e.g. `6871`) in the Order # field, click **Search**.  
   > "Search uses a LINQ `Contains` query — partial matches work on both Order # and Store ID."

3. **Clear** — Click **Clear** to reset filters and reload all orders.

4. **Add New** — Click **+ Add New**. The inline form slides in.  
   Fill in: Store → `7131`, Order # → `TEST01`, Qty → `5`, Pay Terms → `Net 30`, Title → any.  
   Click **Save**.  
   > "Validation runs first — required fields, max-length checks, and a duplicate composite-key check before writing to the database. The new row appears highlighted at the top."

5. **Edit** — Select the row just added, click **Edit**. Change the Qty. Click **Save**.  
   > "Only date, quantity, and payment terms are editable — the composite key fields are locked."

6. **Delete** — Select the row, click **Delete**. Confirm the dialog.  
   > "A confirmation dialog prevents accidental deletes."

---

## Step 3 — Book Search Demo (3 min)

> *Click **Books** in the navigation bar.*

1. "The full catalogue loads immediately on navigation — six titles with publisher, price, YTD sales, and pub date."

2. **Search by author** — Type a partial author name (e.g. `Green`) in the Author field, click **Search**.

3. **Select a row** — Click any title in the grid.  
   > "The Book Details panel on the right shows all co-authors and all stores where the title has been sold."

4. Click **Clear** to return to the full catalogue.  
   > "All four filters — Title, Type, Publisher, Author — can be used independently or combined."

---

## Step 4 — Publisher Search Demo (2 min)

> *Click **Publishers** in the navigation bar.*

1. "All eight publishers load on arrival — ID, name, city, state, country."

2. **Search by city** — Type `Boston`, click **Search**. One result: New Moon Books.

3. **Select the row** — The Publisher Details panel populates with:
   - Publisher metadata
   - Published titles with prices
   - PR description from the `pub_info` table

4. Click **Clear** to reload all publishers.

---

## Step 5 — Code Walk-through (2 min)

> *Switch to Visual Studio and open `SalesOrderViewModel.cs`.*

"Every view has a corresponding ViewModel. Let me highlight a few patterns:

- **RelayCommand** — each button binding is a `RelayCommand` wrapping an execute lambda and an optional `canExecute` predicate. For example, `EditCommand` is only enabled when a row is selected.

- **TryExecute()** — all database calls are wrapped in this helper, which sets `IsLoading = true`, runs the action, and catches any exception — displaying the message inline rather than crashing.

- **DbContextFactory.Create()** — every operation creates a fresh `PubsDbContext`. This avoids stale-entity tracking across multiple operations.

- **Validate()** — runs before every save, checks required fields and max-length, and shows an inline error message without touching the database."

---

## Step 6 — Q&A (2–3 min)

Common questions and answers:

| Question | Answer |
|---|---|
| Why MVVM? | Views are pure XAML — zero code-behind. ViewModels are independently testable and reusable. |
| Why a new DbContext per operation? | Prevents stale change-tracking state. EF Core contexts are lightweight to create. |
| Why composite primary keys? | The real *pubs* schema uses composite PKs on `sales` and `titleauthor`. EF Core's Fluent API configures them with `HasKey`. |
| Why `RelayCommand` instead of built-in commands? | WPF's `RoutedCommand` is UI-coupled. `RelayCommand` is a plain C# class — any method becomes a command in one line. |
| Why does `MainViewModel` own the child VMs? | It aggregates the status bar message from all three subsystems and drives navigation without a navigation service. |
| How does the DB get set up? | Run `Database/setup_pubs.sql` against any SQL Server instance. The script is idempotent — safe to re-run. |

---

## Backup Talking Points

- The `appsettings.json` connection string is loaded via `Microsoft.Extensions.Configuration` — the same pattern used in ASP.NET Core, making it easy to swap between LocalDB and a full SQL Server instance.
- `BaseViewModel` implements `INotifyPropertyChanged` and provides a `SetProperty` helper that only raises the event when the value actually changes, preventing unnecessary UI updates.
- The `DataTemplate` mappings in `MainWindow.xaml` tell WPF which View to render for each ViewModel type — this is how single-window navigation works with zero frames or pages.
