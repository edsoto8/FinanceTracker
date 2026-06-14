# Repository Guidelines

## Project Structure & Module Organization

This repository contains a .NET Finance Tracker solution based on the product outline in `spec.md`. The solution is organized around these projects:

```text
FinanceTracker.Web/      Blazor Web App UI and dashboard
FinanceTracker.Console/  Spectre.Console command-line workflows
FinanceTracker.Core/     Domain models, services, and interfaces
FinanceTracker.Data/     SQLite schema, Dapper repositories, migrations
FinanceTracker.Tests/    xUnit tests for core logic and data access
```

Keep business rules in `FinanceTracker.Core`, persistence details in `FinanceTracker.Data`, and UI-specific behavior in the Web or Console projects. Store static web assets under `FinanceTracker.Web/wwwroot`.

## Build, Test, and Development Commands



Use standard .NET CLI commands from the repository root:

- `dotnet restore` installs NuGet dependencies.
- `dotnet build` compiles the full solution.
- `dotnet test` runs the xUnit test suite.
- `dotnet run --project FinanceTracker.Web` starts the Blazor app locally.
- `dotnet run --project FinanceTracker.Console` runs the CLI experience.

Run commands from the repository root so project references resolve consistently.

## Coding Style & Naming Conventions

Use C# conventions: four-space indentation, PascalCase for public types and members, camelCase for local variables and parameters, and `Async` suffixes for asynchronous methods. Name projects and namespaces with the `FinanceTracker.*` prefix. Prefer small domain-focused classes, interfaces for infrastructure boundaries, and dependency injection between projects. Run `dotnet format` before submitting changes if formatting tools are available.

## Testing Guidelines

Use xUnit in `FinanceTracker.Tests`. Name test classes after the target type, such as `AccountServiceTests`, and name test methods with behavior-focused patterns like `AddAccount_CreatesActiveAccount`. Prioritize tests for domain services, validation rules, Dapper repositories, and date-sensitive logic such as subscription billing and document reminders. Use temporary SQLite databases for data-access tests rather than committing local database files.

## Commit & Pull Request Guidelines

This repository has no existing commit history yet, so use concise imperative commit messages such as `Add account domain model` or `Implement subscription repository`. Pull requests should include a short summary, test results (`dotnet test` output when applicable), linked issues or spec sections, and screenshots for Blazor UI changes.

## Security & Configuration Tips

Do not commit personal finance data, SQLite database files, logs, or secrets. Keep local configuration in ignored development settings files, and document required environment variables or connection strings in the relevant project README when they are introduced.
