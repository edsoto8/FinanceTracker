# Finance Tracker App Spec

## Goal

Build a personal finance tracking application that helps users manage accounts, balances, spending, subscriptions, and important document expiration dates.

## Technology Stack

* .NET 10
* Blazor Web App
* MudBlazor for UI components and theming
* Spectre.Console CLI
* Shared .NET class library
* SQLite database
* Dapper for data access
* Serilog for logging
* xUnit for automated tests

## Solution Structure

Create a .NET solution with these projects:

```text
FinanceTracker/
├── FinanceTracker.Web          # Blazor Web App
├── FinanceTracker.Console      # Spectre.Console CLI app
├── FinanceTracker.Core         # Domain models, services, interfaces
├── FinanceTracker.Data         # SQLite + Dapper repositories
└── FinanceTracker.Tests        # xUnit tests
```

## Core Features

### 1. Account Management

Users should be able to create and manage financial accounts.

Each account should include:

* Account name
* Account type

  * Checking
  * Savings
  * Credit Card
  * Loan
  * Investment
  * Other
* Current balance
* Institution name
* Notes
* Active/inactive status
* Created date
* Updated date

Users should be able to:

* Add accounts
* Edit accounts
* Delete or deactivate accounts
* View all accounts
* View total balance across accounts

---

### 2. Balance Tracker

Users should be able to track balance changes over time.

Each balance entry should include:

* Account ID
* Balance amount
* Balance date
* Notes
* Created date

Users should be able to:

* Add balance snapshots
* View balance history by account
* View current net worth
* See balance trends over time

---

### 3. Spending Tracker

Users should be able to track expenses.

Each expense should include:

* Account ID
* Category
* Description
* Amount
* Transaction date
* Payment method
* Notes
* Created date

Default spending categories:

* Food
* Groceries
* Gas
* Bills
* Rent/Mortgage
* Entertainment
* Subscriptions
* Travel
* Shopping
* Other

Users should be able to:

* Add expenses
* Edit expenses
* Delete expenses
* View expenses by date range
* View spending by category
* View monthly spending total

---

### 4. Subscription Tracker

Users should be able to manage recurring subscriptions.

Each subscription should include:

* Name
* Cost
* Billing frequency

  * Monthly
  * Yearly
  * Weekly
  * Quarterly
* Next billing date
* Category
* Payment account
* Active/inactive status
* Notes

Users should be able to:

* Add subscriptions
* Edit subscriptions
* Deactivate subscriptions
* View active subscriptions
* View upcoming billing dates
* View monthly estimated subscription cost

---

### 5. Document Expiration Tracker

Users should be able to track important financial or personal documents that expire.

Each document should include:

* Document name
* Document type

  * Insurance
  * License
  * Registration
  * Warranty
  * Contract
  * Other
* Expiration date
* Reminder date
* Notes
* Active/inactive status

Users should be able to:

* Add documents
* Edit documents
* Delete or deactivate documents
* View expired documents
* View documents expiring soon
* View upcoming reminders

---

## UI Framework

Use MudBlazor for all Blazor UI components and theming.

* Register `AddMudServices()` in `Program.cs`
* Add MudBlazor CSS/JS imports in `App.razor`
* Include `<MudThemeProvider>`, `<MudDialogProvider>`, and `<MudSnackbarProvider>` in the root layout
* Use `MudDataGrid` or `MudTable` for tabular data (accounts, expenses, subscriptions, documents)
* Use `MudChart` for balance trends and spending breakdowns
* Use `MudDialog` for add/edit forms
* Use `MudDatePicker` for date inputs
* Use `MudSnackbar` for success/error notifications
* Apply a consistent MudBlazor theme across all pages

---

## Dashboard

The Blazor dashboard should display:

* Total balance across active accounts
* Total monthly spending
* Estimated monthly subscriptions
* Upcoming subscription payments
* Documents expiring soon
* Recent expenses
* Account summary

---

## Console App

The Spectre.Console app should support basic workflows:

* View dashboard summary
* Add account
* Add expense
* Add subscription
* Add document expiration
* View accounts
* View upcoming subscriptions
* View expiring documents

Use Spectre.Console tables, prompts, menus, and formatted output.

---

## Database

Use SQLite as the local database.

Use Dapper for all database access.

Do not use Entity Framework.

Suggested tables:

* Accounts
* BalanceEntries
* Expenses
* Subscriptions
* Documents
* Categories

Database should be created automatically if it does not exist.

Include seed data for default categories.

---

## Logging

Use Serilog in both the Blazor app and Console app.

Log:

* Application startup
* Errors
* Database failures
* Important user actions
* Import/export failures if added later

Logs should write to:

* Console
* Local rolling log file

---

## Testing

Use xUnit for tests.

Add tests for:

* Account creation
* Balance calculations
* Monthly spending totals
* Subscription monthly cost calculations
* Expiring document detection
* Repository methods where practical
* Core service logic

---

## Non-Goals for Initial Version

Do not build these in version 1:

* Bank API integrations
* Authentication
* Cloud sync
* Mobile app
* Multi-user support
* Budgeting engine
* Receipt uploads
* AI categorization

---

## Build Requirements

The finished app should allow a user to:

1. Run the Blazor web app locally.
2. Run the console app locally.
3. Store all data in SQLite.
4. Manage accounts, expenses, subscriptions, and document expirations.
5. View a useful finance dashboard.
6. Run automated tests successfully.

## Recommended First Implementation Order

1. Create solution and projects.
2. Add core domain models.
3. Add SQLite database setup.
4. Add Dapper repositories.
5. Add account management.
6. Add expense tracking.
7. Add subscription tracking.
8. Add document expiration tracking.
9. Add MudBlazor and configure theme/layout.
10. Add dashboard.
11. Add console workflows.
12. Add Serilog.
13. Add xUnit tests.
