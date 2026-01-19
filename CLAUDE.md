# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

NemesisEuchre is a console-based engine for training a high-performance Euchre AI using massive dataset seeding and genetic algorithms. The project is in early development with infrastructure in place but core game logic not yet implemented.

## Build & Development Commands

### Core Commands
```bash
# Restore dependencies
dotnet restore

# Build (Debug)
dotnet build

# Build (Release)
dotnet build --configuration Release

# Run the application
dotnet run --project NemesisEuchre.Console

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Format code
dotnet format

# Verify formatting without changes
dotnet format --verify-no-changes
```

### Working with Specific Projects
```bash
# Run Console application with arguments
dotnet run --project NemesisEuchre.Console -- <args>

# Test a specific project
dotnet test NemesisEuchre.Console.Tests

# Build without restoring
dotnet build --no-restore
```

## Architecture

### Application Structure

The application uses the **Generic Host pattern** from Microsoft.Extensions.Hosting:

- **Program.cs**: Entry point that configures the host with:
  - Configuration sources (appsettings.json, environment variables, command line)
  - Service registration (dependency injection)
  - Hosted services

- **EuchreGameServiceHost**: Main application logic as an `IHostedService`
  - Runs on background thread via `Task.Run()`
  - Handles graceful shutdown through `IHostApplicationLifetime`
  - Displays Spectre.Console UI

- **LoggerMessages**: Centralized logging using LoggerMessage source generators
  - Compile-time generated, high-performance logging methods
  - All application logs should be added here

### Configuration System

Configuration is loaded from multiple sources (in priority order):
1. appsettings.json (required file)
2. Environment variables (overrides JSON)
3. Command-line arguments (overrides all)

Add new configuration sections to `appsettings.json`. Access via `IConfiguration` through dependency injection.

### Dependency Injection

All services should be registered in `Program.cs` in the `ConfigureServices` lambda. Use constructor injection for dependencies. The project uses primary constructors for concise dependency declaration.

### Logging

**Always use LoggerMessage source generators** for logging:
1. Add a partial method to `LoggerMessages.cs` with the `[LoggerMessage]` attribute
2. Call the generated method, passing the `ILogger<T>` instance

This provides compile-time generated, zero-allocation logging.

## Package Management

This project uses **Central Package Management (CPM)**:

- **All package versions** are defined in `Directory.Packages.props`
- Project files (`*.csproj`) reference packages **without version numbers**
- To add a package:
  1. Add `<PackageVersion Include="PackageName" Version="x.y.z" />` to `Directory.Packages.props`
  2. Add `<PackageReference Include="PackageName" />` to the project file

### Analyzer Packages

Seven analyzers are configured as `GlobalPackageReference` and apply to all projects automatically:
- StyleCop.Analyzers
- Roslynator.Analyzers
- SonarAnalyzer.CSharp
- Meziantou.Analyzer
- SecurityCodeScan.VS2019
- AsyncFixer
- Microsoft.VisualStudio.Threading.Analyzers

Do not reference these in project files.

## Code Quality & Standards

### Pre-commit Hooks (Husky.Net)

Husky.Net runs automatically on every commit:
1. **Format check**: Verifies code formatting (`dotnet format --verify-no-changes`)
2. **Build and Test**: Ensures code compiles and tests pass

To bypass hooks (use sparingly): `git commit --no-verify`

### EditorConfig

Strict code style enforcement via `.editorconfig`:
- 4-space indentation for C#
- File-scoped namespaces preferred
- Expression-bodied members preferred where appropriate
- Accessibility modifiers required
- `var` usage: Use when type is obvious from right side
- Null checking: Prefer null-coalescing and null-conditional operators

Key disabled warnings:
- `IDE0210`: Top-level statements (not used in this project)
- `MA0038`: Make method static (deprecated analyzer rule)

### Testing

- **Framework**: xUnit
- **Assertions**: FluentAssertions (preferred over xUnit assertions)
- **Mocking**: Moq
- **Test data generation**: Bogus
- **Coverage**: Coverlet (collected automatically in CI)

Tests should be in `NemesisEuchre.Console.Tests` with the pattern `*Tests.cs`.

## CI/CD

GitHub Actions workflow (`.github/workflows/ci.yml`) runs on:
- Push to `main` or `feature/*` branches
- Pull requests to `main`

Pipeline steps:
1. Restore dependencies
2. Build in Release configuration
3. Run tests with code coverage
4. Upload coverage to Codecov

## Versioning

Uses **Nerdbank.GitVersioning** configured in `version.json`:
- Base version: `0.1-preview`
- Versions calculated from git history
- Public releases: `main` branch or tags matching `v\d+\.\d+`

Access version info via generated assembly attributes.

## UI Framework

Uses **Spectre.Console** for terminal UI:
- Colored markup: `AnsiConsole.MarkupLine("[green]text[/]")`
- Tables, progress bars, ASCII art (FigletText) available
- Refer to Spectre.Console documentation for advanced features

## Project Structure Conventions

- **Namespace**: All code in `NemesisEuchre.Console` namespace
- **Class naming**: Descriptive names ending with type (e.g., `EuchreGameServiceHost` for hosted service)
- **Public APIs**: Classes intended for DI or external use should be `public`
- **File organization**: One class per file, filename matches class name

## Future Development Notes

This project is designed for AI/ML workloads:
- Logging infrastructure is in place for training run observability
- Configuration system ready for hyperparameters
- Dependency injection prepared for game engine services
- Infrastructure supports long-running background processes

When implementing game logic:
- Register game services in `Program.cs`
- Use `IHostedService` for long-running training processes
- Leverage Spectre.Console for training progress visualization
- Add structured logging to `LoggerMessages.cs` for fitness tracking
