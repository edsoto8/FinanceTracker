# Finance Tracker — Specification

## Goal

Build a personal finance tracking application that helps users manage accounts, balances, spending, subscriptions, and important document expiration dates.

---

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 10 |
| Web UI | Blazor Web App (Interactive Server) |
| UI Components | MudBlazor |
| CLI | Spectre.Console |
| Database | SQLite (local file) |
| Data access | Dapper (no EF) |
| Logging | Serilog (console + rolling file) |
| Tests | xUnit |

---

## Solution Structure

```text
FinanceTracker/
├── FinanceTracker.sln
├── FinanceTracker.Core/        Domain models, interfaces, services (no infra deps)
├── FinanceTracker.Data/        SQLite + Dapper implementations
├── FinanceTracker.Web/         Blazor Web App
├── FinanceTracker.Console/     Spectre.Console CLI
└── FinanceTracker.Tests/       xUnit tests
```

**Dependency rule:** `Core` has zero dependencies on `Data`, `Web`, or `Console`. `Data` references `Core`. Web and Console reference both.

---

## Domain Models

All models live in `FinanceTracker.Core/Models/`.

### Account

```csharp
public class Account
{
    public int Id { get; set; }
    public string Name { get; set; }          // required, max 100
    public AccountType Type { get; set; }
    public decimal Balance { get; set; }       // current balance snapshot
    public string Institution { get; set; }   // optional, max 100
    public string Notes { get; set; }         // optional, max 500
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum AccountType
{
    Checking,
    Savings,
    CreditCard,
    Loan,
    Investment,
    Other
}
```

### BalanceEntry

```csharp
public class BalanceEntry
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime EntryDate { get; set; }
    public string Notes { get; set; }         // optional, max 500
    public DateTime CreatedAt { get; set; }
}
```

### Category

```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }          // required, max 50, unique
    public bool IsDefault { get; set; }
}
```

Default seed categories: Food, Groceries, Gas, Bills, Rent/Mortgage, Entertainment, Subscriptions, Travel, Shopping, Other.

### Expense

```csharp
public class Expense
{
    public int Id { get; set; }
    public int AccountId { get; set; }        // optional (nullable)
    public int CategoryId { get; set; }
    public string Description { get; set; }   // required, max 200
    public decimal Amount { get; set; }        // positive value
    public DateTime TransactionDate { get; set; }
    public string PaymentMethod { get; set; } // optional, max 50
    public string Notes { get; set; }         // optional, max 500
    public DateTime CreatedAt { get; set; }
}
```

### Subscription

```csharp
public class Subscription
{
    public int Id { get; set; }
    public string Name { get; set; }              // required, max 100
    public decimal Cost { get; set; }              // positive value
    public BillingFrequency Frequency { get; set; }
    public DateTime NextBillingDate { get; set; }
    public int CategoryId { get; set; }
    public int? AccountId { get; set; }            // optional payment account
    public bool IsActive { get; set; }
    public string Notes { get; set; }             // optional, max 500
}

public enum BillingFrequency
{
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}
```

### Document

```csharp
public class Document
{
    public int Id { get; set; }
    public string Name { get; set; }             // required, max 100
    public DocumentType Type { get; set; }
    public DateTime ExpirationDate { get; set; }
    public DateTime? ReminderDate { get; set; }  // optional
    public string Notes { get; set; }            // optional, max 500
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum DocumentType
{
    Insurance,
    License,
    Registration,
    Warranty,
    Contract,
    Other
}
```

---

## Service Interfaces

All interfaces live in `FinanceTracker.Core/Interfaces/`. All methods are async.

### IAccountService

```csharp
public interface IAccountService
{
    Task<IEnumerable<Account>> GetAllAccountsAsync();
    Task<IEnumerable<Account>> GetActiveAccountsAsync();
    Task<Account?> GetAccountByIdAsync(int id);
    Task<Account> AddAccountAsync(Account account);
    Task UpdateAccountAsync(Account account);
    Task DeactivateAccountAsync(int id);
    Task DeleteAccountAsync(int id);
    Task<decimal> GetTotalBalanceAsync();           // sum of active account balances
}
```

### IBalanceService

```csharp
public interface IBalanceService
{
    Task<IEnumerable<BalanceEntry>> GetHistoryAsync(int accountId);
    Task<BalanceEntry> AddEntryAsync(BalanceEntry entry);
    Task<decimal> GetCurrentNetWorthAsync();        // sum of latest entry per active account
}
```

### IExpenseService

```csharp
public interface IExpenseService
{
    Task<IEnumerable<Expense>> GetExpensesAsync(DateTime from, DateTime to);
    Task<IEnumerable<Expense>> GetRecentAsync(int count);
    Task<Expense?> GetByIdAsync(int id);
    Task<Expense> AddExpenseAsync(Expense expense);
    Task UpdateExpenseAsync(Expense expense);
    Task DeleteExpenseAsync(int id);
    Task<decimal> GetMonthlyTotalAsync(int year, int month);
    Task<Dictionary<string, decimal>> GetSpendingByCategoryAsync(DateTime from, DateTime to);
}
```

### ISubscriptionService

```csharp
public interface ISubscriptionService
{
    Task<IEnumerable<Subscription>> GetAllAsync();
    Task<IEnumerable<Subscription>> GetActiveAsync();
    Task<Subscription?> GetByIdAsync(int id);
    Task<Subscription> AddAsync(Subscription subscription);
    Task UpdateAsync(Subscription subscription);
    Task DeactivateAsync(int id);
    Task<decimal> GetMonthlyEstimateAsync();        // normalizes all active subscriptions to monthly
    Task<IEnumerable<Subscription>> GetUpcomingAsync(int daysAhead);
}
```

Monthly estimate normalization:
- Weekly × 4.33
- Monthly × 1
- Quarterly ÷ 3
- Yearly ÷ 12

### IDocumentService

```csharp
public interface IDocumentService
{
    Task<IEnumerable<Document>> GetAllAsync();
    Task<IEnumerable<Document>> GetActiveAsync();
    Task<Document?> GetByIdAsync(int id);
    Task<Document> AddAsync(Document document);
    Task UpdateAsync(Document document);
    Task DeactivateAsync(int id);
    Task DeleteAsync(int id);
    Task<IEnumerable<Document>> GetExpiredAsync();
    Task<IEnumerable<Document>> GetExpiringSoonAsync(int daysAhead);
    Task<IEnumerable<Document>> GetDueForReminderAsync();  // ReminderDate <= today
}
```

### ICategoryService

```csharp
public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category> AddAsync(Category category);
    Task SeedDefaultsAsync();
}
```

---

## Repository Interfaces

All interfaces live in `FinanceTracker.Core/Interfaces/`. Services depend on these; `Data` implements them.

```csharp
public interface IAccountRepository
{
    Task<IEnumerable<Account>> GetAllAsync();
    Task<Account?> GetByIdAsync(int id);
    Task<int> InsertAsync(Account account);       // returns new Id
    Task UpdateAsync(Account account);
    Task DeleteAsync(int id);
}

public interface IBalanceEntryRepository
{
    Task<IEnumerable<BalanceEntry>> GetByAccountIdAsync(int accountId);
    Task<int> InsertAsync(BalanceEntry entry);
}

public interface IExpenseRepository
{
    Task<IEnumerable<Expense>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<Expense>> GetRecentAsync(int count);
    Task<Expense?> GetByIdAsync(int id);
    Task<int> InsertAsync(Expense expense);
    Task UpdateAsync(Expense expense);
    Task DeleteAsync(int id);
}

public interface ISubscriptionRepository
{
    Task<IEnumerable<Subscription>> GetAllAsync();
    Task<int> InsertAsync(Subscription subscription);
    Task UpdateAsync(Subscription subscription);
    Task DeleteAsync(int id);
}

public interface IDocumentRepository
{
    Task<IEnumerable<Document>> GetAllAsync();
    Task<Document?> GetByIdAsync(int id);
    Task<int> InsertAsync(Document document);
    Task UpdateAsync(Document document);
    Task DeleteAsync(int id);
}

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<int> InsertAsync(Category category);
    Task<bool> ExistsAsync(string name);
}
```

---

## Database Schema

SQLite database file: `financetracker.db` (in the app's data directory, never committed).

Database is created automatically on startup via `DatabaseInitializer` in `FinanceTracker.Data`.

```sql
CREATE TABLE IF NOT EXISTS Categories (
    Id      INTEGER PRIMARY KEY AUTOINCREMENT,
    Name    TEXT NOT NULL UNIQUE,
    IsDefault INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS Accounts (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        TEXT NOT NULL,
    Type        INTEGER NOT NULL,
    Balance     REAL NOT NULL DEFAULT 0,
    Institution TEXT,
    Notes       TEXT,
    IsActive    INTEGER NOT NULL DEFAULT 1,
    CreatedAt   TEXT NOT NULL,
    UpdatedAt   TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS BalanceEntries (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId   INTEGER NOT NULL REFERENCES Accounts(Id),
    Amount      REAL NOT NULL,
    EntryDate   TEXT NOT NULL,
    Notes       TEXT,
    CreatedAt   TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Expenses (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId       INTEGER REFERENCES Accounts(Id),
    CategoryId      INTEGER NOT NULL REFERENCES Categories(Id),
    Description     TEXT NOT NULL,
    Amount          REAL NOT NULL,
    TransactionDate TEXT NOT NULL,
    PaymentMethod   TEXT,
    Notes           TEXT,
    CreatedAt       TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Subscriptions (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Name            TEXT NOT NULL,
    Cost            REAL NOT NULL,
    Frequency       INTEGER NOT NULL,
    NextBillingDate TEXT NOT NULL,
    CategoryId      INTEGER NOT NULL REFERENCES Categories(Id),
    AccountId       INTEGER REFERENCES Accounts(Id),
    IsActive        INTEGER NOT NULL DEFAULT 1,
    Notes           TEXT
);

CREATE TABLE IF NOT EXISTS Documents (
    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
    Name           TEXT NOT NULL,
    Type           INTEGER NOT NULL,
    ExpirationDate TEXT NOT NULL,
    ReminderDate   TEXT,
    Notes          TEXT,
    IsActive       INTEGER NOT NULL DEFAULT 1,
    CreatedAt      TEXT NOT NULL,
    UpdatedAt      TEXT NOT NULL
);
```

All dates stored as ISO 8601 strings (`yyyy-MM-ddTHH:mm:ss`).

Enums stored as integers matching their C# ordinal values.

---

## Validation Rules

### Account
- `Name`: required, 1–100 characters
- `Balance`: any decimal (can be negative for credit/loan)
- `Institution`: optional, max 100 characters
- `Notes`: optional, max 500 characters

### Expense
- `Description`: required, 1–200 characters
- `Amount`: must be > 0
- `TransactionDate`: must not be in the future (enforce in UI, not DB)
- `CategoryId`: must reference an existing category

### Subscription
- `Name`: required, 1–100 characters
- `Cost`: must be > 0
- `NextBillingDate`: must be a valid future or today's date

### Document
- `Name`: required, 1–100 characters
- `ExpirationDate`: required
- `ReminderDate`: if provided, must be before `ExpirationDate`

---

## Configuration

### appsettings.json (Web and Console)

```json
{
  "Database": {
    "Path": "financetracker.db"
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/financetracker-.log",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

`DatabaseInitializer` reads `Database:Path` and resolves it relative to the app's data directory (`Environment.GetFolderPath(SpecialFolder.LocalApplicationData)/FinanceTracker/`).

---

## Blazor Web App

### Project type

Blazor Web App — Interactive Server rendering mode. No WebAssembly.

### Startup (`Program.cs`)

```
builder.Services.AddMudServices()
Register IDbConnection factory (SQLite)
Register all repositories (Data implementations)
Register all services (Core implementations)
UseSerilog()
```

### Layout

`MainLayout.razor` contains:
- `<MudLayout>` with `<MudAppBar>` and `<MudNavMenu>`
- `<MudThemeProvider>`, `<MudDialogProvider>`, `<MudSnackbarProvider>` in `App.razor`

Navigation links:
- Dashboard (`/`)
- Accounts (`/accounts`)
- Balance History (`/balance`)
- Expenses (`/expenses`)
- Subscriptions (`/subscriptions`)
- Documents (`/documents`)

### Pages and routes

| File | Route | Purpose |
|------|-------|---------|
| `Pages/Dashboard.razor` | `/` | Summary tiles, recent expenses, upcoming subscriptions, expiring docs |
| `Pages/Accounts/Index.razor` | `/accounts` | MudTable of all accounts, total balance chip |
| `Pages/Accounts/AccountDialog.razor` | (dialog) | Add/Edit account form |
| `Pages/Balance/Index.razor` | `/balance` | Account selector + MudDataGrid of history + MudChart line chart |
| `Pages/Expenses/Index.razor` | `/expenses` | Date range filter, MudDataGrid, category pie chart |
| `Pages/Expenses/ExpenseDialog.razor` | (dialog) | Add/Edit expense form |
| `Pages/Subscriptions/Index.razor` | `/subscriptions` | MudTable of active subscriptions, monthly total chip |
| `Pages/Subscriptions/SubscriptionDialog.razor` | (dialog) | Add/Edit subscription form |
| `Pages/Documents/Index.razor` | `/documents` | MudTable with status badges (expired, expiring soon, OK) |
| `Pages/Documents/DocumentDialog.razor` | (dialog) | Add/Edit document form |

### Dashboard tiles

1. **Net Worth** — sum of active account balances (MudPaper with large number)
2. **Monthly Spending** — current month total expenses
3. **Monthly Subscriptions** — estimated monthly subscription cost
4. **Upcoming Subscriptions** — list of subscriptions billing in next 7 days
5. **Expiring Documents** — documents expiring in next 30 days
6. **Recent Expenses** — last 5 expenses (MudList)
7. **Account Summary** — MudSimpleTable of active accounts with balances

### MudBlazor component usage

- Tables/grids: `MudDataGrid` or `MudTable`
- Charts: `MudChart` (line for balance history, pie/donut for spending by category)
- Forms: `MudDialog` with `MudTextField`, `MudSelect`, `MudDatePicker`, `MudNumericField`
- Notifications: `MudSnackbar` (success on save/delete, error on failure)
- Status chips: `MudChip` for account types and document status

---

## Console App

### Entry point

`Program.cs` bootstraps DI, Serilog, and launches a `MainMenu` command.

### Menu tree

```
Main Menu
├── Dashboard Summary       — prints summary table
├── Accounts
│   ├── View accounts
│   └── Add account
├── Expenses
│   ├── View recent expenses
│   └── Add expense
├── Subscriptions
│   ├── View active subscriptions
│   └── Add subscription
├── Documents
│   ├── View expiring documents
│   └── Add document
└── Exit
```

Each "Add" workflow uses `AnsiConsole.Ask<T>()` and `AnsiConsole.Prompt(SelectionPrompt)` to collect input, then calls the relevant service.

Output uses Spectre.Console `Table`, `Rule`, `Panel`, and color markup.

---

## Logging

Use Serilog in both Web and Console apps.

Log these events:

| Event | Level |
|-------|-------|
| Application starting | Information |
| Database created/initialized | Information |
| Seed data inserted | Information |
| Account/expense/subscription/document added | Information |
| Account/document deactivated | Information |
| Record deleted | Warning |
| Validation failure | Warning |
| Database query failure | Error |
| Unhandled exception | Fatal |

Sink configuration: console + rolling daily file at `logs/financetracker-.log`.

---

## Testing

Tests live in `FinanceTracker.Tests`. Use xUnit + in-memory or temp-file SQLite for repository tests. No mocking of the database.

### Required test coverage

| Class | Tests |
|-------|-------|
| `AccountService` | Add creates active account; total balance sums only active accounts |
| `ExpenseService` | Monthly total for correct month; spending by category groups correctly |
| `SubscriptionService` | Monthly estimate normalizes all frequencies; upcoming filter respects days window |
| `DocumentService` | Expired detection; expiring-soon with configurable days; reminder due today |
| `AccountRepository` | Insert returns new Id; GetById returns null for missing |
| `ExpenseRepository` | GetByDateRange filters correctly |
| `BalanceEntryRepository` | Insert and retrieve by account |

---

## Non-Goals (v1)

- Bank/brokerage API integrations
- Authentication or multi-user support
- Cloud sync or backup
- Mobile app
- Budgeting engine with limits/alerts
- Receipt photo uploads
- AI-assisted categorization
- Import/export (CSV, OFX)

---

## Build Acceptance Criteria

The finished app must satisfy all of the following:

1. `dotnet build` succeeds with zero errors and zero warnings.
2. `dotnet test` passes all tests.
3. `dotnet run --project FinanceTracker.Web` starts and the dashboard loads in a browser.
4. `dotnet run --project FinanceTracker.Console` shows the main menu and allows adding an account.
5. All data persists to `financetracker.db` across restarts.
6. Default categories are present on first run without manual setup.
7. No `.db`, `.log`, or secret files are committed to source control.
