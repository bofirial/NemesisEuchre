# NemesisEuchre

A console-based engine for training a high-performance Euchre AI using massive dataset seeding and genetic algorithms.

## Project Status

🚀 **Version 0.3-preview** - Full game engine implemented, now recording game data for ML training.

### Current State
- ✅ **v0.1** - Project infrastructure and CLI framework
- ✅ **v0.1** - Dependency injection and logging setup
- ✅ **v0.1** - Unit test infrastructure with FluentAssertions, Moq, and Bogus
- ✅ **v0.1** - GitHub Actions CI/CD pipeline
- ✅ **v0.1** - Code quality analyzers (StyleCop, Roslynator, SonarAnalyzer)
- ✅ **v0.2** - Complete Euchre game engine with orchestration
- ✅ **v0.2** - Game models optimized for machine learning (relative suits and positions)
- ✅ **v0.2** - Trump selection, trick playing, and deal result calculation
- ✅ **v0.2** - Console UI with Spectre.Console integration
- ✅ **v0.2** - Bot implementations (ChaosBot, BetaBot) for AI development
- 🚧 **v0.3** - Recording game scores and decision data (in progress)
- 📋 **v0.3** - Database setup for storing game results (planned)
- 📋 **v0.4** - AI training infrastructure (planned)
- 📋 **v0.4** - Genetic algorithms implementation (planned)

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

## Features

### Game Engine (v0.2)
- **Complete Euchre Implementation**: Full rule set including trump selection, trick playing, going alone, and scoring
- **ML-Optimized Models**: Relative card and position representations reduce state space for efficient learning
  - Relative Suits: Cards represented as Trump/Off suits rather than absolute suits
  - Relative Positions: Players represented as Self/Partner/Left Opponent/Right Opponent
- **Orchestration System**: Modular components for game flow, trump selection, trick playing, and result calculation
- **Bot Framework**: Extensible `IPlayerActor` interface with multiple bot implementations
- **Comprehensive Testing**: Full unit test coverage with FluentAssertions and Moq

### Data Recording (v0.3 - In Progress)
- **Score Tracking**: Cumulative game scores recorded on completed deals
- **Decision Recording**: Framework for capturing trump calls, discards, and card plays for ML training
