# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Personal finance tracking application built with .NET 10. The full specification is in `spec.md`, and the current implementation includes Blazor Web, Spectre.Console CLI, Core, Data, and Tests projects.

## Solution Structure

```
FinanceTracker/
├── FinanceTracker.Web/       Blazor Web App — dashboard and UI
├── FinanceTracker.Console/   Spectre.Console CLI workflows
├── FinanceTracker.Core/      Domain models, services, interfaces (no infrastructure deps)
├── FinanceTracker.Data/      SQLite schema, Dapper repositories (no EF)
└── FinanceTracker.Tests/     xUnit tests
```

Dependency rule: `Core` has no dependencies on `Data`, `Web`, or `Console`. `Data` depends on `Core`. UI projects depend on both.

## Commands

```bash
dotnet restore                                    # install NuGet deps
dotnet build                                      # compile full solution
dotnet test                                       # run all xUnit tests
dotnet test --filter "FullyQualifiedName~AccountService"  # run single test class
dotnet run --project FinanceTracker.Web           # start Blazor app
dotnet run --project FinanceTracker.Console       # run CLI
dotnet format                                     # format before committing
```

Run all commands from the repo root.

## Key Architectural Decisions

- **No Entity Framework** — use Dapper for all data access in `FinanceTracker.Data`
- **SQLite** auto-created on first run; seed default categories on startup
- **Serilog** in both Web and Console: logs to console + rolling file
- **Dependency injection** wires `Core` interfaces to `Data` implementations in each app's startup
- Use temporary in-memory or file-based SQLite DBs for data-access tests (never commit `.db` files)

## Coding Conventions

- C# with four-space indentation, PascalCase for public members, camelCase for locals
- `Async` suffix on all async methods
- Namespace prefix: `FinanceTracker.*`
- Test class naming: `AccountServiceTests`; test method pattern: `AddAccount_CreatesActiveAccount`

## Do Not Commit

SQLite database files, log files, personal finance data, or secrets.
