# Finance Tracker — Implementation Plan

Work through each phase in order. Complete and verify each phase before starting the next. The spec in `spec.md` is the authoritative source for model shapes, interfaces, and schema SQL — reference it constantly.

---

## Phase 1 — Solution Scaffold

**Goal:** A compiling, empty solution with all five projects and correct project references.

### Steps

1. Create the solution and projects from the repo root:

```bash
dotnet new sln -n FinanceTracker
dotnet new classlib -n FinanceTracker.Core    -o FinanceTracker.Core    -f net10.0
dotnet new classlib -n FinanceTracker.Data    -o FinanceTracker.Data    -f net10.0
dotnet new blazorwasm -n FinanceTracker.Web   -o FinanceTracker.Web     -f net10.0
# Actually use blazor web app (server-side interactive):
# dotnet new blazor -n FinanceTracker.Web -o FinanceTracker.Web -f net10.0 --interactivity Server
dotnet new console -n FinanceTracker.Console  -o FinanceTracker.Console  -f net10.0
dotnet new xunit   -n FinanceTracker.Tests    -o FinanceTracker.Tests    -f net10.0
```

> For the web project, use `dotnet new blazor --interactivity Server` to create a Blazor Web App with Interactive Server rendering (not WASM).

2. Add all projects to the solution:

```bash
dotnet sln add FinanceTracker.Core/FinanceTracker.Core.csproj
dotnet sln add FinanceTracker.Data/FinanceTracker.Data.csproj
dotnet sln add FinanceTracker.Web/FinanceTracker.Web.csproj
dotnet sln add FinanceTracker.Console/FinanceTracker.Console.csproj
dotnet sln add FinanceTracker.Tests/FinanceTracker.Tests.csproj
```

3. Wire up project references:

```bash
dotnet add FinanceTracker.Data/FinanceTracker.Data.csproj reference FinanceTracker.Core/FinanceTracker.Core.csproj
dotnet add FinanceTracker.Web/FinanceTracker.Web.csproj reference FinanceTracker.Core/FinanceTracker.Core.csproj
dotnet add FinanceTracker.Web/FinanceTracker.Web.csproj reference FinanceTracker.Data/FinanceTracker.Data.csproj
dotnet add FinanceTracker.Console/FinanceTracker.Console.csproj reference FinanceTracker.Core/FinanceTracker.Core.csproj
dotnet add FinanceTracker.Console/FinanceTracker.Console.csproj reference FinanceTracker.Data/FinanceTracker.Data.csproj
dotnet add FinanceTracker.Tests/FinanceTracker.Tests.csproj reference FinanceTracker.Core/FinanceTracker.Core.csproj
dotnet add FinanceTracker.Tests/FinanceTracker.Tests.csproj reference FinanceTracker.Data/FinanceTracker.Data.csproj
```

4. Install NuGet packages:

```bash
# Data layer
dotnet add FinanceTracker.Data package Dapper
dotnet add FinanceTracker.Data package Microsoft.Data.Sqlite

# Web
dotnet add FinanceTracker.Web package MudBlazor
dotnet add FinanceTracker.Web package Serilog.AspNetCore
dotnet add FinanceTracker.Web package Serilog.Sinks.File

# Console
dotnet add FinanceTracker.Console package Spectre.Console
dotnet add FinanceTracker.Console package Serilog.Extensions.Hosting
dotnet add FinanceTracker.Console package Serilog.Sinks.Console
dotnet add FinanceTracker.Console package Serilog.Sinks.File
dotnet add FinanceTracker.Console package Microsoft.Extensions.Hosting
dotnet add FinanceTracker.Console package Microsoft.Extensions.Configuration.Json

# Tests
dotnet add FinanceTracker.Tests package Microsoft.Data.Sqlite
dotnet add FinanceTracker.Tests package Dapper
```

5. Delete the template boilerplate files (Class1.cs, WeatherForecast.cs, etc.) from each project.

6. Run `dotnet build` — it must succeed before continuing.

### Checkpoint
- `dotnet build` → zero errors
- Five projects in solution
- No boilerplate files remaining

---

## Phase 2 — Core Project (Models and Interfaces)

**Goal:** All domain models and interfaces defined in `FinanceTracker.Core`. No implementation yet.

### Steps

Create these files in `FinanceTracker.Core/`:

```
Models/
    Account.cs          — Account class + AccountType enum
    BalanceEntry.cs
    Category.cs
    Expense.cs
    Subscription.cs     — Subscription class + BillingFrequency enum
    Document.cs         — Document class + DocumentType enum
Interfaces/
    IAccountRepository.cs
    IBalanceEntryRepository.cs
    IExpenseRepository.cs
    ISubscriptionRepository.cs
    IDocumentRepository.cs
    ICategoryRepository.cs
    IAccountService.cs
    IBalanceService.cs
    IExpenseService.cs
    ISubscriptionService.cs
    IDocumentService.cs
    ICategoryService.cs
```

Copy the exact class shapes from `spec.md` — field names, types, and nullability must match exactly. The repository and service interfaces are also in `spec.md`.

### Checkpoint
- `dotnet build` → zero errors
- No implementation classes yet (all interfaces, no concrete classes)

---

## Phase 3 — Data Project (Database and Repositories)

**Goal:** SQLite database initialization and all Dapper repository implementations.

### Steps

#### 3a. DatabaseInitializer

Create `FinanceTracker.Data/DatabaseInitializer.cs`.

- Constructor takes `IConfiguration` to read `Database:Path`.
- Resolve the full path: `Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData), "FinanceTracker", configuredPath)`.
- Create the directory if it does not exist.
- Open a `SqliteConnection` and run all `CREATE TABLE IF NOT EXISTS` statements from `spec.md` in a single transaction.
- Expose a `public void Initialize()` method called at startup.

#### 3b. Category Seeder

Create `FinanceTracker.Data/CategorySeeder.cs`.

- Takes `ICategoryRepository`.
- `SeedAsync()` checks each default category name via `ExistsAsync`; inserts it if missing with `IsDefault = true`.
- Default names: `Food`, `Groceries`, `Gas`, `Bills`, `Rent/Mortgage`, `Entertainment`, `Subscriptions`, `Travel`, `Shopping`, `Other`.

#### 3c. Connection Factory

Create `FinanceTracker.Data/SqliteConnectionFactory.cs`:

```csharp
public class SqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string dbPath)
        => _connectionString = $"Data Source={dbPath}";

    public IDbConnection Create() => new SqliteConnection(_connectionString);
}
```

Register as singleton in DI.

#### 3d. Repository implementations

Create one file per repository in `FinanceTracker.Data/Repositories/`:

```
AccountRepository.cs
BalanceEntryRepository.cs
ExpenseRepository.cs
SubscriptionRepository.cs
DocumentRepository.cs
CategoryRepository.cs
```

Each repository:
- Takes `SqliteConnectionFactory` via constructor injection.
- Opens a new connection per method call (using `factory.Create()`).
- Uses Dapper `QueryAsync`, `QuerySingleOrDefaultAsync`, `ExecuteAsync`, and `ExecuteScalarAsync<int>` for `INSERT … RETURNING Id` (SQLite supports `last_insert_rowid()`).
- Maps enum columns as integers (Dapper maps them automatically when the C# property is an enum type).
- Maps `DateTime` to/from ISO 8601 strings using Dapper's type handler or inline conversion.

**DateTime handling:** Register a Dapper `TypeHandler<DateTime>` that parses ISO 8601 strings to `DateTime` and serializes back. Register it in `DatabaseInitializer` before any queries run:

```csharp
SqlMapper.AddTypeHandler(new DateTimeHandler());
```

#### 3e. DI extension

Create `FinanceTracker.Data/DataServiceExtensions.cs` with an `AddDataServices(this IServiceCollection services, IConfiguration config)` extension method that registers `SqliteConnectionFactory`, `DatabaseInitializer`, `CategorySeeder`, and all six repository implementations.

### Checkpoint
- `dotnet build` → zero errors
- Write an ad-hoc test (or a real xUnit test) that creates a temp SQLite DB, runs `DatabaseInitializer.Initialize()`, and confirms all tables exist via `SELECT name FROM sqlite_master WHERE type='table'`.

---

## Phase 4 — Service Layer

**Goal:** All service implementations in `FinanceTracker.Core/Services/`.

### Steps

Create one file per service:

```
FinanceTracker.Core/Services/AccountService.cs
FinanceTracker.Core/Services/BalanceService.cs
FinanceTracker.Core/Services/ExpenseService.cs
FinanceTracker.Core/Services/SubscriptionService.cs
FinanceTracker.Core/Services/DocumentService.cs
FinanceTracker.Core/Services/CategoryService.cs
```

Each service takes only interfaces (repositories, other services) in the constructor. No `SqliteConnection` or Dapper in services.

Key logic to implement:

- **AccountService.GetTotalBalanceAsync** — sums `Balance` on all active accounts.
- **BalanceService.GetCurrentNetWorthAsync** — sum of the most-recent `BalanceEntry.Amount` per active account. Use a LINQ `GroupBy` on entries grouped by `AccountId`, then take `MaxBy(e => e.EntryDate)` for each group.
- **ExpenseService.GetMonthlyTotalAsync** — filters by year+month, sums amounts.
- **ExpenseService.GetSpendingByCategoryAsync** — groups expenses by category name, sums each group.
- **SubscriptionService.GetMonthlyEstimateAsync** — apply the normalization multipliers from `spec.md`.
- **SubscriptionService.GetUpcomingAsync** — filter active subscriptions where `NextBillingDate <= today.AddDays(daysAhead)`.
- **DocumentService.GetExpiredAsync** — `ExpirationDate < today` and `IsActive`.
- **DocumentService.GetExpiringSoonAsync** — `ExpirationDate >= today && ExpirationDate <= today.AddDays(daysAhead)` and `IsActive`.
- **DocumentService.GetDueForReminderAsync** — `ReminderDate.HasValue && ReminderDate.Value.Date == today` and `IsActive`.

Add a `AddCoreServices(this IServiceCollection services)` extension in `FinanceTracker.Core/CoreServiceExtensions.cs` that registers all six service implementations.

### Checkpoint
- `dotnet build` → zero errors

---

## Phase 5 — Tests

**Goal:** All required test classes passing against real SQLite.

### Steps

#### 5a. Test database helper

Create `FinanceTracker.Tests/Helpers/TestDatabase.cs`:
- Creates a temp SQLite file path in `Path.GetTempPath()`.
- Runs `DatabaseInitializer.Initialize()`.
- Exposes a `SqliteConnectionFactory` for creating repositories.
- Implements `IDisposable` and deletes the temp file on dispose.

#### 5b. Service tests

Create one test class per service in `FinanceTracker.Tests/`:

```
AccountServiceTests.cs
ExpenseServiceTests.cs
SubscriptionServiceTests.cs
DocumentServiceTests.cs
```

Write tests for every item in the test coverage table in `spec.md`. Each test:
1. Creates a `TestDatabase`.
2. Seeds categories via `CategorySeeder`.
3. Creates repository instances and the service under test.
4. Runs the operation.
5. Asserts the expected result.
6. Disposes the database.

#### 5c. Repository tests

```
AccountRepositoryTests.cs
ExpenseRepositoryTests.cs
BalanceEntryRepositoryTests.cs
```

### Checkpoint
- `dotnet test` → all tests pass

---

## Phase 6 — Blazor Web App

**Goal:** A working browser app with all pages, dialogs, and the dashboard.

### Steps

#### 6a. Startup wiring

In `Program.cs`:
1. Call `builder.Services.AddDataServices(builder.Configuration)`.
2. Call `builder.Services.AddCoreServices()`.
3. Call `builder.Services.AddMudServices()`.
4. Call `UseSerilog()` (see Phase 7).
5. After `app.Build()`, resolve `DatabaseInitializer` and call `Initialize()`.
6. Resolve `CategorySeeder` and call `SeedAsync().Wait()`.

#### 6b. appsettings.json

Add the `Database` and `Serilog` sections from `spec.md`.

#### 6c. App.razor

Add inside `<head>`:
```html
<link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
```

Add before `</body>`:
```html
<script src="_content/MudBlazor/MudBlazor.min.js"></script>
```

#### 6d. MainLayout.razor

Replace the default layout with `MudLayout`:
- `MudAppBar` with app title and theme toggle.
- `MudDrawer` with `MudNavMenu` containing links to all six routes.
- `MudMainContent` wrapping `@Body`.
- Add `<MudThemeProvider>`, `<MudDialogProvider>`, `<MudSnackbarProvider>` here.

#### 6e. Pages — build in this order

Each page injects the relevant service via `@inject`, loads data in `OnInitializedAsync`, and uses MudBlazor components for display.

1. **Dashboard** (`Pages/Dashboard.razor`, route `/`)
   - Seven tiles as `MudPaper` cards in a `MudGrid`.
   - Call all services in parallel with `Task.WhenAll`.

2. **Accounts** (`Pages/Accounts/Index.razor`, route `/accounts`)
   - `MudTable` of accounts with columns: Name, Type, Institution, Balance, Status.
   - "Add Account" button opens `AccountDialog`.
   - Each row has Edit and Deactivate icon buttons.
   - `AccountDialog.razor`: `MudDialog` containing `MudTextField`, `MudSelect<AccountType>`, `MudNumericField`, `MudTextField` for notes.

3. **Balance History** (`Pages/Balance/Index.razor`, route `/balance`)
   - `MudSelect` to pick an account.
   - `MudDataGrid` of balance entries.
   - `MudChart` (line) showing balance over time.
   - "Add Snapshot" button opens a dialog with date picker and amount.

4. **Expenses** (`Pages/Expenses/Index.razor`, route `/expenses`)
   - Date range pickers (default: current month).
   - `MudDataGrid` of expenses.
   - Donut `MudChart` of spending by category.
   - `ExpenseDialog.razor`: category select, description, amount, date, account, notes.

5. **Subscriptions** (`Pages/Subscriptions/Index.razor`, route `/subscriptions`)
   - Monthly estimate chip at top.
   - `MudTable` of active subscriptions with next billing date column.
   - `SubscriptionDialog.razor`: name, cost, frequency select, next billing date picker, category, account, notes.

6. **Documents** (`Pages/Documents/Index.razor`, route `/documents`)
   - `MudTable` with a status `MudChip` column (Expired = red, Expiring Soon = orange, OK = green).
   - `DocumentDialog.razor`: name, type select, expiration date, reminder date, notes.

For all dialogs, on save call the service, show `ISnackbar` success message, and call `StateHasChanged` to refresh the table.

### Checkpoint
- `dotnet run --project FinanceTracker.Web` starts.
- Dashboard loads in browser.
- Can add an account and see it in the accounts table.
- Can add an expense and see it on the dashboard "Recent Expenses" tile.

---

## Phase 7 — Console App

**Goal:** A working CLI with the full menu tree.

### Steps

#### 7a. Startup

`Program.cs` using `Microsoft.Extensions.Hosting.Host`:
1. Build `IHostBuilder` with `ConfigureAppConfiguration` (load `appsettings.json`).
2. `ConfigureServices`: register `AddDataServices`, `AddCoreServices`.
3. `UseSerilog()` (see Phase 8).
4. In `Main`, build and start the host.
5. Initialize DB and seed categories.
6. Resolve `MainMenu` and call `Run()`.

#### 7b. MainMenu

Create `FinanceTracker.Console/Menus/MainMenu.cs`.

Use `AnsiConsole.Prompt(new SelectionPrompt<string>())` for top-level navigation. Delegate to sub-menus (AccountMenu, ExpenseMenu, SubscriptionMenu, DocumentMenu).

#### 7c. Sub-menus

Create one class per sub-menu under `FinanceTracker.Console/Menus/`:

- `AccountMenu` — ViewAccounts prints a `Table` with columns (Id, Name, Type, Balance, Institution). AddAccount prompts for each field.
- `ExpenseMenu` — ViewRecent prints last 10 expenses. AddExpense prompts for description, amount, date, category (via selection prompt), account (optional), notes.
- `SubscriptionMenu` — ViewActive shows table with monthly estimate at top. AddSubscription prompts for all fields.
- `DocumentMenu` — ViewExpiring shows documents expiring in next 30 days. AddDocument prompts for all fields.

Dashboard summary (`DashboardCommand`): print a single `Panel` containing:
- Net worth
- Monthly spending (current month)
- Monthly subscription estimate
- Count of upcoming subscriptions (next 7 days)
- Count of expiring documents (next 30 days)

Use `AnsiConsole.Markup` with color for good/warning/danger values.

### Checkpoint
- `dotnet run --project FinanceTracker.Console` shows the main menu.
- Can navigate to Accounts → Add Account and then Accounts → View Accounts shows the new account.

---

## Phase 8 — Logging

**Goal:** Serilog wired in both apps, writing to console and rolling file.

### Steps

#### Web (`FinanceTracker.Web/Program.cs`)

```csharp
builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration));
```

Add the `Serilog` section to `appsettings.json` as shown in `spec.md`.

#### Console (`FinanceTracker.Console/Program.cs`)

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
```

Wrap `Main` in a `try/catch` that logs `Fatal` on unhandled exception.

#### Log call sites

Add `ILogger<T>` injection to each service class. Log the events listed in `spec.md` at the specified levels.

### Checkpoint
- Start the web app; confirm a `logs/financetracker-YYYY-MM-DD.log` file is created.
- Start the console app; confirm logging output appears.

---

## Phase 9 — Final Verification

Run the full acceptance checklist from `spec.md`:

```bash
dotnet format
dotnet build          # must be zero errors, zero warnings
dotnet test           # all tests pass
dotnet run --project FinanceTracker.Web     # dashboard loads
dotnet run --project FinanceTracker.Console # main menu appears
```

Manually verify:
1. Add an account via the web app.
2. Add an expense and confirm it appears on the dashboard.
3. Add a subscription and confirm the monthly estimate updates.
4. Add a document with an expiration date 10 days from today; confirm it appears on the dashboard expiring-soon tile.
5. Restart the web app; confirm all data persists.
6. Run `git status` — confirm no `.db`, `.log`, or secrets files are staged.

---

## File Creation Checklist

Use this as a running checklist when building. Check off each file as it is created and compiles.

### FinanceTracker.Core

- [ ] `Models/Account.cs`
- [ ] `Models/BalanceEntry.cs`
- [ ] `Models/Category.cs`
- [ ] `Models/Expense.cs`
- [ ] `Models/Subscription.cs`
- [ ] `Models/Document.cs`
- [ ] `Interfaces/IAccountRepository.cs`
- [ ] `Interfaces/IBalanceEntryRepository.cs`
- [ ] `Interfaces/IExpenseRepository.cs`
- [ ] `Interfaces/ISubscriptionRepository.cs`
- [ ] `Interfaces/IDocumentRepository.cs`
- [ ] `Interfaces/ICategoryRepository.cs`
- [ ] `Interfaces/IAccountService.cs`
- [ ] `Interfaces/IBalanceService.cs`
- [ ] `Interfaces/IExpenseService.cs`
- [ ] `Interfaces/ISubscriptionService.cs`
- [ ] `Interfaces/IDocumentService.cs`
- [ ] `Interfaces/ICategoryService.cs`
- [ ] `Services/AccountService.cs`
- [ ] `Services/BalanceService.cs`
- [ ] `Services/ExpenseService.cs`
- [ ] `Services/SubscriptionService.cs`
- [ ] `Services/DocumentService.cs`
- [ ] `Services/CategoryService.cs`
- [ ] `CoreServiceExtensions.cs`

### FinanceTracker.Data

- [ ] `DatabaseInitializer.cs`
- [ ] `CategorySeeder.cs`
- [ ] `SqliteConnectionFactory.cs`
- [ ] `DateTimeHandler.cs`
- [ ] `Repositories/AccountRepository.cs`
- [ ] `Repositories/BalanceEntryRepository.cs`
- [ ] `Repositories/ExpenseRepository.cs`
- [ ] `Repositories/SubscriptionRepository.cs`
- [ ] `Repositories/DocumentRepository.cs`
- [ ] `Repositories/CategoryRepository.cs`
- [ ] `DataServiceExtensions.cs`

### FinanceTracker.Web

- [ ] `Program.cs`
- [ ] `appsettings.json`
- [ ] `App.razor`
- [ ] `Components/Layout/MainLayout.razor`
- [ ] `Components/Pages/Dashboard.razor`
- [ ] `Components/Pages/Accounts/Index.razor`
- [ ] `Components/Pages/Accounts/AccountDialog.razor`
- [ ] `Components/Pages/Balance/Index.razor`
- [ ] `Components/Pages/Expenses/Index.razor`
- [ ] `Components/Pages/Expenses/ExpenseDialog.razor`
- [ ] `Components/Pages/Subscriptions/Index.razor`
- [ ] `Components/Pages/Subscriptions/SubscriptionDialog.razor`
- [ ] `Components/Pages/Documents/Index.razor`
- [ ] `Components/Pages/Documents/DocumentDialog.razor`

### FinanceTracker.Console

- [ ] `Program.cs`
- [ ] `appsettings.json`
- [ ] `Menus/MainMenu.cs`
- [ ] `Menus/AccountMenu.cs`
- [ ] `Menus/ExpenseMenu.cs`
- [ ] `Menus/SubscriptionMenu.cs`
- [ ] `Menus/DocumentMenu.cs`
- [ ] `Menus/DashboardCommand.cs`

### FinanceTracker.Tests

- [ ] `Helpers/TestDatabase.cs`
- [ ] `AccountServiceTests.cs`
- [ ] `ExpenseServiceTests.cs`
- [ ] `SubscriptionServiceTests.cs`
- [ ] `DocumentServiceTests.cs`
- [ ] `AccountRepositoryTests.cs`
- [ ] `ExpenseRepositoryTests.cs`
- [ ] `BalanceEntryRepositoryTests.cs`
