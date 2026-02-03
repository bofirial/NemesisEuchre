# Nemesis Euchre Roadmap

## Initial Project Structure 0.1

1. ~~Create Console Application~~
2. ~~Add Command Library~~
3. ~~Add Dependency Injection~~
4. ~~Add Unit Testing~~
5. ~~Add Versioning~~
7. ~~Add Linting and Analyzers~~
6. ~~Set up Claude w/ basic skills and instructions files~~

## Euchre Game Engine 0.2

1. ~~Build GameModels~~
2. ~~Optimize GameModels for Machine Learning~~
   * ~~Convert Suits to Relative Suits (Trump vs Not Trump) for reduced number of unique states~~
   * ~~Convert Positions to Relative Positions (Self, Partner, Left Hand Opponent, Right Hand Opponent)~~
3. ~~Create GameOrchestrator~~
4. ~~Create IPlayerActor to provide methods for all player decisions that need to be made during a game~~
   * ~~Calling Trump~~
   * ~~Discarding when Ordered Up~~
   * ~~Playing Cards~~
5. ~~Implement IPlayerActor in simple bots for data generation in order to seed machine learning~~
6. ~~Shuffle Cards and create Deals~~
7. ~~Create DealOrchestrator~~
8. ~~Update Score and return GameModel once complete~~
9. ~~Orchestrate Selecting Trump by looping through IPlayerActors~~
10. ~~Orchestrate Tricks (Playing Cards) by looping through IPlayerActors~~
    * ~~Plan Mode: **Implement the ITrickPlayingOrchestrator with unit tests.  This class should be very similar to ITrumpSelectionOrchestrator.  It should use Dependency Injection to get the IPlayerActors.**~~
    * ~~**Refactor the TrickPlayingOrchestrator to reduce method size and improve code readability.  Changes should not need to be made to any other files or unit tests.**~~
11. ~~Calculate Trick Winner~~
    * ~~Plan Mode: **Implement the ITrickWinnerCalculator with unit tests.**~~
    * ~~**Refactor the ITrickWinnerCalculator to reduce method size and improve code readability.  Changes should not need to be made to any other files or unit tests.**~~
12. ~~Update Deal with result once complete~~
    * ~~Plan Mode: **Implement the IDealResultCalculator with unit tests.**~~
    * ~~**Refactor the IDealResultCalculator to reduce method size and improve code readability.  Changes should not need to be made to any other files or unit tests.**~~
13. ~~Refactor GameEngine to improve readability~~
14. ~~Update Console App to Play a game when called~~
    * ~~Plan Mode: **Update the DefaultCommand to play a game between 4 ChaosBots.  Display the results once the game is completed using Spectre.Console.  Also update the DefaultCommand unit tests.**~~
15. ~~Consider consolidating Relative classes logic to Bots instead of making conversions in all the Orchestrators~~

## Store Game Results 0.3

This version extends the NemesisEuchre engine to capture all ML training data (CallTrump, DiscardCard, PlayCard decisions) and persist completed games to SQL Server. The approach follows existing patterns (DI, unit testing, one class per file) and enables parallel game execution for rapid dataset generation.

**Key Decisions:**
- **Database**: SQL Server
- **ORM**: Entity Framework Core with JSON columns for complex objects
- **Decision Tracking**: Three new decision record models capturing state + choice + outcome
- **Parallelization**: Batch orchestrator with configurable degree of parallelism (default 4)
- **Storage Pattern**: Repository pattern with async persistence after each game completes

### Decision Record Models (Steps 1-3)

1. ~~**Add CallTrumpDecisionRecord model**~~
   - ~~Create `NemesisEuchre.GameEngine\Models\CallTrumpDecisionRecord.cs`~~
   - ~~Include: player's hand (RelativeCard[]), upcard, dealer position, team/opponent scores, valid decision options, chosen decision, decision order (1-8 for two rounds of bidding)~~
   - ~~Add `List<CallTrumpDecisionRecord>` property to Deal model~~
   - ~~Include unit tests validating all required fields~~

2. ~~**Add DiscardCardDecisionRecord model**~~
   - ~~Create `NemesisEuchre.GameEngine\Models\DiscardCardDecisionRecord.cs`~~
   - ~~Include: dealer's relative hand before discarding, current deal state (RelativeDeal), team/opponent scores, valid relative cards to discard, chosen relative card~~
   - ~~Add `List<DiscardCardDecisionRecord>` property to Deal model~~
   - ~~Include unit tests~~

3. ~~**Add PlayCardDecisionRecord model**~~
   - ~~Create `NemesisEuchre.GameEngine\Models\PlayCardDecisionRecord.cs`~~
   - ~~Include: player's current relative hand, full deal state (RelativeDeal), team/opponent scores, valid relative cards to play, chosen relative card, relative lead position~~
   - ~~Add `List<PlayCardDecisionRecord>` property to Deal model~~
   - ~~Include unit tests~~

### Capture Decisions (Steps 4-6)

4. ~~**Capture CallTrump decisions in TrumpSelectionOrchestrator**~~
   - ~~Update `TrumpSelectionOrchestrator.GetAndValidatePlayerDecisionAsync`~~
   - ~~Capture each CallTrump decision immediately after player makes it~~
   - ~~Create CallTrumpDecisionRecord with all context and add to `deal.CallTrumpDecisions`~~
   - ~~Should capture all 4-8 decisions (two rounds of bidding)~~
   - ~~Include unit tests verifying decisions are captured correctly~~

5. ~~**Capture DiscardCard decisions in DealerDiscardHandler**~~
   - ~~Update `DealerDiscardHandler.GetDealerDiscardDecisionAsync`~~
   - ~~Capture dealer's discard decision after trump is ordered up~~
   - ~~Create DiscardCardDecisionRecord with context and add to `deal.DiscardCardDecisions`~~
   - ~~Include unit tests~~

6. ~~**Capture PlayCard decisions in TrickPlayingOrchestrator**~~
   - ~~Update `TrickPlayingOrchestrator.GetPlayerCardChoiceAsync`~~
   - ~~Capture every card play decision during trick execution~~
   - ~~Create PlayCardDecisionRecord with context and add to `deal.PlayCardDecisions`~~
   - ~~Should capture 15-20 decisions per deal (5 tricks × 3-4 players)~~
   - ~~Include unit tests~~

### Database Layer (Steps 7-11)

7. ~~**Create NemesisEuchre.DataAccess project**~~
   - ~~Create new class library: `NemesisEuchre.DataAccess\NemesisEuchre.DataAccess.csproj`~~
   - ~~Install NuGet packages:~~
     - ~~Microsoft.EntityFrameworkCore.SqlServer~~
     - ~~Microsoft.EntityFrameworkCore.Design~~
     - ~~Microsoft.EntityFrameworkCore.Tools~~
   - ~~Create project structure: Entities/, Mappers/, Repositories/~~
   - ~~Create NemesisEuchreDbContext with DbSet properties for Games, Deals, and decision entities~~
   - ~~Add connection string configuration for Sql Server in appsettings.json~~

8. ~~**Create Entity Models for database persistence**~~
   - ~~Create entity classes:~~
     - ~~`NemesisEuchre.DataAccess\Entities\GameEntity.cs`~~
     - ~~`NemesisEuchre.DataAccess\Entities\DealEntity.cs`~~
     - ~~`NemesisEuchre.DataAccess\Entities\TrickEntity.cs`~~
     - ~~`NemesisEuchre.DataAccess\Entities\CallTrumpDecisionEntity.cs`~~
     - ~~`NemesisEuchre.DataAccess\Entities\DiscardCardDecisionEntity.cs`~~
     - ~~`NemesisEuchre.DataAccess\Entities\PlayCardDecisionEntity.cs`~~
   - ~~Use JSON columns (via EF Core value converters) for RelativeCard[], RelativeDeal, valid decision arrays~~
   - ~~Include proper foreign key relationships and navigation properties~~
   - ~~Add unit tests for entity creation and relationships~~

9. **Configure DbContext with indexes and relationships**
   - ~~Configure `NemesisEuchreDbContext.OnModelCreating`~~
   - ~~Define indexes optimized for ML queries:~~
     - ~~Index on ActorType + ChosenDecision for CallTrumpDecisions~~
     - ~~Index on ActorType + TrickNumber for PlayCardDecisions~~
     - ~~Composite indexes for filtering games by date and winning team~~
   - ~~Configure JSON value converters for RelativeCard[], RelativeDeal, decision arrays~~
   - ~~Include FluentAssertions-based unit tests~~

10. ~~**Create GameToEntityMapper for model conversion**~~
    - ~~Create `NemesisEuchre.DataAccess\Mappers\GameToEntityMapper.cs`~~
    - ~~Convert Game models to GameEntity with all child entities (Deals, Tricks, Decisions)~~
    - ~~Use System.Text.Json to serialize complex objects to JSON strings~~
    - ~~Calculate denormalized outcome fields (DidTeamWinTrick, DidTeamWinDeal, DidTeamWinGame) for each decision~~
    - ~~Include comprehensive unit tests validating correct mapping and JSON serialization roundtrips~~

11. ~~**Create IGameRepository interface and implementation**~~
    - ~~Create `NemesisEuchre.DataAccess\IGameRepository.cs`~~
    - ~~Create `NemesisEuchre.DataAccess\GameRepository.cs`~~
    - ~~Implement async methods:~~
      - ~~SaveCompletedGameAsync~~
      - ~~GetTotalGamesStoredAsync~~
      - ~~GetRecentGamesAsync~~
    - ~~Use NemesisEuchreDbContext and GameToEntityMapper~~
    - ~~Include logging for save operations (elapsed time, deal count)~~
    - ~~Add proper error handling~~
    - ~~Add unit tests using EF Core in-memory database provider~~

### Database Setup (Steps 12-13)

12. ~~**Create EF Core migration and update database**~~
    - ~~Create initial migration:~~
      ```bash
      dotnet ef migrations add InitialCreate --project NemesisEuchre.DataAccess --startup-project NemesisEuchre.Console
      ```
    - ~~Review generated migration (verify table structures, indexes, relationships)~~
    - ~~Apply migration to create LocalDB database:~~
      ```bash
      dotnet ef database update --project NemesisEuchre.DataAccess --startup-project NemesisEuchre.Console
      ```

13. ~~**Add Dependency Injection for DataAccess layer**~~
    - ~~Create `NemesisEuchre.DataAccess\DependencyInjection\DataAccessServiceCollectionExtensions.cs`~~
    - ~~Implement AddDataAccess extension method~~
    - ~~Register NemesisEuchreDbContext (with SQL Server connection string from configuration)~~
    - ~~Register IGameRepository as scoped service~~
    - ~~Configure SQL Server retry logic and connection resiliency~~
    - ~~Update `NemesisEuchre.Console\Program.cs` to call AddDataAccess and apply migrations on startup~~
    - ~~Include unit tests for DI registration~~

### Game Storage (Step 14)

14. ~~**Update GameOrchestrator to save completed games**~~
    - ~~Update `NemesisEuchre.GameEngine\GameOrchestrator.cs`~~
    - ~~Call `IGameRepository.SaveCompletedGameAsync` after game completes successfully~~
    - ~~Add proper error handling and logging around save operation~~
    - ~~Save should happen asynchronously and not block game completion~~
    - ~~Include unit tests verifying repository is called with completed game~~

### Parallel Execution (Steps 15-17)

15. ~~**Create BatchGameOrchestrator for parallel execution**~~
    - ~~Create `NemesisEuchre.Console\Services\IBatchGameOrchestrator.cs`~~
    - ~~Create `NemesisEuchre.Console\Services\BatchGameOrchestrator.cs`~~
    - ~~Run multiple games in parallel using Task.WhenAll with SemaphoreSlim (default 4)~~
    - ~~Each game uses scoped IServiceProvider to avoid state sharing bugs~~
    - ~~Include IProgress<int> for reporting completion count~~
    - ~~Return BatchGameResults with statistics (total games, team1/team2 wins, elapsed time)~~
    - ~~Include unit tests with mocked IGameOrchestrator~~

16. ~~**Update DefaultCommand for parallel game execution**~~
    - ~~Update `NemesisEuchre.Console\Commands\DefaultCommand.cs`~~
    - ~~Accept --count parameter (default 1) for number of games to play~~
    - ~~When count > 1, use IBatchGameOrchestrator instead of IGameOrchestrator~~
    - ~~Display Spectre.Console progress bar showing "Playing N games..." with live updates~~
    - ~~After completion, display aggregate statistics:~~
      - ~~Total games~~
      - ~~Team1 wins / Team2 wins~~
      - ~~Win percentages~~
      - ~~Total deals played~~
    - ~~Update unit tests~~

17. ~~**Add configuration for parallel execution settings**~~
    - ~~Create `NemesisEuchre.GameEngine\Options\GameExecutionOptions.cs`~~
    - ~~Properties: ParallelGameCount (default 4), MaxDegreeOfParallelism (default 4)~~
    - ~~Add configuration section to `NemesisEuchre.Console\appsettings.json`~~
    - ~~Update BatchGameOrchestrator to use IOptions<GameExecutionOptions>~~
    - ~~Include unit tests~~

### Performance Optimization (Step 22)

22. ~~**Optimize batch save performance**~~
    - ~~Update `NemesisEuchre.DataAccess\GameRepository.cs`~~
    - ~~Support batch saving for high-throughput scenarios~~
    - ~~Accumulate N games (configurable, default 100) and save in batches~~
    - ~~Use `DbContext.ChangeTracker.AutoDetectChangesEnabled = false` during bulk operations~~
    - ~~Reduces database round-trips and improves performance when generating millions of games~~

## Machine Learning with ML.NET 0.4

This version introduces ML.NET-powered bots that learn from game data, establishing a repeatable training cycle: generate game data → train ML models → deploy new bot generation → generate more data. The approach uses multiclass classification for three decision types (CallTrump, DiscardCard, PlayCard) with generational improvement tracking.

**Key Decisions:**
- **ML Framework**: ML.NET (multiclass classification using SdcaMaximumEntropy)
- **Model Strategy**: Three separate models per bot generation (CallTrump, Discard, PlayCard)
- **Bot Versioning**: Gen1Bot → Gen2Bot → Gen3Bot... (each trained on previous generation's data)
- **Project Structure**: New `NemesisEuchre.MachineLearning` project for training infrastructure
- **Feature Engineering**: Encode RelativeCard arrays, scores, positions, game state into ML.NET vectors
- **Evaluation**: Measure improvement via head-to-head matchups (target: 52%+ win rate vs previous gen)

### Phase 1: Data Extraction & Feature Engineering (Steps 1-5)

1. ~~**Create NemesisEuchre.MachineLearning project**~~
   - ~~Create new class library: `NemesisEuchre.MachineLearning\NemesisEuchre.MachineLearning.csproj`~~
   - ~~Install NuGet packages:~~
     - ~~Microsoft.ML (core ML.NET package)~~
     - ~~Microsoft.ML.FastTree (optional: additional algorithms)~~
   - ~~Create project structure: Models/, Trainers/, DataAccess/, FeatureEngineering/~~
   - ~~Add project references to NemesisEuchre.DataAccess and NemesisEuchre.GameEngine~~
   - ~~Create DI registration extension method for ML services~~

2. ~~**Create ML training data DTOs**~~
   - ~~Create `NemesisEuchre.MachineLearning\Models\CallTrumpTrainingData.cs`~~
     - ~~Feature properties: encoded hand (RelativeCard arrays as rank/suit integers), upcard, dealer position, team/opponent scores, decision order~~
     - ~~Label property: ChosenDecisionIndex (0-based integer for multiclass classification)~~
   - ~~Create `NemesisEuchre.MachineLearning\Models\DiscardCardTrainingData.cs`~~
     - ~~Feature properties: encoded hand, team/opponent scores, calling player position~~
     - ~~Label property: ChosenCardIndex (0-5, representing which card to discard)~~
   - ~~Create `NemesisEuchre.MachineLearning\Models\PlayCardTrainingData.cs`~~
     - ~~Feature properties: encoded hand, lead player, lead suit, played cards, team/opponent scores, trick state~~
     - ~~Label property: ChosenCardIndex (0-based index into valid cards array)~~

3. ~~**Implement feature engineering**~~
   - ~~Create `NemesisEuchre.MachineLearning\FeatureEngineering\IFeatureEngineer.cs` interface~~
   - ~~Create `NemesisEuchre.MachineLearning\FeatureEngineering\CallTrumpFeatureEngineer.cs`~~
   - ~~Create `NemesisEuchre.MachineLearning\FeatureEngineering\DiscardCardFeatureEngineer.cs`~~
   - ~~Create `NemesisEuchre.MachineLearning\FeatureEngineering\PlayCardFeatureEngineer.cs`~~
   - ~~Include unit tests verifying correct encoding and label mapping~~

4. ~~**Add repository methods for ML data queries**~~
   - ~~Extend `NemesisEuchre.DataAccess\IGameRepository.cs` with ML-specific queries:~~
     - ~~`IAsyncEnumerable<CallTrumpDecisionEntity> GetCallTrumpTrainingDataAsync(ActorType actorType, int limit = 0, bool winningTeamOnly = false)`~~
     - ~~`IAsyncEnumerable<DiscardCardDecisionEntity> GetDiscardCardTrainingDataAsync(ActorType actorType, int limit = 0, bool winningTeamOnly = false)`~~
     - ~~`IAsyncEnumerable<PlayCardDecisionEntity> GetPlayCardTrainingDataAsync(ActorType actorType, int limit = 0, bool winningTeamOnly = false)`~~
   - ~~Use IAsyncEnumerable for streaming large datasets without loading all into memory~~
   - ~~Filter by ActorType to get decisions from specific bot generations~~
   - ~~Optional winningTeamOnly parameter to train only on successful strategies~~
   - ~~Include unit tests using EF Core in-memory database with sample decision records~~

5. ~~**Implement train/validation/test splitting**~~
   - ~~Create `NemesisEuchre.MachineLearning\DataAccess\DataSplitter.cs` utility class~~
   - ~~Split data into: 70% train, 15% validation, 15% test (configurable ratios)~~
   - ~~Support stratified sampling by outcome (DidTeamWinDeal) if needed~~
   - ~~Shuffle with configurable seed for reproducibility~~
   - ~~Return three IDataView instances for ML.NET consumption~~
   - ~~Include unit tests verifying split ratios and shuffling~~

### Phase 2: ML Model Training (Steps 6-10)

6. ~~**Create model trainer interfaces and implementations**~~
   - ~~Create `NemesisEuchre.MachineLearning\Trainers\IModelTrainer<TData, TLabel>.cs` generic interface~~
     - ~~Methods: TrainAsync, EvaluateAsync, SaveModelAsync~~
   - ~~Create `NemesisEuchre.MachineLearning\Trainers\CallTrumpModelTrainer.cs`~~
     - ~~Implements IModelTrainer<CallTrumpTrainingData, int>~~
     - ~~Trains 11-class multiclass classification model~~
   - ~~Create `NemesisEuchre.MachineLearning\Trainers\DiscardCardModelTrainer.cs`~~
     - ~~Implements IModelTrainer<DiscardCardTrainingData, int>~~
     - ~~Trains 6-class multiclass classification model~~
   - ~~Create `NemesisEuchre.MachineLearning\Trainers\PlayCardModelTrainer.cs`~~
     - ~~Implements IModelTrainer<PlayCardTrainingData, int>~~
     - ~~Trains variable-class model (1-13 possible cards to play)~~
   - ~~Include unit tests using small synthetic datasets~~

7. ~~**Build ML.NET pipelines**~~
   - ~~Define feature columns and transformations in each trainer~~
   - ~~Use ML.NET's `Concatenate` to combine all features into single "Features" vector~~
   - ~~Example pipeline:~~
     ```
     .Append(mlContext.Transforms.Concatenate("Features", "Hand_Rank1", "Hand_Suit1", ..., "TeamScore", "OpponentScore"))
     .Append(mlContext.Transforms.Conversion.MapValueToKey("Label", "ChosenDecisionIndex"))
     .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
     .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"))
     ```
   - ~~Document alternative algorithms in comments: FastTree, LightGbm, OneVersusAll~~
   - ~~Configure hyperparameters (learning rate, max iterations)~~

8. ~~**Implement training workflow**~~
   - ~~Load training data from IGameRepository using feature engineers~~
   - ~~Convert IEnumerable<TrainingData> to IDataView~~
   - ~~Split into train/validation sets using DataSplitter~~
   - ~~Fit ML.NET pipeline on training data~~
   - ~~Cache trained PredictionEngine for fast inference~~
   - ~~Log training metrics: elapsed time, sample count, memory usage~~
   - ~~Include unit tests verifying pipeline execution~~

9. ~~**Add model evaluation**~~
   - ~~Evaluate trained model on validation set~~
   - ~~Calculate multiclass metrics:~~
     - ~~MacroAccuracy (average accuracy across all classes)~~
     -~~ MicroAccuracy (overall accuracy)~~
     - ~~LogLoss (lower is better)~~
     - ~~PerClassPrecision, PerClassRecall, PerClassF1~~
   - ~~Generate confusion matrix for error analysis~~
   - ~~Log evaluation report to console and file~~
   - ~~Return EvaluationReport DTO with all metrics~~
   - ~~Include unit tests verifying metric calculation~~

10. ~~**Implement model serialization**~~
    - ~~Save trained ML.NET models to `Models/gen{N}_{decisionType}_v{version}.zip`~~
      - ~~Example: `Models/gen1_calltrump_v1.zip`~~
    - ~~Store model metadata in accompanying JSON file:~~
      - ~~Training date/time~~
      - ~~ActorType (which bot's data was used)~~
      - ~~Sample count (train/validation/test)~~
      - ~~Evaluation metrics (accuracy, log loss)~~
      - ~~Feature schema version~~
    - ~~Version management: increment version on each training run~~
    - ~~Create ModelLoader utility for loading models at runtime~~
    - ~~Include unit tests for save/load roundtrip~~

### Phase 3: Gen1Bot Implementation (Steps 11-14)

11. ~~**Create Gen1Bot class**~~
    - ~~Create `NemesisEuchre.GameEngine\PlayerBots\Gen1Bot.cs`~~
    - ~~Inherit from BotBase (same pattern as ChaosBot)~~
    - ~~Add ActorType.Gen1 to `NemesisEuchre.Foundation.Constants\ActorType` enum~~
    - ~~Override ActorType property to return ActorType.Gen1~~
    - ~~Constructor: accepts three model file paths (CallTrump, Discard, PlayCard)~~
    - ~~Load ML.NET PredictionEngines for each model in constructor~~
    - ~~Implement IDisposable to clean up PredictionEngines~~
    - ~~Include unit tests with mock PredictionEngines~~

12. ~~**Implement CallTrumpAsync prediction**~~
    - ~~Override `CallTrumpAsync` protected method from BotBase~~
    - ~~Convert input parameters to CallTrumpTrainingData features~~
    - ~~Use ML.NET PredictionEngine to predict decision index~~
    - ~~Map prediction index back to CallTrumpDecision (0 → Pass, 1-4 → OrderUp/CallSuit, etc.)~~
    - ~~Validate prediction is in validCallTrumpDecisions array~~
    - ~~Fallback to random selection if prediction is invalid~~
    - ~~Include unit tests verifying correct prediction and fallback~~

13. ~~**Implement DiscardCardAsync prediction**~~
    - ~~Override `DiscardCardAsync` protected method from BotBase~~
    - ~~Convert RelativeCard[] hand to DiscardCardTrainingData features~~
    - ~~Use ML.NET PredictionEngine to predict card index (0-5)~~
    - ~~Map prediction index to specific RelativeCard in hand~~
    - ~~Validate predicted card is in validCardsToDiscard array~~
    - ~~Fallback to random selection if prediction is invalid~~
    - ~~Include unit tests~~

14. ~~**Implement PlayCardAsync prediction**~~
    - ~~Override `PlayCardAsync` protected method from BotBase~~
    - ~~Convert game state to PlayCardTrainingData features~~
    - ~~Use ML.NET PredictionEngine to predict card index~~
    - ~~Map prediction index to RelativeCard in validCardsToPlay array~~
    - ~~Handle variable-length valid cards list (1-13 possible cards)~~
    - ~~Fallback to random selection if prediction is out of bounds or invalid~~
    - ~~Include unit tests with various trick scenarios~~

### Phase 4: Evaluation & Iteration (Steps 15-18)

15. ~~**Generate Gen1Bot vs ChaosBot games**~~
    - ~~Use existing BatchGameOrchestrator from v0.3~~
    - ~~Configure matchup: Team1 (2 Gen1Bots) vs Team2 (2 ChaosBots)~~
    - ~~Generate 10,000+ games for statistical significance~~
    - ~~Store results in database via existing GameRepository~~
    - ~~Display progress with Spectre.Console progress bar~~
    - ~~Calculate win rate metrics after completion~~

16. ~~**Create evaluation report**~~
    - ~~Query database for Gen1 vs Chaos games using GameRepository~~
    - ~~Calculate metrics:~~
      - ~~Win rate (Team1 wins / total games)~~
      - ~~Average points per game for each team~~
      - ~~Euchre rate (how often teams get euchred)~~
      - ~~Average deal length~~
    - ~~Compare to Chaos vs Chaos baseline (expected ~50% win rate)~~
    - ~~Document improvement percentage (e.g., "Gen1Bot wins 54% → 4% improvement")~~
    - ~~Export report to markdown file: `Reports/gen1_vs_chaos_evaluation.md`~~
    - ~~Include statistical significance test (z-test for proportions)~~

17. ~~**Train Gen2Bot from Gen1Bot data**~~
    - ~~Query Gen1Bot decision records from database (use ActorType.Gen1 filter)~~
    - ~~Feature engineer Gen1Bot decisions (NOT ChaosBot data)~~
    - ~~Train three new models using Gen1Bot's gameplay data~~
    - ~~Save models as `Models/gen2_*.zip`~~
    - ~~Create `NemesisEuchre.GameEngine\PlayerBots\Gen2Bot.cs` (add ActorType.Gen2 to enum)~~
    - ~~Load Gen2 models in Gen2Bot constructor~~
    - ~~Include unit tests for Gen2Bot~~

18. ~~**Evaluate Gen2Bot vs Gen1Bot**~~
    - ~~Generate 10,000+ games: Gen2Bot vs Gen1Bot~~
    - ~~Calculate improvement metrics (expect Gen2 to improve over Gen1)~~
    - ~~Document generational improvement pattern~~
    - ~~Establish criteria for "good enough" model:~~
      - ~~55%+ win rate vs previous generation~~
      - ~~Consistent improvement over 3+ generations~~
      - ~~Diminishing returns threshold (e.g., <1% improvement)~~
    - ~~Create visualization of win rate trends across generations~~

### Phase 5: Infrastructure & Tooling (Steps 19-22)

19. ~~**Add ML training console command**~~
    - ~~Create `NemesisEuchre.Console\Commands\TrainCommand.cs`~~
    - ~~Parameters:~~
      - ~~`--actor-type` (Chaos, Gen1, Gen2, etc.) - which bot's data to train on~~
      - ~~`--decision-type` (CallTrump, Discard, PlayCard, All) - which model(s) to train~~
      - ~~`--output-path` (optional) - where to save models (default: Models/)~~
      - ~~`--sample-limit` (optional) - max training samples (default: all available)~~
    - ~~Use DI to get IModelTrainer implementations~~
    - ~~Display training progress with Spectre.Console:~~
      - ~~Loading data... (with count)~~
      - ~~Training model... (with progress bar if possible)~~
      - ~~Evaluating model... (show accuracy metrics)~~
    - ~~Output model paths and evaluation metrics to console~~
    - ~~Example:~~
      ```bash
      dotnet run --project NemesisEuchre.Console -- train --actor-type Chaos --decision-type All --sample-limit 50000
      ```
    - ~~Include unit tests for command parsing and execution~~

20. ~~**Add model version management**~~
    - ~~Create `NemesisEuchre.MachineLearning\Models\IModelRepository.cs` interface~~
    - ~~Create `NemesisEuchre.MachineLearning\Models\ModelRepository.cs` implementation~~
    - Methods:
      - ~~`ListAvailableModelsAsync(ActorType?, DecisionType?)` - list all models~~
      - ~~`GetLatestModelPathAsync(ActorType, DecisionType)` - get most recent model~~
      - ~~`LoadModelMetadataAsync(string modelPath)` - read JSON metadata~~
      - ~~`DeleteModelAsync(string modelPath)` - remove obsolete models~~
    - ~~Store models with timestamp: `gen1_calltrump_20260129_143052_v1.zip`~~
    - ~~Include unit tests with temporary file system~~

## Model Memory Improvements 0.5

1. ~~Record CallTrumpDecision in Deals~~
2. Record DiscardedCard in Deals
3. Record KnownPlayerSuitVoids in Deals
4. Record DealerPosition and DealerPickedUpCard in PlayCardDecisionRecords
5. Record KnownPlayerSuitVoids in PlayCardDecisionRecords
6. Record CardsAccountedFor in PlayCardDecisionRecords
7. Add CardsAccountedFor to PlayCardAsync in IPlayerActor
8. Add KnownPlayerSuitVoids to PlayCardAsync in IPlayerActor
9. Add CardsAccountedFor to PlayCardTrainingData
10. Add KnownPlayerSuitVoids to PlayCardTrainingData
11. Add KnownPlayerSuitVoids to PlayCardTrainingData
12. Add DealerPosition and DealerPickedUpCard to PlayCardTrainingData

## User Players 1.0

1. Add a PlayGame command to allow interactive play
2. Implement IPlayerActor with methods in NemesisEuchre.Console for handling player actions
3. Create Display for Cards in terminal ASCII art
4. Add interactivity for selecting an option or card from the terminal
5. Create Display for current state of Deal
6. Create Display for current state of Trick
7. Implement Call Trump action in the terminal
8. Implement Discard Card action in the terminal
9. Implement Play Card action in the terminal
10. Add animations? to simulate bot actions and allow for human reactions
11. CONSIDER Adding hooks into the GameEngine to allow players to get GameState updates in more real time (Would allow for multiple players in the future)
12. Play games against ChaosBot to record win percentage
13. Play games against GenX bots to record win percentage and prove they are improving

## AI Improvements

1. ~~Explore Refactor Slash Commands~~
   * ~~Refactor the {variable} to reduce method size and improve code readability.  Changes should not need to be made to any other files or unit tests.~~
2. ~~Explore TDD Slash Commands~~
   * ~~Is this something I want to do or just a way to burn more tokens?~~
3. Explore Skills
   * When would I use this?
4. Improvements to AGENTS.md
   * I currently need to refactor after implementation.  Is there something I could add to AGENTS.md to improve the code readability on first implementation?
   * Now that there is more code can Claude improve itself with Architecture diagrams?
5. Token Usage
   * I have been using a lot of tokens.  Is this normal?  Could improvements be made?
   * Test using Opus to save on refactoring time
   * Test using Haiku to save on token use
   * Test skipping plan mode for mid-complexity tasks
   * Test changes to Agents.md to reduce token use and improve code quality
   * Test changes to Prompts to reduce token use and improve code quality
6. Consider which Analyzers are being helpful and which may just be causing extra development churn
   * Maybe disable more rules in .editorconfig
