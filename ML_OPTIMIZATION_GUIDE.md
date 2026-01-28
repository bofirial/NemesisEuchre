# ML Database Optimization Guide

## Overview

This document describes the ML-optimized database schema changes implemented for efficient machine learning training data storage and retrieval in NemesisEuchre.

## Phase 1 Changes (Completed)

### 1. ActorType Conversion (Enum → String Constants)

**Motivation:** Enable dynamic generation naming (e.g., `Chad_Gen1`, `Chad_Gen42`) for genetic algorithm tracking.

**Changes:**
- `ActorType` converted from enum to static class with string constants
- Database column increased from `NVARCHAR(10)` → `NVARCHAR(25)`
- All decision entities now use `string? ActorType` instead of `ActorType?`

**Usage:**
```csharp
// Old (enum)
var actorType = ActorType.Chad;

// New (string constants)
var actorType = ActorType.Chad;  // Still works! Returns "Chad"

// Generation naming
var gen5 = ActorType.GetGenerationName(ActorType.Chad, 5);  // "Chad_Gen5"

// Validation
bool isValid = ActorType.IsValid("Chad_Gen42");  // true
```

### 2. Normalized Chosen Card/Decision Columns

**Motivation:** Avoid JSON parsing overhead for frequently-queried decision data.

**Schema Changes:**

**CallTrumpDecisions:**
- Added: `ChosenDecisionType` (Pass, OrderUp, CallTrump, GoAlone)
- Added: `ChosenTrumpSuit` (Spades, Hearts, Clubs, Diamonds - if CallTrump)
- Kept: `ChosenDecisionJson` (for backward compatibility during migration)

**DiscardCardDecisions & PlayCardDecisions:**
- Added: `ChosenCardRank` (Ace, King, Queen, Jack, Ten, Nine)
- Added: `ChosenCardSuit` (Spades, Hearts, Clubs, Diamonds)
- Kept: `ChosenCardJson` (for backward compatibility during migration)

**Benefits:**
- Direct SQL queries: `WHERE ChosenCardRank = 'Jack'`
- Aggregations: `COUNT(*) GROUP BY ChosenCardRank`
- No JSON parsing in ML feature extraction

### 3. Decision Confidence Tracking

**New Column:** `DecisionConfidence` (float?) on all decision tables

**Purpose:** Store model confidence scores during decision generation.

**Usage:**
```csharp
var decision = new PlayCardDecisionEntity
{
    // ... other fields
    DecisionConfidence = 0.85f  // Model was 85% confident
};

// Query uncertain decisions for analysis
var uncertainDecisions = dbContext.PlayCardDecisions
    .Where(d => d.DecisionConfidence < 0.5f)
    .ToList();
```

### 4. Dataset Split Markers

**New Column:** `DatasetSplit` (string) on Games table - Default: "Train"

**Purpose:** Mark games for train/validation/test splits without moving data.

**Valid Values:** "Train", "Validation", "Test"

**Usage:**
```csharp
// Mark games for validation set
var validationGames = dbContext.Games
    .Where(g => g.GameId >= 10000 && g.GameId < 12000)
    .ToList();

foreach (var game in validationGames)
{
    game.DatasetSplit = "Validation";
}
await dbContext.SaveChangesAsync();

// Query only training data
var trainingDecisions = dbContext.PlayCardDecisions
    .Include(d => d.Deal.Game)
    .Where(d => d.Deal.Game.DatasetSplit == "Train")
    .ToList();
```

### 5. Training Batch Metadata

**New Table:** `TrainingBatches`

**Purpose:** Track genetic algorithm generations and dataset provenance.

**Schema:**
```sql
CREATE TABLE TrainingBatches (
    TrainingBatchId INT PRIMARY KEY IDENTITY,
    GenerationNumber INT NOT NULL,
    ModelVersion NVARCHAR(50),
    ActorType NVARCHAR(25),
    GameIdStart INT,
    GameIdEnd INT,
    TotalDecisions INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
)
```

**Usage:**
```csharp
// Record a training batch
var batch = new TrainingBatchEntity
{
    GenerationNumber = 1,
    ModelVersion = "v1.0.0",
    ActorType = ActorType.GetGenerationName(ActorType.Chad, 1),
    GameIdStart = 1,
    GameIdEnd = 10000,
    TotalDecisions = 50000
};
dbContext.TrainingBatches.Add(batch);
await dbContext.SaveChangesAsync();

// Query training history
var gen5Batches = dbContext.TrainingBatches
    .Where(b => b.GenerationNumber == 5)
    .OrderBy(b => b.CreatedAt)
    .ToList();
```

### 6. ML-Optimized Composite Indexes

**Purpose:** 10-50x faster queries for ML training data batch retrieval.

**Indexes Created:**

1. **CallTrumpDecisions:** `IX_CallTrumpDecisions_ActorType_DealId_Outcomes`
   - Columns: `ActorType`, `DealId`
   - Includes: `DidTeamWinDeal`, `DidTeamWinGame`

2. **DiscardCardDecisions:** `IX_DiscardCardDecisions_ActorType_DealId_Outcomes`
   - Columns: `ActorType`, `DealId`
   - Includes: `DidTeamWinDeal`, `DidTeamWinGame`

3. **PlayCardDecisions:** `IX_PlayCardDecisions_ActorType_TrickId_Outcomes`
   - Columns: `ActorType`, `TrickId`
   - Includes: `DidTeamWinTrick`, `DidTeamWinDeal`, `DidTeamWinGame`

4. **Games:** `IX_Games_CreatedAt_WinningTeam`
   - Columns: `CreatedAt`, `WinningTeam`

5. **Games:** `IX_Games_DatasetSplit_CreatedAt`
   - Columns: `DatasetSplit`, `CreatedAt`

6. **Deals:** `IX_Deals_GameId_DealNumber`
   - Columns: `GameId`, `DealNumber`
   - Includes: `DealStatus`, `WinningTeam`

**Query Patterns Optimized:**
```csharp
// Batch query by ActorType (uses composite index)
var chadGen5Decisions = dbContext.PlayCardDecisions
    .AsNoTracking()
    .Where(d => d.ActorType == "Chad_Gen5")
    .ToList();

// Time-range query (uses Games index)
var recentGames = dbContext.Games
    .Where(g => g.CreatedAt >= DateTime.UtcNow.AddDays(-7))
    .ToList();

// Dataset split query (uses composite index)
var trainingGames = dbContext.Games
    .Where(g => g.DatasetSplit == "Train")
    .OrderBy(g => g.CreatedAt)
    .ToList();
```

## ML.NET Query Best Practices

### Use AsNoTracking() for Read-Only Queries

```csharp
// ❌ BAD - Loads change tracking overhead
var decisions = dbContext.PlayCardDecisions
    .Where(d => d.ActorType == "Chad")
    .ToList();

// ✅ GOOD - 40% faster
var decisions = dbContext.PlayCardDecisions
    .AsNoTracking()
    .Where(d => d.ActorType == "Chad")
    .ToList();
```

### Project to DTOs (Avoid Loading Entire Entities)

```csharp
// ❌ BAD - Loads all columns, JSON deserialization
var decisions = dbContext.PlayCardDecisions
    .AsNoTracking()
    .Where(d => d.ActorType == "Chad")
    .ToList();

// ✅ GOOD - Only loads needed columns
var features = dbContext.PlayCardDecisions
    .AsNoTracking()
    .Where(d => d.ActorType == "Chad")
    .Select(d => new PlayCardFeatures
    {
        TeamScore = d.TeamScore,
        OpponentScore = d.OpponentScore,
        ChosenCardRank = d.ChosenCardRank,  // No JSON parsing!
        ChosenCardSuit = d.ChosenCardSuit,
        DidWin = d.DidTeamWinTrick ?? false
    })
    .ToArray();
```

### Batch Processing Pattern

```csharp
const int batchSize = 10_000;
int skip = 0;

while (true)
{
    var batch = await dbContext.PlayCardDecisions
        .AsNoTracking()
        .Where(d => d.ActorType == actorType)
        .OrderBy(d => d.PlayCardDecisionId)
        .Skip(skip)
        .Take(batchSize)
        .Select(d => new { /* projection */ })
        .ToArrayAsync();

    if (batch.Length == 0) break;

    // Train on batch
    var dataView = mlContext.Data.LoadFromEnumerable(batch);
    model = mlContext.BinaryClassification.Trainers.SdcaLogisticRegression()
        .Fit(dataView);

    skip += batchSize;
}
```

### Compiled Queries for Repeated Patterns

```csharp
// Define once
private static readonly Func<NemesisEuchreDbContext, string, IAsyncEnumerable<PlayCardFeatures>>
    GetDecisionsByActorCompiled = EF.CompileAsyncQuery(
        (NemesisEuchreDbContext ctx, string actorType) =>
            ctx.PlayCardDecisions
                .AsNoTracking()
                .Where(d => d.ActorType == actorType)
                .Select(d => new PlayCardFeatures { /* ... */ })
    );

// Reuse many times (avoids LINQ translation overhead)
await foreach (var decision in GetDecisionsByActorCompiled(dbContext, "Chad_Gen5"))
{
    // Process decision
}
```

## Migration Application

```bash
# Apply migration to LocalDB
dotnet ef database update --project NemesisEuchre.DataAccess

# Generate SQL script (for manual review)
dotnet ef migrations script --project NemesisEuchre.DataAccess --output migration.sql
```

## Next Steps

### Phase 2: After First Training Run (Measure First)

These optimizations should only be implemented if performance testing shows they're needed:

1. **Computed Columns** (if JSON parsing is a bottleneck):
   - HandSize, ValidOptionCount, CardsPlayedInTrick
   - Requires SQL migration with computed column definitions

2. **Materialized Views** (if joins are a bottleneck):
   - Flatten decision queries across Games → Deals → Decisions

### Not Needed at Current Scale

- Partitioning (only for > 100M records)
- Columnstore indexes (only for OLAP workloads)
- In-memory tables (only for real-time queries)

## Performance Expectations

With these optimizations:

- **Actor-filtered queries:** 10-50x faster than without composite indexes
- **Batch queries:** Efficient for 10K-50K records per batch
- **Training data loading:** ~1-2 seconds for 100K decisions (with projection)
- **Card-specific analysis:** Direct SQL queries without JSON parsing

## Validation Queries

```sql
-- Check indexes were created
SELECT
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('CallTrumpDecisions', 'DiscardCardDecisions', 'PlayCardDecisions', 'Games', 'TrainingBatches')
ORDER BY t.name, i.name;

-- Verify ActorType length
SELECT
    t.name AS TableName,
    c.name AS ColumnName,
    c.max_length AS MaxLength
FROM sys.columns c
INNER JOIN sys.tables t ON c.object_id = t.object_id
WHERE c.name = 'ActorType'
    AND t.name LIKE '%Decision%';

-- Test query performance (compare before/after)
SET STATISTICS TIME ON;
SET STATISTICS IO ON;

SELECT PlayCardDecisionId, ChosenCardRank, ChosenCardSuit, DidTeamWinTrick
FROM PlayCardDecisions
WHERE ActorType = 'Chad_Gen5';
```

## References

- **Migration File:** `NemesisEuchre.DataAccess/Migrations/*_MLOptimizationsPhase1.cs`
- **Entity Configurations:** `NemesisEuchre.DataAccess/Entities/*Entity.cs`
- **ActorType Class:** `NemesisEuchre.GameEngine/PlayerDecisionEngine/ActorType.cs`
