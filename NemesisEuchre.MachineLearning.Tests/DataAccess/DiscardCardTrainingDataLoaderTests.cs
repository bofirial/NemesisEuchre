using System.Runtime.CompilerServices;

using Bogus;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.Tests.DataAccess;

public class DiscardCardTrainingDataLoaderTests
{
    private readonly Mock<ITrainingDataRepository> _mockTrainingDataRepository;
    private readonly Mock<IFeatureEngineer<DiscardCardDecisionEntity, DiscardCardTrainingData>> _mockFeatureEngineer;
    private readonly Mock<ILogger<DiscardCardTrainingDataLoader>> _mockLogger;
    private readonly Faker<DiscardCardDecisionEntity> _entityFaker;

    public DiscardCardTrainingDataLoaderTests()
    {
        _mockTrainingDataRepository = new Mock<ITrainingDataRepository>();
        _mockFeatureEngineer = new Mock<IFeatureEngineer<DiscardCardDecisionEntity, DiscardCardTrainingData>>();
        _mockLogger = new Mock<ILogger<DiscardCardTrainingDataLoader>>();

        _entityFaker = new Faker<DiscardCardDecisionEntity>()
            .RuleFor(x => x.DiscardCardDecisionId, f => f.IndexFaker)
            .RuleFor(x => x.DealId, f => f.Random.Int(1, 1000))
            .RuleFor(x => x.TeamScore, f => (short)f.Random.Int(0, 10))
            .RuleFor(x => x.OpponentScore, f => (short)f.Random.Int(0, 10))
            .RuleFor(x => x.CallingRelativePlayerPositionId, f => f.Random.Int(0, 3))
            .RuleFor(x => x.CallingPlayerGoingAlone, f => f.Random.Bool())
            .RuleFor(x => x.ChosenRelativeCardId, f => f.Random.Int(9, 316))
            .RuleFor(x => x.ActorTypeId, f => f.Random.Int(0, 3))
            .RuleFor(x => x.DidTeamWinDeal, f => f.Random.Bool())
            .RuleFor(x => x.RelativeDealPoints, f => (short)f.Random.Int(-2, 4))
            .RuleFor(x => x.DidTeamWinGame, f => f.Random.Bool());
    }

    [Fact]
    public async Task LoadTrainingDataAsync_WithValidEntities_TransformsCorrectly()
    {
        var entities = _entityFaker.Generate(10);
        var expectedTrainingData = new List<DiscardCardTrainingData>();

        foreach (var entity in entities)
        {
            var trainingData = new DiscardCardTrainingData
            {
                CallingPlayerPosition = entity.DiscardCardDecisionId % 4,
                ExpectedDealPoints = (short)((entity.DiscardCardDecisionId % 5) - 2),
            };
            expectedTrainingData.Add(trainingData);
            _mockFeatureEngineer.Setup(x => x.Transform(entity)).Returns(trainingData);
        }

        _mockTrainingDataRepository.Setup(x => x.GetDecisionDataAsync<DiscardCardDecisionEntity>(
            It.IsAny<ActorType>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncEnumerable(entities));

        var loader = new DiscardCardTrainingDataLoader(
            _mockTrainingDataRepository.Object,
            _mockFeatureEngineer.Object,
            _mockLogger.Object);

        var result = await loader.LoadTrainingDataAsync(ActorType.Chaos, 10, false, TestContext.Current.CancellationToken);

        result.Should().HaveCount(10);
        _mockFeatureEngineer.Verify(x => x.Transform(It.IsAny<DiscardCardDecisionEntity>()), Times.Exactly(10));
    }

    [Fact]
    public async Task LoadTrainingDataAsync_WithTransformError_ContinuesProcessing()
    {
        var entities = _entityFaker.Generate(5);
        var processedCount = 0;

        _mockFeatureEngineer.Setup(x => x.Transform(It.IsAny<DiscardCardDecisionEntity>()))
            .Returns<DiscardCardDecisionEntity>(entity =>
            {
                var currentCount = processedCount++;
                if (currentCount == 2)
                {
                    throw new InvalidOperationException("Transform error");
                }

                return new DiscardCardTrainingData
                {
                    ExpectedDealPoints = (short)((entity.DiscardCardDecisionId % 5) - 2),
                };
            });

        _mockTrainingDataRepository.Setup(x => x.GetDecisionDataAsync<DiscardCardDecisionEntity>(
            It.IsAny<ActorType>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncEnumerable(entities));

        var loader = new DiscardCardTrainingDataLoader(
            _mockTrainingDataRepository.Object,
            _mockFeatureEngineer.Object,
            _mockLogger.Object);

        var result = await loader.LoadTrainingDataAsync(ActorType.Chaos, cancellationToken: TestContext.Current.CancellationToken);

        result.Should().HaveCount(4);
    }

    [Fact]
    public async Task LoadTrainingDataAsync_WithLimitParameter_ReturnsCorrectCount()
    {
        var entities = _entityFaker.Generate(100);

        _mockFeatureEngineer.Setup(x => x.Transform(It.IsAny<DiscardCardDecisionEntity>()))
            .Returns<DiscardCardDecisionEntity>(entity => new DiscardCardTrainingData
            {
                ExpectedDealPoints = (short)((entity.DiscardCardDecisionId % 5) - 2),
            });

        _mockTrainingDataRepository.Setup(x => x.GetDecisionDataAsync<DiscardCardDecisionEntity>(
            It.IsAny<ActorType>(),
            50,
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncEnumerable(entities.Take(50)));

        var loader = new DiscardCardTrainingDataLoader(
            _mockTrainingDataRepository.Object,
            _mockFeatureEngineer.Object,
            _mockLogger.Object);

        var result = await loader.LoadTrainingDataAsync(ActorType.Chaos, 50, cancellationToken: TestContext.Current.CancellationToken);

        result.Should().HaveCount(50);
    }

    [Fact]
    public async Task LoadTrainingDataAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        var entities = _entityFaker.Generate(1000);
        var processedCount = 0;

        _mockFeatureEngineer.Setup(x => x.Transform(It.IsAny<DiscardCardDecisionEntity>()))
            .Returns<DiscardCardDecisionEntity>(entity =>
            {
                processedCount++;
                if (processedCount > 5)
                {
                    cts.Cancel();
                }

                return new DiscardCardTrainingData
                {
                    ExpectedDealPoints = (short)((entity.DiscardCardDecisionId % 5) - 2),
                };
            });

        _mockTrainingDataRepository.Setup(x => x.GetDecisionDataAsync<DiscardCardDecisionEntity>(
            It.IsAny<ActorType>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncEnumerableWithCancellation(entities, cts.Token));

        var loader = new DiscardCardTrainingDataLoader(
            _mockTrainingDataRepository.Object,
            _mockFeatureEngineer.Object,
            _mockLogger.Object);

        var act = async () => await loader.LoadTrainingDataAsync(ActorType.Chaos, 1000, false, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task LoadTrainingDataAsync_LogsProgressEvery10000Records()
    {
        var entities = _entityFaker.Generate(25000);

        _mockLogger.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);

        _mockFeatureEngineer.Setup(x => x.Transform(It.IsAny<DiscardCardDecisionEntity>()))
            .Returns<DiscardCardDecisionEntity>(entity => new DiscardCardTrainingData
            {
                ExpectedDealPoints = (short)((entity.DiscardCardDecisionId % 5) - 2),
            });

        _mockTrainingDataRepository.Setup(x => x.GetDecisionDataAsync<DiscardCardDecisionEntity>(
            It.IsAny<ActorType>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncEnumerable(entities));

        var loader = new DiscardCardTrainingDataLoader(
            _mockTrainingDataRepository.Object,
            _mockFeatureEngineer.Object,
            _mockLogger.Object);

        await loader.LoadTrainingDataAsync(ActorType.Chaos, 25000, cancellationToken: TestContext.Current.CancellationToken);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("10000")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("20000")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void StreamTrainingData_WithValidEntities_TransformsCorrectly()
    {
        var entities = _entityFaker.Generate(10);

        foreach (var entity in entities)
        {
            var trainingData = new DiscardCardTrainingData
            {
                CallingPlayerPosition = entity.DiscardCardDecisionId % 4,
                ExpectedDealPoints = (short)((entity.DiscardCardDecisionId % 5) - 2),
            };
            _mockFeatureEngineer.Setup(x => x.Transform(entity)).Returns(trainingData);
        }

        _mockTrainingDataRepository.Setup(x => x.GetDecisionData<DiscardCardDecisionEntity>(
            It.IsAny<ActorType>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<bool>()))
            .Returns(entities);

        var loader = new DiscardCardTrainingDataLoader(
            _mockTrainingDataRepository.Object,
            _mockFeatureEngineer.Object,
            _mockLogger.Object);

        var result = loader.StreamTrainingData(
            ActorType.Chaos, 10, cancellationToken: TestContext.Current.CancellationToken).ToList();

        result.Should().HaveCount(10);
        _mockFeatureEngineer.Verify(x => x.Transform(It.IsAny<DiscardCardDecisionEntity>()), Times.Exactly(10));
    }

    [Fact]
    public void StreamTrainingData_WithTransformError_ContinuesProcessing()
    {
        var entities = _entityFaker.Generate(5);
        var processedCount = 0;

        _mockFeatureEngineer.Setup(x => x.Transform(It.IsAny<DiscardCardDecisionEntity>()))
            .Returns<DiscardCardDecisionEntity>(entity =>
            {
                var currentCount = processedCount++;
                if (currentCount == 2)
                {
                    throw new InvalidOperationException("Transform error");
                }

                return new DiscardCardTrainingData
                {
                    ExpectedDealPoints = (short)((entity.DiscardCardDecisionId % 5) - 2),
                };
            });

        _mockTrainingDataRepository.Setup(x => x.GetDecisionData<DiscardCardDecisionEntity>(
            It.IsAny<ActorType>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<bool>()))
            .Returns(entities);

        var loader = new DiscardCardTrainingDataLoader(
            _mockTrainingDataRepository.Object,
            _mockFeatureEngineer.Object,
            _mockLogger.Object);

        var result = loader.StreamTrainingData(
            ActorType.Chaos, cancellationToken: TestContext.Current.CancellationToken).ToList();

        result.Should().HaveCount(4);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1051:Intentionally using own CancellationTokenSource to test cancellation behavior", Justification = "Testing cancellation")]
    public void StreamTrainingData_WithCancellation_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        var entities = _entityFaker.Generate(1000);
        var processedCount = 0;

        _mockFeatureEngineer.Setup(x => x.Transform(It.IsAny<DiscardCardDecisionEntity>()))
            .Returns<DiscardCardDecisionEntity>(entity =>
            {
                processedCount++;
                if (processedCount > 5)
                {
                    cts.Cancel();
                }

                return new DiscardCardTrainingData
                {
                    ExpectedDealPoints = (short)((entity.DiscardCardDecisionId % 5) - 2),
                };
            });

        _mockTrainingDataRepository.Setup(x => x.GetDecisionData<DiscardCardDecisionEntity>(
            It.IsAny<ActorType>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<bool>()))
            .Returns(entities);

        var loader = new DiscardCardTrainingDataLoader(
            _mockTrainingDataRepository.Object,
            _mockFeatureEngineer.Object,
            _mockLogger.Object);

        var act = () => loader.StreamTrainingData(ActorType.Chaos, 1000, false, false, cts.Token).ToList();

        act.Should().Throw<OperationCanceledException>();
    }

    private static async IAsyncEnumerable<T> CreateAsyncEnumerable<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<T> CreateAsyncEnumerableWithCancellation<T>(
        IEnumerable<T> items,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }

        await Task.CompletedTask;
    }
}
