using FluentAssertions;

using Moq;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Handlers;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Services;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.GameEngine.Tests.Handlers;

public class DealerDiscardHandlerTests
{
    private readonly Mock<IPlayerActorResolver> _actorResolverMock = new();
    private readonly Mock<IPlayerContextBuilder> _contextBuilderMock = new();
    private readonly Mock<ITrumpSelectionValidator> _validatorMock = new();
    private readonly Mock<IPlayerActor> _playerActorMock = new();
    private readonly DealerDiscardHandler _handler;

    public DealerDiscardHandlerTests()
    {
        var cardAccountingService = new CardAccountingService();
        var decisionRecorder = new DecisionRecorder(_contextBuilderMock.Object, cardAccountingService);
        _handler = new DealerDiscardHandler(
            _actorResolverMock.Object,
            _contextBuilderMock.Object,
            _validatorMock.Object,
            decisionRecorder);

        _actorResolverMock.Setup(x => x.GetPlayerActor(It.IsAny<DealPlayer>()))
            .Returns(_playerActorMock.Object);

        _contextBuilderMock.Setup(x => x.GetScores(It.IsAny<Deal>(), It.IsAny<PlayerPosition>()))
            .Returns((10, 5));
    }

    [Fact]
    public async Task HandleDealerDiscardAsync_AddsUpcardToDealerHand()
    {
        var upCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var deal = CreateTestDeal(upCard);
        var dealer = deal.Players[PlayerPosition.North];
        var initialHandCount = dealer.CurrentHand.Count;

        _playerActorMock.Setup(x => x.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .ReturnsAsync(new CardDecisionContext { ChosenCard = upCard });

        await _handler.HandleDealerDiscardAsync(deal);

        dealer.CurrentHand.Count.Should().Be(initialHandCount);
    }

    [Fact]
    public async Task HandleDealerDiscardAsync_CallsPlayerActorWithCorrectParameters()
    {
        var upCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var deal = CreateTestDeal(upCard);

        _playerActorMock.Setup(x => x.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .ReturnsAsync(new CardDecisionContext { ChosenCard = upCard });

        await _handler.HandleDealerDiscardAsync(deal);

        _playerActorMock.Verify(
            x => x.DiscardCardAsync(
                It.Is<DiscardCardContext>(ctx =>
                    ctx.CardsInHand.Length == 6 &&
                    ctx.PlayerPosition == PlayerPosition.North &&
                    ctx.TeamScore == 10 &&
                    ctx.OpponentScore == 5 &&
                    ctx.TrumpSuit == Suit.Hearts &&

                    !ctx.CallingPlayerGoingAlone &&
                    ctx.ValidCardsToDiscard.Length == 6)),
            Times.Once);
    }

    [Fact]
    public async Task HandleDealerDiscardAsync_ValidatesDiscard()
    {
        var upCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var deal = CreateTestDeal(upCard);

        _playerActorMock.Setup(x => x.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .ReturnsAsync(new CardDecisionContext { ChosenCard = upCard });

        await _handler.HandleDealerDiscardAsync(deal);

        _validatorMock.Verify(
            x => x.ValidateDiscard(
                It.IsAny<Card>(),
                It.IsAny<Card[]>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleDealerDiscardAsync_RemovesDiscardedCardFromHand()
    {
        var upCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var deal = CreateTestDeal(upCard);
        var dealer = deal.Players[PlayerPosition.North];
        var initialHandCount = dealer.CurrentHand.Count;

        var cardToDiscard = dealer.CurrentHand[0];

        _playerActorMock.Setup(x => x.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .ReturnsAsync(new CardDecisionContext { ChosenCard = cardToDiscard });

        await _handler.HandleDealerDiscardAsync(deal);

        dealer.CurrentHand.Should().NotContain(cardToDiscard);
        dealer.CurrentHand.Count.Should().Be(initialHandCount);
    }

    [Fact]
    public async Task HandleDealerDiscardAsync_CapturesDiscardDecision()
    {
        var upCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var deal = CreateTestDeal(upCard);

        _playerActorMock.Setup(x => x.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .ReturnsAsync(new CardDecisionContext { ChosenCard = upCard });

        await _handler.HandleDealerDiscardAsync(deal);

        deal.DiscardCardDecisions.Should().HaveCount(1);
        deal.DiscardCardDecisions[0].ChosenCard.Should().Be(upCard);
    }

    [Fact]
    public async Task HandleDealerDiscardAsync_CapturesSixCardHand()
    {
        var upCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var deal = CreateTestDeal(upCard);
        var dealer = deal.Players[PlayerPosition.North];

        _playerActorMock.Setup(x => x.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .ReturnsAsync(new CardDecisionContext { ChosenCard = upCard });

        await _handler.HandleDealerDiscardAsync(deal);

        var record = deal.DiscardCardDecisions[0];
        record.CardsInHand.Should().HaveCount(6);
        record.CardsInHand.Should().Contain(upCard);
        dealer.CurrentHand.Should().HaveCount(5);
    }

    [Fact]
    public async Task HandleDealerDiscardAsync_CapturesScoresFromContextBuilder()
    {
        var upCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var deal = CreateTestDeal(upCard);

        _contextBuilderMock.Setup(x => x.GetScores(deal, PlayerPosition.North))
            .Returns((7, 5));

        _playerActorMock.Setup(x => x.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .ReturnsAsync(new CardDecisionContext { ChosenCard = upCard });

        await _handler.HandleDealerDiscardAsync(deal);

        var record = deal.DiscardCardDecisions[0];
        record.TeamScore.Should().Be(7);
        record.OpponentScore.Should().Be(5);
    }

    [Fact]
    public async Task HandleDealerDiscardAsync_ValidCardsToDiscardMatchesHand()
    {
        var upCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var deal = CreateTestDeal(upCard);

        _playerActorMock.Setup(x => x.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .ReturnsAsync(new CardDecisionContext { ChosenCard = upCard });

        await _handler.HandleDealerDiscardAsync(deal);

        var record = deal.DiscardCardDecisions[0];
        record.ValidCardsToDiscard.Should().HaveCount(6);
        record.ValidCardsToDiscard.Should().BeEquivalentTo(record.CardsInHand);
    }

    [Fact]
    public async Task HandleDealerDiscardAsync_CapturesTrumpAndCallingPlayerInfo()
    {
        var upCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var deal = CreateTestDeal(upCard);
        deal.Trump = Suit.Spades;
        deal.CallingPlayer = PlayerPosition.East;
        deal.CallingPlayerIsGoingAlone = true;

        _playerActorMock.Setup(x => x.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .ReturnsAsync(new CardDecisionContext { ChosenCard = upCard });

        await _handler.HandleDealerDiscardAsync(deal);

        var record = deal.DiscardCardDecisions[0];
        record.TrumpSuit.Should().Be(Suit.Spades);
        record.CallingPlayer.Should().Be(PlayerPosition.East);
        record.CallingPlayerGoingAlone.Should().BeTrue();
    }

    [Fact]
    public async Task HandleDealerDiscardAsync_CapturesDealerPosition()
    {
        var upCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var dealSouth = new Deal
        {
            DealerPosition = PlayerPosition.South,
            UpCard = upCard,
            Trump = Suit.Hearts,
            CallingPlayer = PlayerPosition.South,
            CallingPlayerIsGoingAlone = false,
        };

        var southPlayer = new DealPlayer
        {
            ActorType = ActorType.Chaos,
            CurrentHand =
            [
                new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
                new Card { Suit = Suit.Hearts, Rank = Rank.Ten },
                new Card { Suit = Suit.Clubs, Rank = Rank.Jack },
                new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
                new Card { Suit = Suit.Spades, Rank = Rank.King },
            ],
        };

        dealSouth.Players.Add(PlayerPosition.South, southPlayer);

        _contextBuilderMock.Setup(x => x.GetScores(dealSouth, PlayerPosition.South))
            .Returns((10, 5));

        _playerActorMock.Setup(x => x.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .ReturnsAsync(new CardDecisionContext { ChosenCard = upCard });

        await _handler.HandleDealerDiscardAsync(dealSouth);

        var record = dealSouth.DiscardCardDecisions[0];
        record.PlayerPosition.Should().Be(PlayerPosition.South);
    }

    [Fact]
    public async Task HandleDealerDiscardAsync_CapturesAndDiscardsCorrectly()
    {
        var upCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var deal = CreateTestDeal(upCard);
        var dealer = deal.Players[PlayerPosition.North];
        var cardToDiscard = dealer.CurrentHand[0];

        _playerActorMock.Setup(x => x.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .ReturnsAsync(new CardDecisionContext { ChosenCard = cardToDiscard });

        await _handler.HandleDealerDiscardAsync(deal);

        deal.DiscardCardDecisions.Should().HaveCount(1);
        var record = deal.DiscardCardDecisions[0];
        record.CardsInHand.Should().HaveCount(6);
        record.ChosenCard.Should().Be(cardToDiscard);
        dealer.CurrentHand.Should().NotContain(cardToDiscard);
        dealer.CurrentHand.Should().HaveCount(5);
    }

    [Fact]
    public async Task HandleDealerDiscardAsync_SetsDiscardedCardOnDeal()
    {
        var upCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var deal = CreateTestDeal(upCard);
        var dealer = deal.Players[PlayerPosition.North];
        var cardToDiscard = dealer.CurrentHand[0];

        _playerActorMock.Setup(x => x.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .ReturnsAsync(new CardDecisionContext { ChosenCard = cardToDiscard });

        await _handler.HandleDealerDiscardAsync(deal);

        deal.DiscardedCard.Should().NotBeNull();
        deal.DiscardedCard.Should().Be(cardToDiscard);
    }

    private static Deal CreateTestDeal(Card upCard)
    {
        var deal = new Deal
        {
            DealerPosition = PlayerPosition.North,
            UpCard = upCard,
            Trump = Suit.Hearts,
            CallingPlayer = PlayerPosition.North,
            CallingPlayerIsGoingAlone = false,
        };

        var northPlayer = new DealPlayer
        {
            ActorType = ActorType.Chaos,
            CurrentHand =
            [
                new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
                new Card { Suit = Suit.Hearts, Rank = Rank.Ten },
                new Card { Suit = Suit.Clubs, Rank = Rank.Jack },
                new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
                new Card { Suit = Suit.Spades, Rank = Rank.King },
            ],
        };

        deal.Players.Add(PlayerPosition.North, northPlayer);

        return deal;
    }
}
