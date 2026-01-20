# NemesisEuchre

A console-based engine for training a high-performance Euchre AI using massive dataset seeding and genetic algorithms.

## Project Status

⚠️ **Early Development** - Infrastructure in place, core game logic in progress.

### Current State
- ✅ Project infrastructure and CLI framework
- ✅ Dependency injection and logging setup
- ✅ Unit test infrastructure with FluentAssertions
- ✅ GitHub Actions CI/CD pipeline
- ✅ Code quality analyzers (StyleCop, Roslynator, SonarAnalyzer)
- 🚧 Game logic and domain models (in progress)
- 📋 AI training infrastructure (planned)
- 📋 Genetic algorithms implementation (planned)

## Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Build & Run

```bash
# Clone the repository
git clone https://github.com/bofirial/NemesisEuchre.git
cd NemesisEuchre

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project NemesisEuchre.Console

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```
