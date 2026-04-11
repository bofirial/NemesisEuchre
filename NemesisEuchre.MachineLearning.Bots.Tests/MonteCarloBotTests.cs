using FluentAssertions;

using Moq;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;
using NemesisEuchre.MachineLearning.Bots.Simulation;

using Xunit;

namespace NemesisEuchre.MachineLearning.Bots.Tests;

public class MonteCarloBotTests
{
    private readonly Mock<IPlayerActor> _innerBotMock = new();
    private readonly Mock<IDealSimulator> _simulatorMock = new();
    private readonly Mock<IHiddenCardDistributor> _distributorMock = new();
    private readonly Mock<IRandomNumberGenerator> _randomMock = new();

    public MonteCarloBotTests()
    {
        _randomMock.Setup(r => r.NextInt(It.IsAny<int>())).Returns(0);

        _distributorMock
            .Setup(d => d.DistributeHiddenCards(
                It.IsAny<Card[]>(),
                It.IsAny<PlayerPosition>(),
                It.IsAny<int>(),
                It.IsAny<Suit>(),
                It.IsAny<IReadOnlyList<PlayerSuitVoid>>(),
                It.IsAny<PlayerPosition?>(),
                It.IsAny<IRandomNumberGenerator>()))
            .Returns(new Dictionary<PlayerPosition, List<Card>>
            {
                [PlayerPosition.North] = [new(Suit.Hearts, Rank.Nine)],
                [PlayerPosition.East] = [new(Suit.Clubs, Rank.Nine)],
                [PlayerPosition.West] = [new(Suit.Diamonds, Rank.Nine)],
            });
    }

    [Fact]
    public async Task PlayCardAsync_ReturnsValidCard()
    {
        _simulatorMock
            .Setup(s => s.SimulateFromPlayCardAsync(
                It.IsAny<PlayCardContext>(),
                It.IsAny<Card>(),
                It.IsAny<Dictionary<PlayerPosition, List<Card>>>(),
                It.IsAny<IPlayerActor>()))
            .ReturnsAsync(1.0f);

        var bot = new MonteCarloBot([_innerBotMock.Object], _simulatorMock.Object, _distributorMock.Object, [_randomMock.Object], 2);

        var context = CreatePlayCardContext();
        var result = await bot.PlayCardAsync(context);

        result.ChosenCard.Should().BeOneOf(context.ValidCardsToPlay);
    }

    [Fact]
    public async Task PlayCardAsync_PopulatesDecisionPredictedPoints()
    {
        _simulatorMock
            .Setup(s => s.SimulateFromPlayCardAsync(
                It.IsAny<PlayCardContext>(),
                It.IsAny<Card>(),
                It.IsAny<Dictionary<PlayerPosition, List<Card>>>(),
                It.IsAny<IPlayerActor>()))
            .ReturnsAsync(1.0f);

        var bot = new MonteCarloBot([_innerBotMock.Object], _simulatorMock.Object, _distributorMock.Object, [_randomMock.Object], 2);

        var context = CreatePlayCardContext();
        var result = await bot.PlayCardAsync(context);

        result.DecisionPredictedPoints.Should().HaveCount(context.ValidCardsToPlay.Length);
    }

    [Fact]
    public async Task PlayCardAsync_PicksCardWithHighestAverageScore()
    {
        var highCard = new Card(Suit.Spades, Rank.Ace);
        var lowCard = new Card(Suit.Hearts, Rank.Nine);

        _simulatorMock
            .Setup(s => s.SimulateFromPlayCardAsync(
                It.IsAny<PlayCardContext>(),
                highCard,
                It.IsAny<Dictionary<PlayerPosition, List<Card>>>(),
                It.IsAny<IPlayerActor>()))
            .ReturnsAsync(2.0f);

        _simulatorMock
            .Setup(s => s.SimulateFromPlayCardAsync(
                It.IsAny<PlayCardContext>(),
                lowCard,
                It.IsAny<Dictionary<PlayerPosition, List<Card>>>(),
                It.IsAny<IPlayerActor>()))
            .ReturnsAsync(-1.0f);

        var bot = new MonteCarloBot([_innerBotMock.Object], _simulatorMock.Object, _distributorMock.Object, [_randomMock.Object], 3);

        var context = new PlayCardContext
        {
            CardsInHand = [highCard, lowCard],
            ValidCardsToPlay = [highCard, lowCard],
            PlayerPosition = PlayerPosition.South,
            TeamScore = 0,
            OpponentScore = 0,
            WonTricks = 0,
            OpponentsWonTricks = 0,
            TrumpSuit = Suit.Spades,
            CallingPlayer = PlayerPosition.South,
            CallingPlayerIsGoingAlone = false,
            Dealer = PlayerPosition.East,
            DealerPickedUpCard = null,
            LeadPlayer = PlayerPosition.South,
            LeadSuit = null,
            TrickNumber = 1,
            PlayedCardsInTrick = [],
            CurrentlyWinningTrickPlayer = null,
            KnownPlayerSuitVoids = [],
            CardsAccountedFor = [highCard, lowCard],
        };

        var result = await bot.PlayCardAsync(context);

        result.ChosenCard.Should().Be(highCard);
    }

    [Fact]
    public async Task CallTrumpAsync_SimulatesPassOption()
    {
        _simulatorMock
            .Setup(s => s.SimulateFromPassAsync(
                It.IsAny<CallTrumpContext>(),
                It.IsAny<Dictionary<PlayerPosition, List<Card>>>(),
                It.IsAny<IPlayerActor>()))
            .ReturnsAsync(0.5f);

        _simulatorMock
            .Setup(s => s.SimulateFromCallTrumpAsync(
                It.IsAny<CallTrumpContext>(),
                It.IsAny<CallTrumpDecision>(),
                It.IsAny<Dictionary<PlayerPosition, List<Card>>>(),
                It.IsAny<IPlayerActor>()))
            .ReturnsAsync(-1.0f);

        var bot = new MonteCarloBot([_innerBotMock.Object], _simulatorMock.Object, _distributorMock.Object, [_randomMock.Object], 2);

        var context = new CallTrumpContext
        {
            CardsInHand =
            [
                new(Suit.Hearts, Rank.Nine),
                new(Suit.Hearts, Rank.Ten),
                new(Suit.Clubs, Rank.Nine),
                new(Suit.Clubs, Rank.Ten),
                new(Suit.Diamonds, Rank.Nine),
            ],
            PlayerPosition = PlayerPosition.South,
            TeamScore = 0,
            OpponentScore = 0,
            DealerPosition = PlayerPosition.East,
            UpCard = new Card(Suit.Spades, Rank.Queen),
            ValidCallTrumpDecisions = [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp, CallTrumpDecision.OrderItUpAndGoAlone],
            DecisionNumber = 1,
        };

        var result = await bot.CallTrumpAsync(context);

        result.ChosenCallTrumpDecision.Should().Be(CallTrumpDecision.Pass, "pass simulation returns higher score than calling");
        _simulatorMock.Verify(
            s => s.SimulateFromPassAsync(
                It.IsAny<CallTrumpContext>(),
                It.IsAny<Dictionary<PlayerPosition, List<Card>>>(),
                It.IsAny<IPlayerActor>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task DiscardCardAsync_ReturnsValidDiscard()
    {
        _simulatorMock
            .Setup(s => s.SimulateFromDiscardAsync(
                It.IsAny<DiscardCardContext>(),
                It.IsAny<Card>(),
                It.IsAny<Dictionary<PlayerPosition, List<Card>>>(),
                It.IsAny<IPlayerActor>()))
            .ReturnsAsync(1.0f);

        var bot = new MonteCarloBot([_innerBotMock.Object], _simulatorMock.Object, _distributorMock.Object, [_randomMock.Object], 2);

        var hand = new Card[]
        {
            new(Suit.Spades, Rank.Ace),
            new(Suit.Spades, Rank.King),
            new(Suit.Hearts, Rank.Ace),
            new(Suit.Hearts, Rank.King),
            new(Suit.Clubs, Rank.Ace),
            new(Suit.Diamonds, Rank.Nine),
        };

        var context = new DiscardCardContext
        {
            CardsInHand = hand,
            PlayerPosition = PlayerPosition.South,
            TeamScore = 0,
            OpponentScore = 0,
            TrumpSuit = Suit.Spades,
            CallingPlayer = PlayerPosition.North,
            CallingPlayerGoingAlone = false,
            ValidCardsToDiscard = hand,
        };

        var result = await bot.DiscardCardAsync(context);

        result.ChosenCard.Should().BeOneOf(hand);
        result.DecisionPredictedPoints.Should().HaveCount(hand.Length);
    }

    [Fact]
    public void ActorType_IsMonteCarlo()
    {
        var bot = new MonteCarloBot([_innerBotMock.Object], _simulatorMock.Object, _distributorMock.Object, [_randomMock.Object], 50);

        bot.ActorType.Should().Be(ActorType.MonteCarlo);
    }

    [Fact]
    public async Task CallTrumpAsync_WhenSkipSimCallTrump_DelegatesToInnerBot()
    {
        var expectedResult = new CallTrumpDecisionContext
        {
            ChosenCallTrumpDecision = CallTrumpDecision.OrderItUp,
            DecisionPredictedPoints = new Dictionary<CallTrumpDecision, float>
            {
                [CallTrumpDecision.Pass] = -0.5f,
                [CallTrumpDecision.OrderItUp] = 1.0f,
            },
        };

        _innerBotMock
            .Setup(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(expectedResult);

        var bot = new MonteCarloBot(
            [_innerBotMock.Object],
            _simulatorMock.Object,
            _distributorMock.Object,
            [_randomMock.Object],
            50,
            skipSimCallTrump: true);

        var context = new CallTrumpContext
        {
            CardsInHand =
            [
                new(Suit.Hearts, Rank.Nine),
                new(Suit.Hearts, Rank.Ten),
                new(Suit.Clubs, Rank.Nine),
                new(Suit.Clubs, Rank.Ten),
                new(Suit.Diamonds, Rank.Nine),
            ],
            PlayerPosition = PlayerPosition.South,
            TeamScore = 0,
            OpponentScore = 0,
            DealerPosition = PlayerPosition.East,
            UpCard = new Card(Suit.Spades, Rank.Queen),
            ValidCallTrumpDecisions = [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp],
            DecisionNumber = 1,
        };

        var result = await bot.CallTrumpAsync(context);

        result.Should().BeSameAs(expectedResult);
        _innerBotMock.Verify(b => b.CallTrumpAsync(context), Times.Once);
        _simulatorMock.Verify(
            s => s.SimulateFromCallTrumpAsync(
                It.IsAny<CallTrumpContext>(),
                It.IsAny<CallTrumpDecision>(),
                It.IsAny<Dictionary<PlayerPosition, List<Card>>>(),
                It.IsAny<IPlayerActor>()),
            Times.Never);
    }

    [Fact]
    public async Task DiscardCardAsync_WhenSkipSimDiscard_DelegatesToInnerBot()
    {
        var hand = new Card[]
        {
            new(Suit.Spades, Rank.Ace),
            new(Suit.Spades, Rank.King),
            new(Suit.Hearts, Rank.Ace),
            new(Suit.Hearts, Rank.King),
            new(Suit.Clubs, Rank.Ace),
            new(Suit.Diamonds, Rank.Nine),
        };

        var expectedResult = new CardDecisionContext
        {
            ChosenCard = hand[5],
            DecisionPredictedPoints = new Dictionary<Card, float> { [hand[5]] = 1.0f },
        };

        _innerBotMock
            .Setup(b => b.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .ReturnsAsync(expectedResult);

        var bot = new MonteCarloBot(
            [_innerBotMock.Object],
            _simulatorMock.Object,
            _distributorMock.Object,
            [_randomMock.Object],
            50,
            skipSimDiscard: true);

        var context = new DiscardCardContext
        {
            CardsInHand = hand,
            PlayerPosition = PlayerPosition.South,
            TeamScore = 0,
            OpponentScore = 0,
            TrumpSuit = Suit.Spades,
            CallingPlayer = PlayerPosition.North,
            CallingPlayerGoingAlone = false,
            ValidCardsToDiscard = hand,
        };

        var result = await bot.DiscardCardAsync(context);

        result.Should().BeSameAs(expectedResult);
        _innerBotMock.Verify(b => b.DiscardCardAsync(context), Times.Once);
        _simulatorMock.Verify(
            s => s.SimulateFromDiscardAsync(
                It.IsAny<DiscardCardContext>(),
                It.IsAny<Card>(),
                It.IsAny<Dictionary<PlayerPosition, List<Card>>>(),
                It.IsAny<IPlayerActor>()),
            Times.Never);
    }

    [Fact]
    public async Task PlayCardAsync_WhenSkipSimPlayCard_DelegatesToInnerBot()
    {
        var expectedResult = new CardDecisionContext
        {
            ChosenCard = new Card(Suit.Spades, Rank.Ace),
            DecisionPredictedPoints = new Dictionary<Card, float>
            {
                [new Card(Suit.Spades, Rank.Ace)] = 2.0f,
            },
        };

        _innerBotMock
            .Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .ReturnsAsync(expectedResult);

        var bot = new MonteCarloBot(
            [_innerBotMock.Object],
            _simulatorMock.Object,
            _distributorMock.Object,
            [_randomMock.Object],
            50,
            skipSimPlayCard: true);

        var context = CreatePlayCardContext();
        var result = await bot.PlayCardAsync(context);

        result.Should().BeSameAs(expectedResult);
        _innerBotMock.Verify(b => b.PlayCardAsync(context), Times.Once);
        _simulatorMock.Verify(
            s => s.SimulateFromPlayCardAsync(
                It.IsAny<PlayCardContext>(),
                It.IsAny<Card>(),
                It.IsAny<Dictionary<PlayerPosition, List<Card>>>(),
                It.IsAny<IPlayerActor>()),
            Times.Never);
    }

    [Fact]
    public async Task PlayCardAsync_WhenOnlyCallTrumpSkipped_StillSimulatesPlayCard()
    {
        _simulatorMock
            .Setup(s => s.SimulateFromPlayCardAsync(
                It.IsAny<PlayCardContext>(),
                It.IsAny<Card>(),
                It.IsAny<Dictionary<PlayerPosition, List<Card>>>(),
                It.IsAny<IPlayerActor>()))
            .ReturnsAsync(1.0f);

        var bot = new MonteCarloBot(
            [_innerBotMock.Object],
            _simulatorMock.Object,
            _distributorMock.Object,
            [_randomMock.Object],
            2,
            skipSimCallTrump: true);

        var context = CreatePlayCardContext();
        var result = await bot.PlayCardAsync(context);

        result.ChosenCard.Should().BeOneOf(context.ValidCardsToPlay);
        _simulatorMock.Verify(
            s => s.SimulateFromPlayCardAsync(
                It.IsAny<PlayCardContext>(),
                It.IsAny<Card>(),
                It.IsAny<Dictionary<PlayerPosition, List<Card>>>(),
                It.IsAny<IPlayerActor>()),
            Times.AtLeastOnce);
        _innerBotMock.Verify(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()), Times.Never);
    }

    private static PlayCardContext CreatePlayCardContext()
    {
        var hand = new Card[]
        {
            new(Suit.Spades, Rank.Ace),
            new(Suit.Hearts, Rank.Ace),
        };

        return new PlayCardContext
        {
            CardsInHand = hand,
            ValidCardsToPlay = hand,
            PlayerPosition = PlayerPosition.South,
            TeamScore = 0,
            OpponentScore = 0,
            WonTricks = 0,
            OpponentsWonTricks = 0,
            TrumpSuit = Suit.Spades,
            CallingPlayer = PlayerPosition.South,
            CallingPlayerIsGoingAlone = false,
            Dealer = PlayerPosition.East,
            DealerPickedUpCard = null,
            LeadPlayer = PlayerPosition.South,
            LeadSuit = null,
            TrickNumber = 1,
            PlayedCardsInTrick = [],
            CurrentlyWinningTrickPlayer = null,
            KnownPlayerSuitVoids = [],
            CardsAccountedFor = hand,
        };
    }
}
