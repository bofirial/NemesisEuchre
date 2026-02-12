# NemesisEuchre

A console-based engine for training a high-performance Euchre AI using massive dataset seeding and genetic algorithms.

## Project Status

🚀 **Version 0.7-preview** - Interactive play with ASCII card display and human opponent support.

### Current State
- ✅ **v0.1** - Project infrastructure and CLI framework
- ✅ **v0.1** - Dependency injection and logging setup
- ✅ **v0.1** - Unit test infrastructure with FluentAssertions, Moq, and Bogus
- ✅ **v0.1** - GitHub Actions CI/CD pipeline
- ✅ **v0.1** - Code quality analyzers (StyleCop, Roslynator)
- ✅ **v0.2** - Complete Euchre game engine with orchestration
- ✅ **v0.2** - Game models optimized for machine learning (relative suits and positions)
- ✅ **v0.2** - Trump selection, trick playing, and deal result calculation
- ✅ **v0.2** - Console UI with Spectre.Console integration
- ✅ **v0.2** - Bot implementations (ChaosBot, BetaBot) for AI development
- ✅ **v0.3** - Recording game scores and decision data
- ✅ **v0.3** - Database setup for storing game results with SQL Server
- ✅ **v0.4** - AI training infrastructure with ML.NET and LightGBM
- ✅ **v0.4** - Gen1Bot ML-powered player with regression-based decision making
- ✅ **v0.4** - Cached prediction engine provider for efficient model inference
- ✅ **v0.5** - Model memory enhancements for game state tracking
- ✅ **v0.5** - Enhanced feature engineering with historical context
- ✅ **v0.6** - Advanced game display, Gen1TrainerBot with exploration, database normalization, streaming training
- ✅ **v0.7** - Machine Learning iteration improvments using idv files
- 🚧 **v0.8** - Behavioral Model Testing (in progress)
- 🚧 **v0.9** - Interactive play with ASCII card display

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

### Data Recording (v0.3 - Completed)
- **Score Tracking**: Cumulative game scores recorded on completed deals
- **Decision Recording**: Comprehensive capture of all game decisions (trump calls, discards, card plays)
- **Database Storage**: SQL Server with JSON columns for complex game state
- **Training Data Pipeline**: IAsyncEnumerable-based repository pattern for efficient data streaming

### Machine Learning Infrastructure (v0.4 - Completed)
- **ML Framework**: ML.NET with LightGBM regression trainers
- **Three-Model Architecture**: Separate regression models for CallTrump, DiscardCard, and PlayCard decisions
  - Each model predicts expected deal points for optimal decision-making
  - Trained independently with decision-specific feature engineering
- **Gen1Bot**: First ML-powered player implementation using regression-based strategy selection
- **Prediction Caching**: Thread-safe CachedPredictionEngineProvider for efficient model inference
- **Feature Engineering**: Modular builders for training and inference feature transformation
- **Model Persistence**: Versioned model storage with generation-based loading
- **Training Pipeline**: End-to-end workflow from data loading → splitting → training → evaluation → persistence

### Model Memory Enhancements (v0.5 - Completed)
- **Game State Tracking**: Enhanced Deal model with trump decisions, discarded cards, and known suit voids
- **Historical Context**: PlayCardDecision records now include dealer position, upcard pickup, and cards accounted for
- **Data Denormalization**: TrickNumber directly on PlayCardDecision for simplified feature engineering
- **Void Tracking**: Capture known player suit voids discovered during trick play for improved inference

### Advanced Game Display & Training Improvements (v0.6 - Completed)
- **Rich Game Results Display**: Detailed rendering of tricks, deals, and game outcomes with Spectre.Console
  - `show-game` CLI command to load and display any saved game from the database
  - `--show-decisions` flag to inspect AI decision details including predicted points
  - Dedicated DecisionRenderer for call trump, discard, and play card decision tables
- **Gen1TrainerBot**: Exploration-capable training bot using Boltzmann/softmax selection
  - Temperature-controlled stochastic decision making for discovering suboptimal-but-interesting strategies
  - Generates more diverse training data than greedy-only selection
- **Database Normalization**: Migrated JSON columns to normalized metadata tables with foreign keys
  - Replaces serialized JSON blobs with proper relational structure for all decision entities
  - Adds metadata reference tables for cards, suits, ranks, positions, and game status enums
  - Enables efficient querying and indexing on previously opaque JSON fields
- **Streaming Training Data**: Reduced ML training memory from ~26 GB to ~4-6 GB
  - IAsyncEnumerable-based data streaming to ML.NET instead of loading entire dataset into memory
  - TrainingDataLoaderBase with unified streaming pattern across all decision types
- **Immutable Game Models**: Converted Card, RelativeCard, and PlayedCard to immutable records
  - Protects against accidental state mutation in IPlayerActor implementations
  - Strongly-typed PlayerSuitVoid and RelativePlayerSuitVoid records replace raw tuples
- **Unified Feature Builders**: Extended the PlayCard consolidation pattern to CallTrump and DiscardCard
  - CallTrumpFeatureBuilder and DiscardCardFeatureBuilder eliminate training/inference duplication
  - FeatureContextBuilder pattern replaces EntityDeserializers for cleaner data transformation
- **Decision Predicted Points**: IPlayerActor methods now return decision context objects with scored alternatives
  - Persisted to database for post-hoc analysis of bot reasoning
- **Batch Results Enhancements**: Richer statistics including total tricks and per-decision-type counters
- **Legacy Cleanup**: Removed multiclass model trainers in favor of regression-only approach

### Code Quality & Performance (v0.5 - Completed)
- **Context Objects**: Eliminated parameter bloat by introducing PlayCardContext, CallTrumpContext, and DiscardCardContext
  - Reduced 17-parameter methods to single-parameter methods (94% reduction)
  - Simplified test setup code by 82% (~800 lines eliminated)
- **Feature Engineering Consolidation**: Unified PlayCardFeatureBuilder eliminates 95% duplication
  - Single source of truth for all 65 PlayCard features
  - Net elimination of 172 lines of duplicated logic
- **Performance Optimization**: Array pooling and batch deserialization reduce GC pressure by 20-30%
  - GameEnginePoolManager with float[], RelativeCard[], and PlayerVoid[] pools
  - Batch JSON deserialization consolidates 7 operations into 1
- **Infrastructure Upgrades**: xUnit v3 and Entity Framework Core 10 for improved testing and data access

## Architecture

### Machine Learning Pipeline

**Data Flow:**
```
GameOrchestrator → DecisionRecorder → GameRepository → Database
                                                          ↓
TrainingDataRepository → FeatureEngineer → DataSplitter → Trainer → ModelPersistenceService
                                                                           ↓
                                                    Gen1Bot ← CachedPredictionEngineProvider
```

**Three-Model Regression Strategy:**
1. **CallTrump Model** (39 features): Predicts expected deal points for each trump call option
2. **DiscardCard Model** (23 features): Predicts expected deal points for each discard choice
3. **PlayCard Model** (36 features): Predicts expected deal points for each valid card play

Each decision type uses LightGBM regression to evaluate all valid options and select the action with the highest predicted point value.

### Project Structure

```
NemesisEuchre.Console/          # CLI application and game orchestration
NemesisEuchre.Core/             # Game engine, models, and rule implementations
NemesisEuchre.DataAccess/       # Entity Framework Core, SQL Server, repositories
NemesisEuchre.MachineLearning/  # ML.NET trainers, feature engineering, model loading
NemesisEuchre.MachineLearning.Bots/  # Gen1Bot and future ML-powered players
NemesisEuchre.Console.Tests/    # Comprehensive test suite
```

### Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| **Regression vs Classification** | Point prediction better reflects continuous game outcomes than discrete classes |
| **Three Separate Models** | Each decision type has distinct features and strategic context |
| **LightGBM** | Fast tree-based learner, handles non-linear feature interactions well for game AI |
| **Relative Representations** | Trump/Off suits and Self/Partner/Opponent positions reduce state space complexity |
| **Cached Prediction Engines** | Model loading is expensive; thread-safe caching avoids reload per prediction |
| **Denormalized Data** | TrickNumber on PlayCardDecision eliminates navigation property overhead |
| **Context Objects** | Single-parameter methods with context objects eliminate parameter bloat and improve API clarity |
| **Array Pooling** | Reusing arrays reduces GC pressure by 20-30% during high-throughput feature engineering |
| **Unified Feature Builders** | Single source of truth eliminates duplication between training and inference code paths |
| **Immutable Records** | Card, RelativeCard, PlayedCard as records prevent bot implementations from mutating shared game state |
| **Boltzmann Exploration** | Temperature-controlled softmax selection generates diverse training data beyond greedy-optimal play |
| **Normalized Schema** | Replacing JSON columns with relational tables enables indexing, querying, and referential integrity |
| **Streaming Training** | IAsyncEnumerable data loading keeps memory at ~4-6 GB instead of ~26 GB for large datasets |

### Generational Training Vision

The project implements a generational improvement strategy:
1. **Gen1Bot** trained on rule-based bot (ChaosBot, BetaBot) gameplay data
2. **Gen2Bot** will train on Gen1Bot gameplay, learning from Gen1's strategies
3. **Iterative refinement** through successive generations, each learning from the previous
4. **Performance tracking** via win rate metrics across generations
