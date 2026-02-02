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

public class CallTrumpTrainingDataLoaderTests
{
    private readonly Mock<IGameRepository> _mockGameRepository;
    private readonly Mock<IFeatureEngineer<CallTrumpDecisionEntity, CallTrumpTrainingData>> _mockFeatureEngineer;
    private readonly Mock<ILogger<CallTrumpTrainingDataLoader>> _mockLogger;
    private readonly Faker<CallTrumpDecisionEntity> _entityFaker;

    public CallTrumpTrainingDataLoaderTests()
    {
        _mockGameRepository = new Mock<IGameRepository>();
        _mockFeatureEngineer = new Mock<IFeatureEngineer<CallTrumpDecisionEntity, CallTrumpTrainingData>>();
        _mockLogger = new Mock<ILogger<CallTrumpTrainingDataLoader>>();

        _entityFaker = new Faker<CallTrumpDecisionEntity>()
            .RuleFor(x => x.CallTrumpDecisionId, f => f.IndexFaker)
            .RuleFor(x => x.DealId, f => f.Random.Int(1, 1000))
            .RuleFor(x => x.CardsInHandJson, _ => "[1,2,3,4,5]")
            .RuleFor(x => x.TeamScore, f => (short)f.Random.Int(0, 10))
            .RuleFor(x => x.OpponentScore, f => (short)f.Random.Int(0, 10))
            .RuleFor(x => x.DealerPosition, f => f.PickRandom(RelativePlayerPosition.Self, RelativePlayerPosition.LeftHandOpponent, RelativePlayerPosition.Partner, RelativePlayerPosition.RightHandOpponent))
            .RuleFor(x => x.UpCardJson, _ => /*lang=json,strict*/ "{\"Rank\":0,\"Suit\":0}")
            .RuleFor(x => x.ValidDecisionsJson, _ => "[0,1,2,3,4,5,6,7,8,9,10]")
            .RuleFor(x => x.ChosenDecisionJson, _ => /*lang=json,strict*/ "{\"Decision\":0}")
            .RuleFor(x => x.DecisionOrder, f => (byte)f.Random.Int(0, 7))
            .RuleFor(x => x.ActorType, f => f.PickRandom(ActorType.Chaos, ActorType.Chad, ActorType.Beta))
            .RuleFor(x => x.DidTeamWinDeal, f => f.Random.Bool())
            .RuleFor(x => x.RelativeDealPoints, f => (short)f.Random.Int(-2, 4))
            .RuleFor(x => x.DidTeamWinGame, f => f.Random.Bool());
    }

    [Fact]
    public async Task LoadTrainingDataAsync_WithValidEntities_TransformsCorrectly()
    {
        var entities = _entityFaker.Generate(10);
        var expectedTrainingData = new List<CallTrumpTrainingData>();

        foreach (var entity in entities)
        {
            var trainingData = new CallTrumpTrainingData
            {
                Card1Rank = entity.CallTrumpDecisionId % 6,
                ExpectedDealPoints = (short)((entity.CallTrumpDecisionId % 5) - 2),
            };
            expectedTrainingData.Add(trainingData);
            _mockFeatureEngineer.Setup(x => x.Transform(entity)).Returns(trainingData);
        }

        _mockGameRepository.Setup(x => x.GetCallTrumpTrainingDataAsync(
            It.IsAny<ActorType>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncEnumerable(entities));

        var loader = new CallTrumpTrainingDataLoader(
            _mockGameRepository.Object,
            _mockFeatureEngineer.Object,
            _mockLogger.Object);

        var result = await loader.LoadTrainingDataAsync(ActorType.Chaos, 10, false);

        result.Should().HaveCount(10);
        _mockFeatureEngineer.Verify(x => x.Transform(It.IsAny<CallTrumpDecisionEntity>()), Times.Exactly(10));
    }

    [Fact]
    public async Task LoadTrainingDataAsync_WithTransformError_ContinuesProcessing()
    {
        var entities = _entityFaker.Generate(5);
        var processedCount = 0;

        _mockFeatureEngineer.Setup(x => x.Transform(It.IsAny<CallTrumpDecisionEntity>()))
            .Returns<CallTrumpDecisionEntity>(entity =>
            {
                var currentCount = processedCount++;
                if (currentCount == 2)
                {
                    throw new InvalidOperationException("Transform error");
                }

                return new CallTrumpTrainingData
                {
                    ExpectedDealPoints = (short)((entity.CallTrumpDecisionId % 5) - 2),
                };
            });

        _mockGameRepository.Setup(x => x.GetCallTrumpTrainingDataAsync(
            It.IsAny<ActorType>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncEnumerable(entities));

        var loader = new CallTrumpTrainingDataLoader(
            _mockGameRepository.Object,
            _mockFeatureEngineer.Object,
            _mockLogger.Object);

        var result = await loader.LoadTrainingDataAsync(ActorType.Chaos);

        result.Should().HaveCount(4);
    }

    [Fact]
    public async Task LoadTrainingDataAsync_WithLimitParameter_ReturnsCorrectCount()
    {
        var entities = _entityFaker.Generate(100);

        _mockFeatureEngineer.Setup(x => x.Transform(It.IsAny<CallTrumpDecisionEntity>()))
            .Returns<CallTrumpDecisionEntity>(entity => new CallTrumpTrainingData
            {
                ExpectedDealPoints = (short)((entity.CallTrumpDecisionId % 5) - 2),
            });

        _mockGameRepository.Setup(x => x.GetCallTrumpTrainingDataAsync(
            It.IsAny<ActorType>(),
            50,
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncEnumerable(entities.Take(50)));

        var loader = new CallTrumpTrainingDataLoader(
            _mockGameRepository.Object,
            _mockFeatureEngineer.Object,
            _mockLogger.Object);

        var result = await loader.LoadTrainingDataAsync(ActorType.Chaos, 50);

        result.Should().HaveCount(50);
    }

    [Fact]
    public async Task LoadTrainingDataAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        var entities = _entityFaker.Generate(1000);
        var processedCount = 0;

        _mockFeatureEngineer.Setup(x => x.Transform(It.IsAny<CallTrumpDecisionEntity>()))
            .Returns<CallTrumpDecisionEntity>(entity =>
            {
                processedCount++;
                if (processedCount > 5)
                {
                    cts.Cancel();
                }

                return new CallTrumpTrainingData
                {
                    ExpectedDealPoints = (short)((entity.CallTrumpDecisionId % 5) - 2),
                };
            });

        _mockGameRepository.Setup(x => x.GetCallTrumpTrainingDataAsync(
            It.IsAny<ActorType>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncEnumerableWithCancellation(entities, cts.Token));

        var loader = new CallTrumpTrainingDataLoader(
            _mockGameRepository.Object,
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

        _mockFeatureEngineer.Setup(x => x.Transform(It.IsAny<CallTrumpDecisionEntity>()))
            .Returns<CallTrumpDecisionEntity>(entity => new CallTrumpTrainingData
            {
                ExpectedDealPoints = (short)((entity.CallTrumpDecisionId % 5) - 2),
            });

        _mockGameRepository.Setup(x => x.GetCallTrumpTrainingDataAsync(
            It.IsAny<ActorType>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncEnumerable(entities));

        var loader = new CallTrumpTrainingDataLoader(
            _mockGameRepository.Object,
            _mockFeatureEngineer.Object,
            _mockLogger.Object);

        await loader.LoadTrainingDataAsync(ActorType.Chaos, 25000);

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
