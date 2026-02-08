using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.DataAccess.Tests.Repositories;

public class TrainingDataRepositoryTests : IDisposable
{
    private readonly NemesisEuchreDbContext _context;
    private readonly TrainingDataRepository _repository;
    private readonly Mock<ILogger<TrainingDataRepository>> _mockLogger = new();
    private bool _disposed;

    public TrainingDataRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<NemesisEuchreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new NemesisEuchreDbContext(options);
        _repository = new TrainingDataRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetDecisionDataAsync_CallTrumpDecision_ReturnsMatchingEntities()
    {
        const int dealId = 1;

        await _context.CallTrumpDecisions!.AddRangeAsync(
            new CallTrumpDecisionEntity
            {
                DealId = dealId,
                ActorType = ActorType.Chaos,
                CardsInHandJson = "[]",
                UpCardJson = "{}",
                ValidDecisionsJson = "[]",
                ChosenDecisionJson = "{}",
                DecisionOrder = 1,
                DidTeamWinGame = true,
            },
            new CallTrumpDecisionEntity
            {
                DealId = dealId,
                ActorType = ActorType.Beta,
                CardsInHandJson = "[]",
                UpCardJson = "{}",
                ValidDecisionsJson = "[]",
                ChosenDecisionJson = "{}",
                DecisionOrder = 2,
                DidTeamWinGame = false,
            },
            new CallTrumpDecisionEntity
            {
                DealId = dealId,
                ActorType = ActorType.Chaos,
                CardsInHandJson = "[]",
                UpCardJson = "{}",
                ValidDecisionsJson = "[]",
                ChosenDecisionJson = "{}",
                DecisionOrder = 3,
                DidTeamWinGame = true,
            });

        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var results = new List<CallTrumpDecisionEntity>();
        await foreach (var entity in _repository.GetDecisionDataAsync<CallTrumpDecisionEntity>(ActorType.Chaos, cancellationToken: TestContext.Current.CancellationToken))
        {
            results.Add(entity);
        }

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(e => e.ActorType.Should().Be(ActorType.Chaos));
    }

    [Fact]
    public async Task GetDecisionDataAsync_WithLimit_ReturnsLimitedResults()
    {
        const int dealId = 1;

        await _context.CallTrumpDecisions!.AddRangeAsync(
            Enumerable.Range(0, 10).Select(i => new CallTrumpDecisionEntity
            {
                DealId = dealId,
                ActorType = ActorType.Chaos,
                CardsInHandJson = "[]",
                UpCardJson = "{}",
                ValidDecisionsJson = "[]",
                ChosenDecisionJson = "{}",
                DecisionOrder = (byte)i,
                DidTeamWinGame = true,
            }),
            TestContext.Current.CancellationToken);

        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var results = new List<CallTrumpDecisionEntity>();
        await foreach (var entity in _repository.GetDecisionDataAsync<CallTrumpDecisionEntity>(ActorType.Chaos, limit: 5, cancellationToken: TestContext.Current.CancellationToken))
        {
            results.Add(entity);
        }

        results.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetDecisionDataAsync_WinningTeamOnly_ReturnsOnlyWinners()
    {
        const int dealId = 1;

        await _context.CallTrumpDecisions!.AddRangeAsync(
            new CallTrumpDecisionEntity
            {
                DealId = dealId,
                ActorType = ActorType.Chaos,
                CardsInHandJson = "[]",
                UpCardJson = "{}",
                ValidDecisionsJson = "[]",
                ChosenDecisionJson = "{}",
                DecisionOrder = 1,
                DidTeamWinGame = true,
            },
            new CallTrumpDecisionEntity
            {
                DealId = dealId,
                ActorType = ActorType.Chaos,
                CardsInHandJson = "[]",
                UpCardJson = "{}",
                ValidDecisionsJson = "[]",
                ChosenDecisionJson = "{}",
                DecisionOrder = 2,
                DidTeamWinGame = false,
            },
            new CallTrumpDecisionEntity
            {
                DealId = dealId,
                ActorType = ActorType.Chaos,
                CardsInHandJson = "[]",
                UpCardJson = "{}",
                ValidDecisionsJson = "[]",
                ChosenDecisionJson = "{}",
                DecisionOrder = 3,
                DidTeamWinGame = true,
            });

        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var results = new List<CallTrumpDecisionEntity>();
        await foreach (var entity in _repository.GetDecisionDataAsync<CallTrumpDecisionEntity>(ActorType.Chaos, winningTeamOnly: true, cancellationToken: TestContext.Current.CancellationToken))
        {
            results.Add(entity);
        }

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(e => e.DidTeamWinGame.Should().BeTrue());
    }

    [Fact]
    public async Task GetDecisionDataAsync_NoMatchingData_ReturnsEmpty()
    {
        var results = new List<CallTrumpDecisionEntity>();
        await foreach (var entity in _repository.GetDecisionDataAsync<CallTrumpDecisionEntity>(ActorType.Chaos, cancellationToken: TestContext.Current.CancellationToken))
        {
            results.Add(entity);
        }

        results.Should().BeEmpty();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Database.EnsureDeleted();
                _context.Dispose();
            }

            _disposed = true;
        }
    }
}
