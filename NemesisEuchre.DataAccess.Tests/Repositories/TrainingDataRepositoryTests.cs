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
                ActorTypeId = (int)ActorType.Chaos,
                UpCardId = 209,
                ChosenDecisionValueId = 0,
                DealerRelativePositionId = 0,
                DecisionOrder = 1,
                DidTeamWinGame = true,
            },
            new CallTrumpDecisionEntity
            {
                DealId = dealId,
                ActorTypeId = (int)ActorType.Beta,
                UpCardId = 209,
                ChosenDecisionValueId = 0,
                DealerRelativePositionId = 0,
                DecisionOrder = 2,
                DidTeamWinGame = false,
            },
            new CallTrumpDecisionEntity
            {
                DealId = dealId,
                ActorTypeId = (int)ActorType.Chaos,
                UpCardId = 209,
                ChosenDecisionValueId = 0,
                DealerRelativePositionId = 0,
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
        results.Should().AllSatisfy(e => e.ActorTypeId.Should().Be((int)ActorType.Chaos));
    }

    [Fact]
    public async Task GetDecisionDataAsync_WithLimit_ReturnsLimitedResults()
    {
        const int dealId = 1;

        await _context.CallTrumpDecisions!.AddRangeAsync(
            Enumerable.Range(0, 10).Select(i => new CallTrumpDecisionEntity
            {
                DealId = dealId,
                ActorTypeId = (int)ActorType.Chaos,
                UpCardId = 209,
                ChosenDecisionValueId = 0,
                DealerRelativePositionId = 0,
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
                ActorTypeId = (int)ActorType.Chaos,
                UpCardId = 209,
                ChosenDecisionValueId = 0,
                DealerRelativePositionId = 0,
                DecisionOrder = 1,
                DidTeamWinGame = true,
            },
            new CallTrumpDecisionEntity
            {
                DealId = dealId,
                ActorTypeId = (int)ActorType.Chaos,
                UpCardId = 209,
                ChosenDecisionValueId = 0,
                DealerRelativePositionId = 0,
                DecisionOrder = 2,
                DidTeamWinGame = false,
            },
            new CallTrumpDecisionEntity
            {
                DealId = dealId,
                ActorTypeId = (int)ActorType.Chaos,
                UpCardId = 209,
                ChosenDecisionValueId = 0,
                DealerRelativePositionId = 0,
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

    [Fact]
    public async Task GetDecisionDataAsync_CallTrumpDecision_IncludesNavigationProperties()
    {
        var decision = new CallTrumpDecisionEntity
        {
            DealId = 1,
            ActorTypeId = (int)ActorType.Chaos,
            UpCardId = 209,
            ChosenDecisionValueId = 0,
            DealerRelativePositionId = 0,
            DecisionOrder = 1,
            DidTeamWinGame = true,
            CardsInHand =
            [
                new() { CardId = 101, SortOrder = 0 },
                new() { CardId = 102, SortOrder = 1 },
            ],
            ValidDecisions =
            [
                new() { CallTrumpDecisionValueId = 0 },
                new() { CallTrumpDecisionValueId = 1 },
                new() { CallTrumpDecisionValueId = 2 },
            ],
        };

        await _context.CallTrumpDecisions!.AddAsync(decision, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var results = new List<CallTrumpDecisionEntity>();
        await foreach (var entity in _repository.GetDecisionDataAsync<CallTrumpDecisionEntity>(ActorType.Chaos, cancellationToken: TestContext.Current.CancellationToken))
        {
            results.Add(entity);
        }

        results.Should().ContainSingle();
        results[0].CardsInHand.Should().HaveCount(2);
        results[0].ValidDecisions.Should().HaveCount(3);
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
