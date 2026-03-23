# NemesisEuchre

An AI-powered Euchre engine combining machine learning, generational training, and interactive web-based play.

## Project Status

**Version 0.9** - Interactive web play with real-time SignalR, ML-powered bots, and event-driven animations.

### Milestone History
- **v0.1** - Project infrastructure, CLI framework, dependency injection, CI/CD pipeline
- **v0.2** - Complete Euchre game engine with ML-optimized models (relative suits/positions)
- **v0.3** - Decision recording, SQL Server persistence, parallel batch game execution
- **v0.4** - ML.NET training infrastructure, LightGBM regression, Gen1Bot
- **v0.5** - Model memory enhancements, game state tracking, void detection
- **v0.6** - Advanced game display, Gen1TrainerBot with Boltzmann exploration, database normalization
- **v0.7** - IDV file training pipeline, chunked flush, metadata sidecars, SQL training removal
- **v0.8** - Behavioral model testing framework with parameterized test cases
- **v0.9** - Interactive web play (ASP.NET + React + SignalR), GitHub OAuth, game event pipeline

## Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22+](https://nodejs.org/) (for the React client)
- SQL Server (LocalDB or full instance)

### Console (Training & Batch Games)

```bash
# Build and run
dotnet build
dotnet run --project NemesisEuchre.Console

# Play 1000 games with ML bots, persist to IDV
dotnet run --project NemesisEuchre.Console -- --count 1000 --persist-to-idv gen5

# Train models from IDV files
dotnet run --project NemesisEuchre.Console -- train --idv-name gen5 --decision-type All

# Run behavioral model tests
dotnet run --project NemesisEuchre.Console -- test

# Merge IDV generations
dotnet run --project NemesisEuchre.Console -- merge --sources gen4,gen5 --output merged

# Run unit tests
dotnet test
```

### Web Server (Interactive Play)

```bash
# Configure secrets (first time only)
dotnet user-secrets set "ConnectionStrings:NemesisEuchreDb" "<your-connection-string>" --project NemesisEuchre.Server
dotnet user-secrets set "Jwt:SecretKey" "<random-key>" --project NemesisEuchre.Server
dotnet user-secrets set "GitHub:ClientId" "<oauth-client-id>" --project NemesisEuchre.Server
dotnet user-secrets set "GitHub:ClientSecret" "<oauth-secret>" --project NemesisEuchre.Server

# Run the server (serves React client + API)
dotnet run --project NemesisEuchre.Server
```

## Architecture

### Project Structure

```
NemesisEuchre.Foundation/            # Shared constants, enums (PlayerPosition, Suit, DealStatus, etc.)
NemesisEuchre.GameEngine/            # Game models, orchestrators, rule engine, bot framework
NemesisEuchre.DataAccess/            # Entity Framework Core, SQL Server, entity mapping
NemesisEuchre.MachineLearning/       # ML.NET trainers, feature engineering, IDV pipeline
NemesisEuchre.MachineLearning.Bots/  # ML-powered bot implementations (ModelBot, ModelTrainerBot)
NemesisEuchre.Console/               # CLI application (batch games, training, testing, merging)
NemesisEuchre.Server/                # ASP.NET web server, SignalR hub, game session management
nemesiseuchre.client/                # React + TypeScript + Tailwind CSS frontend
NemesisEuchre.*.Tests/               # 1,375 unit tests across 5 test projects
```

### Interactive Play (v0.9)

```
Browser (React)                          Server (ASP.NET)
┌─────────────────────┐                 ┌──────────────────────────────┐
│ GamePage             │  SignalR Hub    │ GameHub                      │
│  ├─ GameLobby        │◄──────────────►│  ├─ IGameSessionService      │
│  │   └─ Seat selection│               │  ├─ IInteractiveTrumpService │
│  └─ GameActive       │               │  ├─ IInteractiveCardPlayService│
│      ├─ PlayerHand   │  GameEvents    │  └─ IPlayerStateProjector    │
│      ├─ PlayingCard  │◄──────────────│      ├─ GameEvent → PlayerGameEvent│
│      ├─ UpCard       │               │      └─ Per-player projection │
│      └─ ScoreDisplay │               │                              │
│                      │               │ ActiveGameService (in-memory) │
│ useEventAnimator     │               │  └─ Game.GameEvents list     │
│  └─ Event-driven     │               │                              │
│     animation queue  │               │ ML Bots (IPlayerActor)       │
└─────────────────────┘               │  └─ On-demand model download  │
                                       └──────────────────────────────┘
```

**Game Session Flow:**
1. Users authenticate via GitHub OAuth, receive JWT tokens
2. Session leader creates/joins a named session, claims seats, assigns bots
3. `StartGameAsync` creates the game, emits `NewDealStartedEvent`, drives bot decisions
4. Human decisions arrive via SignalR hub methods (`MakeTrumpDecisionAsync`, `PlayCardAsync`, etc.)
5. Server emits `GameEvent` records on the `Game` model, projected per-player as `PlayerGameEvent`
6. Client `useEventAnimator` consumes events sequentially with speech bubbles, card animations, and progressive state updates

**Game Event Types:** NewDealStarted, TrumpDecisionMade, UpCardFlipped, UpCardPickedUp, DealerDiscarded, CardPlayed, TrickCompleted, DealCompleted, GameCompleted, WaitingForDecision

### Machine Learning Pipeline

```
Batch Games (Console)
  └─ GameOrchestrator → DecisionRecorder → IDV Files (.idv + .idv.meta.json)
                                                ↓
Training (Console)                         IDV Merge (Console)
  └─ FeatureBuilder → DataSplitter → LightGBM Trainer → Model (.zip)
                                                              ↓
Interactive Play (Server)              ModelBot ← CachedPredictionEngineProvider
  └─ AzureBlobModelFileProvider → On-demand model download
```

**Three-Model Regression Strategy:**
1. **CallTrump Model**: Predicts expected deal points for each trump call option
2. **DiscardCard Model**: Predicts expected deal points for each discard choice
3. **PlayCard Model**: Predicts expected deal points for each valid card play

Advanced variants (AdvancedCallTrump, AdvancedDiscard, AdvancedPlay) add threat-based features for deeper strategic reasoning.

### IDV Training Data Pipeline

- `GameToTrainingDataConverter` transforms games into training data without SQL persistence
- `TrainingDataAccumulator` coordinates chunked IDV writes with streaming merge
- Per-actor IDV files enable targeted training on specific bot generations
- Sidecar metadata (`.idv.meta.json`) tracks game/deal/trick/actor statistics

## Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| **Regression vs Classification** | Point prediction better reflects continuous game outcomes than discrete classes |
| **Three Separate Models** | Each decision type has distinct features and strategic context |
| **LightGBM** | Fast tree-based learner, handles non-linear feature interactions well for game AI |
| **Relative Representations** | Trump/Off suits and Self/Partner/Opponent positions reduce state space complexity |
| **Event-Driven Animations** | Server emits `GameEvent` records; client replaces fragile snapshot-diffing with sequential event consumption |
| **Hybrid State + Events** | Full `PlayerGameState` for initial load/reconnect, events for incremental animation — no full event-sourcing complexity |
| **Player-Specific Projection** | `GameEvent` contains all data; `PlayerGameEvent` redacts private info (other hands, dealer discard) per viewer |
| **IDV Files over SQL Training** | Binary IDataView files eliminate SQL round-trips, enable chunked writes, and support generation-based merging |
| **Boltzmann Exploration** | Temperature-controlled softmax generates diverse training data beyond greedy-optimal play |
| **SignalR + JWT** | Real-time bidirectional communication with stateless authentication for multi-device support |

## Bot Types

| Bot | Strategy | Use Case |
|-----|----------|----------|
| **ChaosBot** | Purely random decisions | Baseline, data generation seeding |
| **BetaBot** | Always passes, plays lowest card | Timid baseline |
| **ChadBot** | Always calls alone, plays highest trump | Aggressive baseline |
| **ModelBot** | ML regression predictions | Trained play, deployed to web |
| **ModelTrainerBot** | Boltzmann/softmax exploration | Generates diverse training data |

## Generational Training

The project implements iterative improvement:
1. **Gen1-3**: Trained on ChaosBot/BetaBot data, establishing baseline ML play
2. **Gen4**: Hyperparameter-tuned LightGBM with L1/L2 regularization sweeps
3. **Gen5**: Data composition experiments, per-team IDV, advanced threat-based features
4. Each generation trains on gameplay from previous generations, learning progressively stronger strategies

## Testing

- **1,375 unit tests** across 5 test projects (GameEngine, Console, DataAccess, MachineLearning, ML Bots)
- **Behavioral model testing**: Parameterized test cases for CallTrump, DiscardCard, and PlayCard decisions
- **Test export**: `--output-json` flag for CI/CD validation of model quality
- **Frameworks**: xUnit v3, FluentAssertions, Moq, Bogus
