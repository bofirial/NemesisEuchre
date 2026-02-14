using FluentAssertions;

using Moq;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Services;

namespace NemesisEuchre.GameEngine.Tests.Services;

public class DecisionRecorderTests
{
    private readonly Mock<IPlayerContextBuilder> _mockContextBuilder = new();
    private readonly Mock<ICardAccountingService> _mockCardAccountingService = new();
    private readonly DecisionRecorder _recorder;

    public DecisionRecorderTests()
    {
        _recorder = new DecisionRecorder(_mockContextBuilder.Object, _mockCardAccountingService.Object);
    }

    [Fact]
    public void RecordCallTrumpDecision_AddsRecordToDealCallTrumpDecisions()
    {
        var deal = CreateDealWithPlayers();
        _mockContextBuilder.Setup(x => x.GetScores(deal, PlayerPosition.North)).Returns((5, 3));
        var validDecisions = new[] { CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp };
        var context = new CallTrumpDecisionContext
        {
            ChosenCallTrumpDecision = CallTrumpDecision.Pass,
            DecisionPredictedPoints = new Dictionary<CallTrumpDecision, float>
            {
                { CallTrumpDecision.Pass, 1.5f },
                { CallTrumpDecision.OrderItUp, 0.8f },
            },
        };
        byte counter = 0;

        var recordingContext = new CallTrumpRecordingContext(
            Deal: deal,
            PlayerPosition: PlayerPosition.North,
            ValidDecisions: validDecisions,
            CallTrumpDecisionContext: context);
        _recorder.RecordCallTrumpDecision(recordingContext, ref counter);

        deal.CallTrumpDecisions.Should().HaveCount(1);
    }

    [Fact]
    public void RecordCallTrumpDecision_SetsCorrectPlayerPosition()
    {
        var deal = CreateDealWithPlayers();
        _mockContextBuilder.Setup(x => x.GetScores(deal, PlayerPosition.East)).Returns((3, 5));
        var validDecisions = new[] { CallTrumpDecision.Pass };
        var context = new CallTrumpDecisionContext
        {
            ChosenCallTrumpDecision = CallTrumpDecision.Pass,
        };
        byte counter = 0;

        var recordingContext = new CallTrumpRecordingContext(
            Deal: deal,
            PlayerPosition: PlayerPosition.East,
            ValidDecisions: validDecisions,
            CallTrumpDecisionContext: context);
        _recorder.RecordCallTrumpDecision(recordingContext, ref counter);

        deal.CallTrumpDecisions[0].PlayerPosition.Should().Be(PlayerPosition.East);
    }

    [Fact]
    public void RecordCallTrumpDecision_SetsCorrectScores()
    {
        var deal = CreateDealWithPlayers();
        _mockContextBuilder.Setup(x => x.GetScores(deal, PlayerPosition.North)).Returns((7, 2));
        var validDecisions = new[] { CallTrumpDecision.Pass };
        var context = new CallTrumpDecisionContext { ChosenCallTrumpDecision = CallTrumpDecision.Pass };
        byte counter = 0;

        var recordingContext = new CallTrumpRecordingContext(
            Deal: deal,
            PlayerPosition: PlayerPosition.North,
            ValidDecisions: validDecisions,
            CallTrumpDecisionContext: context);
        _recorder.RecordCallTrumpDecision(recordingContext, ref counter);

        deal.CallTrumpDecisions[0].TeamScore.Should().Be(7);
        deal.CallTrumpDecisions[0].OpponentScore.Should().Be(2);
    }

    [Fact]
    public void RecordCallTrumpDecision_IncrementsDecisionOrderCounter()
    {
        var deal = CreateDealWithPlayers();
        _mockContextBuilder.Setup(x => x.GetScores(It.IsAny<Deal>(), It.IsAny<PlayerPosition>())).Returns((0, 0));
        var validDecisions = new[] { CallTrumpDecision.Pass };
        var context = new CallTrumpDecisionContext { ChosenCallTrumpDecision = CallTrumpDecision.Pass };
        byte counter = 2;

        var recordingContext = new CallTrumpRecordingContext(
            Deal: deal,
            PlayerPosition: PlayerPosition.North,
            ValidDecisions: validDecisions,
            CallTrumpDecisionContext: context);
        _recorder.RecordCallTrumpDecision(recordingContext, ref counter);

        counter.Should().Be(3);
        deal.CallTrumpDecisions[0].DecisionOrder.Should().Be(3);
    }

    [Fact]
    public void RecordCallTrumpDecision_CopiesCardsFromCurrentHand()
    {
        var deal = CreateDealWithPlayers();
        deal.Players[PlayerPosition.North].CurrentHand =
        [
            new Card(Suit.Hearts, Rank.Ace),
            new Card(Suit.Spades, Rank.King),
        ];
        _mockContextBuilder.Setup(x => x.GetScores(deal, PlayerPosition.North)).Returns((0, 0));
        var context = new CallTrumpDecisionContext { ChosenCallTrumpDecision = CallTrumpDecision.Pass };
        byte counter = 0;

        var recordingContext = new CallTrumpRecordingContext(
            Deal: deal,
            PlayerPosition: PlayerPosition.North,
            ValidDecisions: [],
            CallTrumpDecisionContext: context);
        _recorder.RecordCallTrumpDecision(recordingContext, ref counter);

        deal.CallTrumpDecisions[0].CardsInHand.Should().HaveCount(2);
    }

    [Fact]
    public void RecordCallTrumpDecision_SetsUpCardFromDeal()
    {
        var deal = CreateDealWithPlayers();
        deal.UpCard = new Card(Suit.Diamonds, Rank.Jack);
        _mockContextBuilder.Setup(x => x.GetScores(deal, PlayerPosition.North)).Returns((0, 0));
        var context = new CallTrumpDecisionContext { ChosenCallTrumpDecision = CallTrumpDecision.Pass };
        byte counter = 0;

        var recordingContext = new CallTrumpRecordingContext(
            Deal: deal,
            PlayerPosition: PlayerPosition.North,
            ValidDecisions: [],
            CallTrumpDecisionContext: context);
        _recorder.RecordCallTrumpDecision(recordingContext, ref counter);

        deal.CallTrumpDecisions[0].UpCard.Should().Be(new Card(Suit.Diamonds, Rank.Jack));
    }

    [Fact]
    public void RecordCallTrumpDecision_SetsChosenDecision()
    {
        var deal = CreateDealWithPlayers();
        _mockContextBuilder.Setup(x => x.GetScores(deal, PlayerPosition.North)).Returns((0, 0));
        var validDecisions = new[] { CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp };
        var context = new CallTrumpDecisionContext
        {
            ChosenCallTrumpDecision = CallTrumpDecision.OrderItUp,
        };
        byte counter = 0;

        var recordingContext = new CallTrumpRecordingContext(
            Deal: deal,
            PlayerPosition: PlayerPosition.North,
            ValidDecisions: validDecisions,
            CallTrumpDecisionContext: context);
        _recorder.RecordCallTrumpDecision(recordingContext, ref counter);

        deal.CallTrumpDecisions[0].ChosenDecision.Should().Be(CallTrumpDecision.OrderItUp);
    }

    [Fact]
    public void RecordPlayCardDecision_AddsRecordToTrickPlayCardDecisions()
    {
        var deal = CreateDealForPlayCard();
        var trick = new Trick { TrickNumber = 1, LeadPosition = PlayerPosition.North };
        var hand = new[] { new Card(Suit.Hearts, Rank.Ace) };
        var validCards = new[] { new Card(Suit.Hearts, Rank.Ace) };
        var cardContext = new CardDecisionContext
        {
            ChosenCard = new Card(Suit.Hearts, Rank.Ace),
        };
        _mockContextBuilder.Setup(x => x.GetScores(deal, PlayerPosition.North)).Returns((0, 0));
        _mockCardAccountingService.Setup(x => x.GetAccountedForCards(deal, trick, PlayerPosition.North, hand)).Returns([]);
        var mockCalculator = new Mock<ITrickWinnerCalculator>();

        var recordingContext = new PlayCardRecordingContext(
            Deal: deal,
            Trick: trick,
            PlayerPosition: PlayerPosition.North,
            Hand: hand,
            ValidCards: validCards,
            CardDecisionContext: cardContext,
            TrickWinnerCalculator: mockCalculator.Object);
        _recorder.RecordPlayCardDecision(recordingContext);

        trick.PlayCardDecisions.Should().HaveCount(1);
    }

    [Fact]
    public void RecordPlayCardDecision_SetsCorrectTrumpSuit()
    {
        var deal = CreateDealForPlayCard();
        deal.Trump = Suit.Spades;
        var trick = new Trick { TrickNumber = 1, LeadPosition = PlayerPosition.North };
        var hand = new[] { new Card(Suit.Spades, Rank.Ace) };
        var cardContext = new CardDecisionContext { ChosenCard = new Card(Suit.Spades, Rank.Ace) };
        _mockContextBuilder.Setup(x => x.GetScores(deal, PlayerPosition.North)).Returns((0, 0));
        _mockCardAccountingService.Setup(x => x.GetAccountedForCards(deal, trick, PlayerPosition.North, hand)).Returns([]);

        var recordingContext = new PlayCardRecordingContext(
            Deal: deal,
            Trick: trick,
            PlayerPosition: PlayerPosition.North,
            Hand: hand,
            ValidCards: hand,
            CardDecisionContext: cardContext,
            TrickWinnerCalculator: new Mock<ITrickWinnerCalculator>().Object);
        _recorder.RecordPlayCardDecision(recordingContext);

        trick.PlayCardDecisions[0].TrumpSuit.Should().Be(Suit.Spades);
    }

    [Fact]
    public void RecordPlayCardDecision_SetsLeadSuitFromTrick()
    {
        var deal = CreateDealForPlayCard();
        var trick = new Trick { TrickNumber = 1, LeadPosition = PlayerPosition.North, LeadSuit = Suit.Hearts };
        var hand = new[] { new Card(Suit.Hearts, Rank.Ace) };
        var cardContext = new CardDecisionContext { ChosenCard = new Card(Suit.Hearts, Rank.Ace) };
        _mockContextBuilder.Setup(x => x.GetScores(deal, PlayerPosition.North)).Returns((0, 0));
        _mockCardAccountingService.Setup(x => x.GetAccountedForCards(deal, trick, PlayerPosition.North, hand)).Returns([]);

        var recordingContext = new PlayCardRecordingContext(
            Deal: deal,
            Trick: trick,
            PlayerPosition: PlayerPosition.North,
            Hand: hand,
            ValidCards: hand,
            CardDecisionContext: cardContext,
            TrickWinnerCalculator: new Mock<ITrickWinnerCalculator>().Object);
        _recorder.RecordPlayCardDecision(recordingContext);

        trick.PlayCardDecisions[0].LeadSuit.Should().Be(Suit.Hearts);
    }

    [Fact]
    public void RecordPlayCardDecision_CalculatesWinningTrickPlayer_WhenCardsPlayed()
    {
        var deal = CreateDealForPlayCard();
        var trick = new Trick { TrickNumber = 1, LeadPosition = PlayerPosition.North, LeadSuit = Suit.Hearts };
        trick.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.King), PlayerPosition.North));
        var hand = new[] { new Card(Suit.Hearts, Rank.Ace) };
        var cardContext = new CardDecisionContext { ChosenCard = new Card(Suit.Hearts, Rank.Ace) };
        _mockContextBuilder.Setup(x => x.GetScores(deal, PlayerPosition.East)).Returns((0, 0));
        _mockCardAccountingService.Setup(x => x.GetAccountedForCards(deal, trick, PlayerPosition.East, hand)).Returns([]);
        var mockCalculator = new Mock<ITrickWinnerCalculator>();
        mockCalculator.Setup(x => x.CalculateWinner(trick, Suit.Clubs)).Returns(PlayerPosition.North);

        var recordingContext = new PlayCardRecordingContext(
            Deal: deal,
            Trick: trick,
            PlayerPosition: PlayerPosition.East,
            Hand: hand,
            ValidCards: hand,
            CardDecisionContext: cardContext,
            TrickWinnerCalculator: mockCalculator.Object);
        _recorder.RecordPlayCardDecision(recordingContext);

        trick.PlayCardDecisions[0].WinningTrickPlayer.Should().Be(PlayerPosition.North);
    }

    [Fact]
    public void RecordPlayCardDecision_WinningTrickPlayerIsNull_WhenNoCardsPlayed()
    {
        var deal = CreateDealForPlayCard();
        var trick = new Trick { TrickNumber = 1, LeadPosition = PlayerPosition.North };
        var hand = new[] { new Card(Suit.Hearts, Rank.Ace) };
        var cardContext = new CardDecisionContext { ChosenCard = new Card(Suit.Hearts, Rank.Ace) };
        _mockContextBuilder.Setup(x => x.GetScores(deal, PlayerPosition.North)).Returns((0, 0));
        _mockCardAccountingService.Setup(x => x.GetAccountedForCards(deal, trick, PlayerPosition.North, hand)).Returns([]);

        var recordingContext = new PlayCardRecordingContext(
            Deal: deal,
            Trick: trick,
            PlayerPosition: PlayerPosition.North,
            Hand: hand,
            ValidCards: hand,
            CardDecisionContext: cardContext,
            TrickWinnerCalculator: new Mock<ITrickWinnerCalculator>().Object);
        _recorder.RecordPlayCardDecision(recordingContext);

        trick.PlayCardDecisions[0].WinningTrickPlayer.Should().BeNull();
    }

    [Fact]
    public void RecordDiscardDecision_AddsRecordToDealDiscardDecisions()
    {
        var deal = CreateDealForPlayCard();
        deal.CallingPlayer = PlayerPosition.North;
        var hand = new[] { new Card(Suit.Hearts, Rank.Ace), new Card(Suit.Spades, Rank.King) };
        var cardContext = new CardDecisionContext { ChosenCard = new Card(Suit.Hearts, Rank.Ace) };
        _mockContextBuilder.Setup(x => x.GetScores(deal, PlayerPosition.North)).Returns((0, 0));

        var recordingContext = new DiscardCardRecordingContext(
            Deal: deal,
            PlayerPosition: PlayerPosition.North,
            Hand: hand,
            CardDecisionContext: cardContext);
        _recorder.RecordDiscardDecision(recordingContext);

        deal.DiscardCardDecisions.Should().HaveCount(1);
    }

    [Fact]
    public void RecordDiscardDecision_SetsChosenCard()
    {
        var deal = CreateDealForPlayCard();
        deal.CallingPlayer = PlayerPosition.North;
        var chosenCard = new Card(Suit.Spades, Rank.King);
        var hand = new[] { new Card(Suit.Hearts, Rank.Ace), chosenCard };
        var cardContext = new CardDecisionContext { ChosenCard = chosenCard };
        _mockContextBuilder.Setup(x => x.GetScores(deal, PlayerPosition.North)).Returns((0, 0));

        var recordingContext = new DiscardCardRecordingContext(
            Deal: deal,
            PlayerPosition: PlayerPosition.North,
            Hand: hand,
            CardDecisionContext: cardContext);
        _recorder.RecordDiscardDecision(recordingContext);

        deal.DiscardCardDecisions[0].ChosenCard.Should().Be(chosenCard);
    }

    [Fact]
    public void RecordDiscardDecision_SetsValidCardsToDiscardFromHand()
    {
        var deal = CreateDealForPlayCard();
        deal.CallingPlayer = PlayerPosition.North;
        var hand = new[] { new Card(Suit.Hearts, Rank.Ace), new Card(Suit.Spades, Rank.King) };
        var cardContext = new CardDecisionContext { ChosenCard = hand[0] };
        _mockContextBuilder.Setup(x => x.GetScores(deal, PlayerPosition.North)).Returns((0, 0));

        var recordingContext = new DiscardCardRecordingContext(
            Deal: deal,
            PlayerPosition: PlayerPosition.North,
            Hand: hand,
            CardDecisionContext: cardContext);
        _recorder.RecordDiscardDecision(recordingContext);

        deal.DiscardCardDecisions[0].ValidCardsToDiscard.Should().HaveCount(2);
    }

    private static Deal CreateDealWithPlayers()
    {
        return new Deal
        {
            DealerPosition = PlayerPosition.North,
            UpCard = new Card(Suit.Hearts, Rank.Nine),
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                { PlayerPosition.North, new DealPlayer { Position = PlayerPosition.North, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
                { PlayerPosition.East, new DealPlayer { Position = PlayerPosition.East, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
                { PlayerPosition.South, new DealPlayer { Position = PlayerPosition.South, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
                { PlayerPosition.West, new DealPlayer { Position = PlayerPosition.West, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
            },
        };
    }

    private static Deal CreateDealForPlayCard()
    {
        return new Deal
        {
            DealerPosition = PlayerPosition.North,
            Trump = Suit.Clubs,
            CallingPlayer = PlayerPosition.East,
            UpCard = new Card(Suit.Clubs, Rank.Nine),
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                { PlayerPosition.North, new DealPlayer { Position = PlayerPosition.North, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
                { PlayerPosition.East, new DealPlayer { Position = PlayerPosition.East, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
                { PlayerPosition.South, new DealPlayer { Position = PlayerPosition.South, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
                { PlayerPosition.West, new DealPlayer { Position = PlayerPosition.West, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
            },
        };
    }
}
