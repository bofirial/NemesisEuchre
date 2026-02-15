using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Services;
using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.Console.Tests.Services;

public class GameToTrainingDataConverterTests
{
    private readonly Mock<IGameToEntityMapper> _mockMapper;
    private readonly Mock<IFeatureEngineer<PlayCardDecisionEntity, PlayCardTrainingData>> _mockPlayCardEngineer;
    private readonly Mock<IFeatureEngineer<CallTrumpDecisionEntity, CallTrumpTrainingData>> _mockCallTrumpEngineer;
    private readonly Mock<IFeatureEngineer<DiscardCardDecisionEntity, DiscardCardTrainingData>> _mockDiscardCardEngineer;
    private readonly Mock<ILogger<GameToTrainingDataConverter>> _mockLogger;
    private readonly GameToTrainingDataConverter _converter;

    public GameToTrainingDataConverterTests()
    {
        _mockMapper = new Mock<IGameToEntityMapper>();
        _mockPlayCardEngineer = new Mock<IFeatureEngineer<PlayCardDecisionEntity, PlayCardTrainingData>>();
        _mockCallTrumpEngineer = new Mock<IFeatureEngineer<CallTrumpDecisionEntity, CallTrumpTrainingData>>();
        _mockDiscardCardEngineer = new Mock<IFeatureEngineer<DiscardCardDecisionEntity, DiscardCardTrainingData>>();
        _mockLogger = new Mock<ILogger<GameToTrainingDataConverter>>();

        _converter = new GameToTrainingDataConverter(
            _mockMapper.Object,
            _mockPlayCardEngineer.Object,
            _mockCallTrumpEngineer.Object,
            _mockDiscardCardEngineer.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Convert_WithValidGames_ProducesCorrectTrainingData()
    {
        var game = CreateGameWithPlayers();
        var games = new List<Game> { game };

        var callTrumpDecision = new CallTrumpDecisionEntity { RelativeDealPoints = 2 };
        var discardCardDecision = new DiscardCardDecisionEntity { RelativeDealPoints = 1 };
        var playCardDecision = new PlayCardDecisionEntity { RelativeDealPoints = 3 };

        var gameEntity = new GameEntity
        {
            Deals =
            [
                new DealEntity
                {
                    CallTrumpDecisions = [callTrumpDecision],
                    DiscardCardDecisions = [discardCardDecision],
                    PlayCardDecisions = [playCardDecision],
                },
            ],
        };

        _mockMapper.Setup(m => m.Map(game)).Returns(gameEntity);

        var expectedCallTrumpData = new CallTrumpTrainingData();
        var expectedDiscardCardData = new DiscardCardTrainingData();
        var expectedPlayCardData = new PlayCardTrainingData();

        _mockCallTrumpEngineer.Setup(e => e.Transform(callTrumpDecision)).Returns(expectedCallTrumpData);
        _mockDiscardCardEngineer.Setup(e => e.Transform(discardCardDecision)).Returns(expectedDiscardCardData);
        _mockPlayCardEngineer.Setup(e => e.Transform(playCardDecision)).Returns(expectedPlayCardData);

        var result = _converter.Convert(games);

        result.CallTrumpData.Should().ContainSingle().Which.Should().BeSameAs(expectedCallTrumpData);
        result.DiscardCardData.Should().ContainSingle().Which.Should().BeSameAs(expectedDiscardCardData);
        result.PlayCardData.Should().ContainSingle().Which.Should().BeSameAs(expectedPlayCardData);
    }

    [Fact]
    public void Convert_FiltersOutDecisionsWithNullRelativeDealPoints()
    {
        var game = CreateGameWithPlayers();
        var games = new List<Game> { game };

        var validCallTrumpDecision = new CallTrumpDecisionEntity { RelativeDealPoints = 2 };
        var invalidCallTrumpDecision = new CallTrumpDecisionEntity { RelativeDealPoints = null };
        var validDiscardCardDecision = new DiscardCardDecisionEntity { RelativeDealPoints = 1 };
        var invalidDiscardCardDecision = new DiscardCardDecisionEntity { RelativeDealPoints = null };
        var validPlayCardDecision = new PlayCardDecisionEntity { RelativeDealPoints = 3 };
        var invalidPlayCardDecision = new PlayCardDecisionEntity { RelativeDealPoints = null };

        var gameEntity = new GameEntity
        {
            Deals =
            [
                new DealEntity
                {
                    CallTrumpDecisions = [validCallTrumpDecision, invalidCallTrumpDecision],
                    DiscardCardDecisions = [validDiscardCardDecision, invalidDiscardCardDecision],
                    PlayCardDecisions = [validPlayCardDecision, invalidPlayCardDecision],
                },
            ],
        };

        _mockMapper.Setup(m => m.Map(game)).Returns(gameEntity);

        var expectedCallTrumpData = new CallTrumpTrainingData();
        var expectedDiscardCardData = new DiscardCardTrainingData();
        var expectedPlayCardData = new PlayCardTrainingData();

        _mockCallTrumpEngineer.Setup(e => e.Transform(validCallTrumpDecision)).Returns(expectedCallTrumpData);
        _mockDiscardCardEngineer.Setup(e => e.Transform(validDiscardCardDecision)).Returns(expectedDiscardCardData);
        _mockPlayCardEngineer.Setup(e => e.Transform(validPlayCardDecision)).Returns(expectedPlayCardData);

        var result = _converter.Convert(games);

        result.CallTrumpData.Should().ContainSingle();
        result.DiscardCardData.Should().ContainSingle();
        result.PlayCardData.Should().ContainSingle();

        _mockCallTrumpEngineer.Verify(
            e => e.Transform(invalidCallTrumpDecision),
            Times.Never,
            "Should not transform decisions with null RelativeDealPoints");
        _mockDiscardCardEngineer.Verify(
            e => e.Transform(invalidDiscardCardDecision),
            Times.Never,
            "Should not transform decisions with null RelativeDealPoints");
        _mockPlayCardEngineer.Verify(
            e => e.Transform(invalidPlayCardDecision),
            Times.Never,
            "Should not transform decisions with null RelativeDealPoints");
    }

    [Fact]
    public void Convert_WithEmptyGameList_ReturnsEmptyTrainingDataBatch()
    {
        var games = new List<Game>();

        var result = _converter.Convert(games);

        result.CallTrumpData.Should().BeEmpty();
        result.DiscardCardData.Should().BeEmpty();
        result.PlayCardData.Should().BeEmpty();
    }

    [Fact]
    public void Convert_WhenFeatureEngineerThrows_ContinuesProcessing()
    {
        var game = CreateGameWithPlayers();
        var games = new List<Game> { game };

        var callTrumpDecision1 = new CallTrumpDecisionEntity { RelativeDealPoints = 2 };
        var callTrumpDecision2 = new CallTrumpDecisionEntity { RelativeDealPoints = 3 };

        var gameEntity = new GameEntity
        {
            Deals =
            [
                new DealEntity
                {
                    CallTrumpDecisions = [callTrumpDecision1, callTrumpDecision2],
                },
            ],
        };

        _mockMapper.Setup(m => m.Map(game)).Returns(gameEntity);

        var expectedException = new InvalidOperationException("Feature engineering failed");
        _mockCallTrumpEngineer
            .Setup(e => e.Transform(callTrumpDecision1))
            .Throws(expectedException);

        var expectedCallTrumpData = new CallTrumpTrainingData();
        _mockCallTrumpEngineer
            .Setup(e => e.Transform(callTrumpDecision2))
            .Returns(expectedCallTrumpData);

        var result = _converter.Convert(games);

        result.CallTrumpData.Should().ContainSingle().Which.Should().BeSameAs(expectedCallTrumpData);
    }

    [Fact]
    public void Convert_WhenMultipleFeatureEngineersThrow_ReturnsEmptyLists()
    {
        var game = CreateGameWithPlayers();
        var games = new List<Game> { game };

        var callTrumpDecision = new CallTrumpDecisionEntity { RelativeDealPoints = 2 };
        var discardCardDecision = new DiscardCardDecisionEntity { RelativeDealPoints = 1 };
        var playCardDecision = new PlayCardDecisionEntity { RelativeDealPoints = 3 };

        var gameEntity = new GameEntity
        {
            Deals =
            [
                new DealEntity
                {
                    CallTrumpDecisions = [callTrumpDecision],
                    DiscardCardDecisions = [discardCardDecision],
                    PlayCardDecisions = [playCardDecision],
                },
            ],
        };

        _mockMapper.Setup(m => m.Map(game)).Returns(gameEntity);

        _mockCallTrumpEngineer
            .Setup(e => e.Transform(callTrumpDecision))
            .Throws(new InvalidOperationException("Error 1"));
        _mockDiscardCardEngineer
            .Setup(e => e.Transform(discardCardDecision))
            .Throws(new InvalidOperationException("Error 2"));
        _mockPlayCardEngineer
            .Setup(e => e.Transform(playCardDecision))
            .Throws(new InvalidOperationException("Error 3"));

        var result = _converter.Convert(games);

        result.CallTrumpData.Should().BeEmpty();
        result.DiscardCardData.Should().BeEmpty();
        result.PlayCardData.Should().BeEmpty();
    }

    [Fact]
    public void Convert_CallsMapperForEachGame()
    {
        var game1 = CreateGameWithPlayers();
        var game2 = CreateGameWithPlayers();
        var games = new List<Game> { game1, game2 };

        var gameEntity1 = new GameEntity { Deals = [] };
        var gameEntity2 = new GameEntity { Deals = [] };

        _mockMapper.Setup(m => m.Map(game1)).Returns(gameEntity1);
        _mockMapper.Setup(m => m.Map(game2)).Returns(gameEntity2);

        _converter.Convert(games);

        _mockMapper.Verify(m => m.Map(game1), Times.Once);
        _mockMapper.Verify(m => m.Map(game2), Times.Once);
    }

    [Fact]
    public void Convert_CallsCorrectFeatureEngineerForEachDecisionType()
    {
        var game = CreateGameWithPlayers();
        var games = new List<Game> { game };

        var callTrumpDecision = new CallTrumpDecisionEntity { RelativeDealPoints = 2 };
        var discardCardDecision = new DiscardCardDecisionEntity { RelativeDealPoints = 1 };
        var playCardDecision = new PlayCardDecisionEntity { RelativeDealPoints = 3 };

        var gameEntity = new GameEntity
        {
            Deals =
            [
                new DealEntity
                {
                    CallTrumpDecisions = [callTrumpDecision],
                    DiscardCardDecisions = [discardCardDecision],
                    PlayCardDecisions = [playCardDecision],
                },
            ],
        };

        _mockMapper.Setup(m => m.Map(game)).Returns(gameEntity);

        _mockCallTrumpEngineer.Setup(e => e.Transform(callTrumpDecision)).Returns(new CallTrumpTrainingData());
        _mockDiscardCardEngineer.Setup(e => e.Transform(discardCardDecision)).Returns(new DiscardCardTrainingData());
        _mockPlayCardEngineer.Setup(e => e.Transform(playCardDecision)).Returns(new PlayCardTrainingData());

        _converter.Convert(games);

        _mockCallTrumpEngineer.Verify(e => e.Transform(callTrumpDecision), Times.Once);
        _mockDiscardCardEngineer.Verify(e => e.Transform(discardCardDecision), Times.Once);
        _mockPlayCardEngineer.Verify(e => e.Transform(playCardDecision), Times.Once);
    }

    [Fact]
    public void Convert_WithMultipleDealsAndDecisions_ProcessesAll()
    {
        var game = CreateGameWithPlayers();
        var games = new List<Game> { game };

        var callTrumpDecision1 = new CallTrumpDecisionEntity { RelativeDealPoints = 2 };
        var callTrumpDecision2 = new CallTrumpDecisionEntity { RelativeDealPoints = 3 };
        var discardCardDecision1 = new DiscardCardDecisionEntity { RelativeDealPoints = 1 };
        var discardCardDecision2 = new DiscardCardDecisionEntity { RelativeDealPoints = 2 };
        var playCardDecision1 = new PlayCardDecisionEntity { RelativeDealPoints = 1 };
        var playCardDecision2 = new PlayCardDecisionEntity { RelativeDealPoints = 2 };

        var gameEntity = new GameEntity
        {
            Deals =
            [
                new DealEntity
                {
                    CallTrumpDecisions = [callTrumpDecision1],
                    DiscardCardDecisions = [discardCardDecision1],
                    PlayCardDecisions = [playCardDecision1],
                },
                new DealEntity
                {
                    CallTrumpDecisions = [callTrumpDecision2],
                    DiscardCardDecisions = [discardCardDecision2],
                    PlayCardDecisions = [playCardDecision2],
                },
            ],
        };

        _mockMapper.Setup(m => m.Map(game)).Returns(gameEntity);

        _mockCallTrumpEngineer.Setup(e => e.Transform(It.IsAny<CallTrumpDecisionEntity>())).Returns(new CallTrumpTrainingData());
        _mockDiscardCardEngineer.Setup(e => e.Transform(It.IsAny<DiscardCardDecisionEntity>())).Returns(new DiscardCardTrainingData());
        _mockPlayCardEngineer.Setup(e => e.Transform(It.IsAny<PlayCardDecisionEntity>())).Returns(new PlayCardTrainingData());

        var result = _converter.Convert(games);

        result.CallTrumpData.Should().HaveCount(2);
        result.DiscardCardData.Should().HaveCount(2);
        result.PlayCardData.Should().HaveCount(2);
    }

    [Fact]
    public void Convert_WithSuccessfulConversion_ReturnsTrainingData()
    {
        var game = CreateGameWithPlayers();
        var games = new List<Game> { game };

        var gameEntity = new GameEntity
        {
            Deals =
            [
                new DealEntity
                {
                    CallTrumpDecisions = [new CallTrumpDecisionEntity { RelativeDealPoints = 2 }],
                },
            ],
        };

        _mockMapper.Setup(m => m.Map(game)).Returns(gameEntity);
        _mockCallTrumpEngineer.Setup(e => e.Transform(It.IsAny<CallTrumpDecisionEntity>())).Returns(new CallTrumpTrainingData());

        var result = _converter.Convert(games);

        result.CallTrumpData.Should().ContainSingle();
    }

    [Fact]
    public void Convert_ReturnsStats_WithCorrectGameCount()
    {
        var game1 = CreateGameWithPlayers();
        var game2 = CreateGameWithPlayers();
        var games = new List<Game> { game1, game2 };

        _mockMapper.Setup(m => m.Map(It.IsAny<Game>())).Returns(new GameEntity { Deals = [] });

        var result = _converter.Convert(games);

        result.Stats.GameCount.Should().Be(2);
    }

    [Fact]
    public void Convert_ReturnsStats_WithCorrectDealAndTrickCounts()
    {
        var game = CreateGameWithPlayers();
        game.CompletedDeals.Add(new Deal
        {
            CompletedTricks = [new Trick(), new Trick(), new Trick()],
        });
        game.CompletedDeals.Add(new Deal
        {
            CompletedTricks = [new Trick(), new Trick()],
        });
        var games = new List<Game> { game };

        _mockMapper.Setup(m => m.Map(It.IsAny<Game>())).Returns(new GameEntity { Deals = [] });

        var result = _converter.Convert(games);

        result.Stats.DealCount.Should().Be(2);
        result.Stats.TrickCount.Should().Be(5);
    }

    [Fact]
    public void Convert_ReturnsStats_WithUniqueActors()
    {
        var actor1 = new Actor(ActorType.Chaos);
        var actor2 = Actor.WithModel(ActorType.Model, "gen1", 0.1f);

        var game1 = new Game { GameStatus = GameStatus.Complete };
        game1.Players[PlayerPosition.South] = new Player { Actor = actor1 };
        game1.Players[PlayerPosition.North] = new Player { Actor = actor1 };
        game1.Players[PlayerPosition.East] = new Player { Actor = actor2 };
        game1.Players[PlayerPosition.West] = new Player { Actor = actor2 };

        var game2 = new Game { GameStatus = GameStatus.Complete };
        game2.Players[PlayerPosition.South] = new Player { Actor = actor1 };
        game2.Players[PlayerPosition.North] = new Player { Actor = actor2 };
        game2.Players[PlayerPosition.East] = new Player { Actor = actor1 };
        game2.Players[PlayerPosition.West] = new Player { Actor = actor2 };

        var games = new List<Game> { game1, game2 };

        _mockMapper.Setup(m => m.Map(It.IsAny<Game>())).Returns(new GameEntity { Deals = [] });

        var result = _converter.Convert(games);

        result.Stats.Actors.Should().HaveCount(2);
        result.Stats.Actors.Should().Contain(actor1);
        result.Stats.Actors.Should().Contain(actor2);
    }

    [Fact]
    public void Convert_WithEmptyGameList_ReturnsZeroStats()
    {
        var games = new List<Game>();

        var result = _converter.Convert(games);

        result.Stats.GameCount.Should().Be(0);
        result.Stats.DealCount.Should().Be(0);
        result.Stats.TrickCount.Should().Be(0);
        result.Stats.Actors.Should().BeEmpty();
    }

    private static Game CreateGameWithPlayers()
    {
        var game = new Game { GameStatus = GameStatus.Complete };
        game.Players[PlayerPosition.South] = new Player { Actor = new Actor(ActorType.Chaos) };
        game.Players[PlayerPosition.North] = new Player { Actor = new Actor(ActorType.Chaos) };
        game.Players[PlayerPosition.East] = new Player { Actor = new Actor(ActorType.Chaos) };
        game.Players[PlayerPosition.West] = new Player { Actor = new Actor(ActorType.Chaos) };
        return game;
    }
}
