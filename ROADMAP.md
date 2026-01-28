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

16. **Update DefaultCommand for parallel game execution**
    - Update `NemesisEuchre.Console\Commands\DefaultCommand.cs`
    - Accept --count parameter (default 1) for number of games to play
    - When count > 1, use IBatchGameOrchestrator instead of IGameOrchestrator
    - Display Spectre.Console progress bar showing "Playing N games..." with live updates
    - After completion, display aggregate statistics:
      - Total games
      - Team1 wins / Team2 wins
      - Win percentages
      - Total deals played
    - Update unit tests

17. **Add configuration for parallel execution settings**
    - Create `NemesisEuchre.GameEngine\Options\GameExecutionOptions.cs`
    - Properties: ParallelGameCount (default 4), MaxDegreeOfParallelism (default 4)
    - Add configuration section to `NemesisEuchre.Console\appsettings.json`
    - Update BatchGameOrchestrator to use IOptions<GameExecutionOptions>
    - Include unit tests

### Performance Optimization (Step 22)

22. **Optimize batch save performance**
    - Update `NemesisEuchre.DataAccess\GameRepository.cs`
    - Support batch saving for high-throughput scenarios
    - Accumulate N games (configurable, default 100) and save in batches
    - Use `DbContext.ChangeTracker.AutoDetectChangesEnabled = false` during bulk operations
    - Reduces database round-trips and improves performance when generating millions of games
    - Create `NemesisEuchre.DataAccess.Tests\Performance\GameRepositoryBenchmarks.cs`
    - Include performance benchmark tests

### Verification & Success Criteria

After completing all steps, verify:
- ✅ All CallTrump, DiscardCard, PlayCard decisions captured during gameplay
- ✅ SQL Server LocalDB installed and database created via EF migrations
- ✅ Completed games persist to database with all child entities
- ✅ Console app runs N games in parallel (configurable, default 4)
- ✅ Progress bar shows real-time completion status
- ✅ Aggregate statistics displayed after batch completion
- ✅ Unit tests >80% coverage for all new code
- ✅ Integration tests validate full data pipeline
- ✅ Sample SQL queries documented for ML exploration
- ✅ 10,000+ game performance benchmark passes
- ✅ Database size acceptable (<100MB for 10k games)

**End-to-End Testing:**
1. Single game storage: `dotnet run --project NemesisEuchre.Console`
2. Parallel execution: `dotnet run --project NemesisEuchre.Console -- --count 100`
3. Database verification (SSMS):
   ```sql
   SELECT COUNT(*) FROM Games;
   SELECT COUNT(*) FROM Deals;
   SELECT COUNT(*) FROM CallTrumpDecisions;
   SELECT COUNT(*) FROM PlayCardDecisions;
   ```
4. Performance benchmark: `dotnet run --project NemesisEuchre.Console -- --count 10000`

## Machine Learning 0.4

1. Use Console App to Generate TONS of Euchre Game data using ChaosBots
2. Use Machine Learning? to analyze data in order to create a new bot (Gen1) that can play better than ChaosBot (Use R?, Stored Procedures?)
3. Implement the new (Gen1) Bot
4. Use Console App to Generate TONS of Euchre Game data using Gen1Bots
5. etc.

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
