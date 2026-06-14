# Finance Tracker

A personal finance tracking application built with .NET 10, Blazor, Spectre.Console, SQLite, and Dapper.

## Projects

```text
FinanceTracker.Web/      Blazor Web App UI and dashboard
FinanceTracker.Console/  Spectre.Console command-line workflows
FinanceTracker.Core/     Domain models, services, and interfaces
FinanceTracker.Data/     SQLite schema, Dapper repositories, migrations
FinanceTracker.Tests/    xUnit tests for core logic and data access
```

## Features

- Account tracking with balance history
- Expense tracking by date range and category
- Subscription tracking with estimated monthly cost
- Document expiration and reminder tracking
- Blazor dashboard and CRUD screens
- Console workflows for common finance tasks
- SQLite database initialized on first run

## Getting Started

Restore dependencies:

```bash
dotnet restore
```

Build the solution:

```bash
dotnet build
```

Run tests:

```bash
dotnet test
```

Start the web app:

```bash
dotnet run --project FinanceTracker.Web
```

Run the console app:

```bash
dotnet run --project FinanceTracker.Console
```

## Data And Privacy

The app creates local SQLite databases on first run. Database files, logs, build outputs, and local tool settings are ignored by Git and should not be committed.
