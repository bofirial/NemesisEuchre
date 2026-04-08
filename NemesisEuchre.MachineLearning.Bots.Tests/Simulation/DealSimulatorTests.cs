using FluentAssertions;

using Moq;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Bots.Simulation;

using Xunit;

namespace NemesisEuchre.MachineLearning.Bots.Tests.Simulation;

public class DealSimulatorTests
{
    private readonly DealSimulator _simulator = new();
    private readonly Mock<IPlayerActor> _innerBotMock = new();

    public DealSimulatorTests()
    {
        _innerBotMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .Returns((PlayCardContext ctx) => Task.FromResult(new CardDecisionContext
            {
                ChosenCard = ctx.ValidCardsToPlay[0],
                DecisionPredictedPoints = [],
            }));

        _innerBotMock.Setup(b => b.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .Returns((DiscardCardContext ctx) => Task.FromResult(new CardDecisionContext
            {
                ChosenCard = ctx.ValidCardsToDiscard[0],
                DecisionPredictedPoints = [],
            }));

        _innerBotMock.Setup(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .Returns((CallTrumpContext ctx) => Task.FromResult(new CallTrumpDecisionContext
            {
                ChosenCallTrumpDecision = ctx.ValidCallTrumpDecisions[0],
                DecisionPredictedPoints = [],
            }));
    }

    [Fact]
    public async Task SimulateFromPlayCardAsync_CompletesAllTricks()
    {
        var context = CreatePlayCardContext(trickNumber: 1);
        var hands = CreateSimulatedHands(5);

        var score = await _simulator.SimulateFromPlayCardAsync(
            context,
            context.ValidCardsToPlay[0],
            hands,
            _innerBotMock.Object);

        score.Should().NotBe(0f, "a completed deal should produce a non-zero score (win or loss)");
    }

    [Fact]
    public async Task SimulateFromPlayCardAsync_ReturnsPositiveScoreForWinningTeam()
    {
        var southHand = new Card[]
        {
            new(Suit.Spades, Rank.Jack),
            new(Suit.Clubs, Rank.Jack),
            new(Suit.Spades, Rank.Ace),
            new(Suit.Spades, Rank.King),
            new(Suit.Spades, Rank.Queen),
        };

        var context = new PlayCardContext
        {
            CardsInHand = southHand,
            ValidCardsToPlay = southHand,
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
            CardsAccountedFor = southHand,
        };

        var hands = new Dictionary<PlayerPosition, List<Card>>
        {
            [PlayerPosition.North] =
            [
                new(Suit.Hearts, Rank.Nine),
                new(Suit.Hearts, Rank.Ten),
                new(Suit.Diamonds, Rank.Nine),
                new(Suit.Diamonds, Rank.Ten),
                new(Suit.Clubs, Rank.Nine),
            ],
            [PlayerPosition.East] =
            [
                new(Suit.Hearts, Rank.Jack),
                new(Suit.Hearts, Rank.Queen),
                new(Suit.Diamonds, Rank.Jack),
                new(Suit.Diamonds, Rank.Queen),
                new(Suit.Clubs, Rank.Ten),
            ],
            [PlayerPosition.West] =
            [
                new(Suit.Hearts, Rank.King),
                new(Suit.Hearts, Rank.Ace),
                new(Suit.Diamonds, Rank.King),
                new(Suit.Diamonds, Rank.Ace),
                new(Suit.Clubs, Rank.Queen),
            ],
        };

        var score = await _simulator.SimulateFromPlayCardAsync(
            context,
            southHand[0],
            hands,
            _innerBotMock.Object);

        score.Should().BePositive("South's team called trump with all top trump cards");
    }

    [Fact]
    public async Task SimulateFromDiscardAsync_CompletesAllTricks()
    {
        var context = new DiscardCardContext
        {
            CardsInHand =
            [
                new(Suit.Spades, Rank.Ace),
                new(Suit.Spades, Rank.King),
                new(Suit.Hearts, Rank.Ace),
                new(Suit.Hearts, Rank.King),
                new(Suit.Clubs, Rank.Ace),
                new(Suit.Diamonds, Rank.Nine),
            ],
            PlayerPosition = PlayerPosition.South,
            TeamScore = 0,
            OpponentScore = 0,
            TrumpSuit = Suit.Spades,
            CallingPlayer = PlayerPosition.North,
            CallingPlayerGoingAlone = false,
            ValidCardsToDiscard =
            [
                new(Suit.Spades, Rank.Ace),
                new(Suit.Spades, Rank.King),
                new(Suit.Hearts, Rank.Ace),
                new(Suit.Hearts, Rank.King),
                new(Suit.Clubs, Rank.Ace),
                new(Suit.Diamonds, Rank.Nine),
            ],
        };

        var hands = CreateSimulatedHands(5);

        var score = await _simulator.SimulateFromDiscardAsync(
            context,
            new Card(Suit.Diamonds, Rank.Nine),
            hands,
            _innerBotMock.Object);

        score.Should().NotBe(float.NaN);
    }

    [Fact]
    public async Task SimulateFromCallTrumpAsync_CompletesWithOrderItUp()
    {
        var context = new CallTrumpContext
        {
            CardsInHand =
            [
                new(Suit.Spades, Rank.Ace),
                new(Suit.Spades, Rank.King),
                new(Suit.Hearts, Rank.Ace),
                new(Suit.Hearts, Rank.King),
                new(Suit.Clubs, Rank.Ace),
            ],
            PlayerPosition = PlayerPosition.South,
            TeamScore = 0,
            OpponentScore = 0,
            DealerPosition = PlayerPosition.East,
            UpCard = new Card(Suit.Spades, Rank.Queen),
            ValidCallTrumpDecisions = [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp, CallTrumpDecision.OrderItUpAndGoAlone],
            DecisionNumber = 1,
        };

        var hands = CreateSimulatedHands(5);

        var score = await _simulator.SimulateFromCallTrumpAsync(
            context,
            CallTrumpDecision.OrderItUp,
            hands,
            _innerBotMock.Object);

        score.Should().NotBe(float.NaN);
    }

    [Fact]
    public async Task SimulateFromPassAsync_ReturnsZeroOnThrowIn()
    {
        _innerBotMock.Setup(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(new CallTrumpDecisionContext
            {
                ChosenCallTrumpDecision = CallTrumpDecision.Pass,
                DecisionPredictedPoints = [],
            });

        var context = new CallTrumpContext
        {
            CardsInHand =
            [
                new(Suit.Spades, Rank.Nine),
                new(Suit.Hearts, Rank.Nine),
                new(Suit.Clubs, Rank.Nine),
                new(Suit.Diamonds, Rank.Nine),
                new(Suit.Spades, Rank.Ten),
            ],
            PlayerPosition = PlayerPosition.South,
            TeamScore = 0,
            OpponentScore = 0,
            DealerPosition = PlayerPosition.East,
            UpCard = new Card(Suit.Spades, Rank.Queen),
            ValidCallTrumpDecisions = [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp, CallTrumpDecision.OrderItUpAndGoAlone],
            DecisionNumber = 1,
        };

        var hands = CreateSimulatedHands(5);

        var score = await _simulator.SimulateFromPassAsync(
            context,
            hands,
            _innerBotMock.Object);

        score.Should().Be(0f, "all players passed so it should be a throw-in");
    }

    [Fact]
    public async Task SimulateFromPlayCardAsync_HandlesGoingAlone()
    {
        var context = CreatePlayCardContext(trickNumber: 1, goingAlone: true);
        var hands = CreateSimulatedHands(5);

        var score = await _simulator.SimulateFromPlayCardAsync(
            context,
            context.ValidCardsToPlay[0],
            hands,
            _innerBotMock.Object);

        score.Should().NotBe(float.NaN);
    }

    private static PlayCardContext CreatePlayCardContext(short trickNumber = 1, bool goingAlone = false)
    {
        var hand = new Card[]
        {
            new(Suit.Spades, Rank.Ace),
            new(Suit.Hearts, Rank.Ace),
            new(Suit.Clubs, Rank.Ace),
            new(Suit.Diamonds, Rank.Ace),
            new(Suit.Spades, Rank.King),
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
            CallingPlayerIsGoingAlone = goingAlone,
            Dealer = PlayerPosition.East,
            DealerPickedUpCard = null,
            LeadPlayer = PlayerPosition.South,
            LeadSuit = null,
            TrickNumber = trickNumber,
            PlayedCardsInTrick = [],
            CurrentlyWinningTrickPlayer = null,
            KnownPlayerSuitVoids = [],
            CardsAccountedFor = hand,
        };
    }

    private static Dictionary<PlayerPosition, List<Card>> CreateSimulatedHands(int cardsPerPlayer)
    {
        var allCards = new List<Card>();
        foreach (var suit in Enum.GetValues<Suit>())
        {
            allCards.Add(new Card(suit, Rank.Nine));
            allCards.Add(new Card(suit, Rank.Ten));
            allCards.Add(new Card(suit, Rank.Jack));
            allCards.Add(new Card(suit, Rank.Queen));
            allCards.Add(new Card(suit, Rank.King));
            allCards.Add(new Card(suit, Rank.Ace));
        }

        var usedCards = new HashSet<Card>
        {
            new(Suit.Spades, Rank.Ace),
            new(Suit.Hearts, Rank.Ace),
            new(Suit.Clubs, Rank.Ace),
            new(Suit.Diamonds, Rank.Ace),
            new(Suit.Spades, Rank.King),
        };

        var available = allCards.Where(c => !usedCards.Contains(c)).ToList();

        return new Dictionary<PlayerPosition, List<Card>>
        {
            [PlayerPosition.North] = [.. available.Take(cardsPerPlayer)],
            [PlayerPosition.East] = [.. available.Skip(cardsPerPlayer).Take(cardsPerPlayer)],
            [PlayerPosition.West] = [.. available.Skip(cardsPerPlayer * 2).Take(cardsPerPlayer)],
        };
    }
}
