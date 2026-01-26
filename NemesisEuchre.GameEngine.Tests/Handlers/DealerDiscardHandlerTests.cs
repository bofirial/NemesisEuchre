using FluentAssertions;

using Moq;

using NemesisEuchre.GameEngine.Constants;
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
        _handler = new DealerDiscardHandler(
            _actorResolverMock.Object,
            _contextBuilderMock.Object,
            _validatorMock.Object);

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

        _playerActorMock.Setup(x => x.DiscardCardAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Deal>(),
                It.IsAny<PlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<Card[]>()))
            .ReturnsAsync(upCard);

        await _handler.HandleDealerDiscardAsync(deal);

        dealer.CurrentHand.Count.Should().Be(initialHandCount);
    }

    [Fact]
    public async Task HandleDealerDiscardAsync_CallsPlayerActorWithCorrectParameters()
    {
        var upCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var deal = CreateTestDeal(upCard);

        _playerActorMock.Setup(x => x.DiscardCardAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Deal>(),
                It.IsAny<PlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<Card[]>()))
            .ReturnsAsync(upCard);

        await _handler.HandleDealerDiscardAsync(deal);

        _playerActorMock.Verify(
            x => x.DiscardCardAsync(
                It.Is<Card[]>(cards => cards.Length == 6),
                It.IsAny<Deal>(),
                PlayerPosition.North,
                10,
                5,
                It.Is<Card[]>(cards => cards.Length == 6)),
            Times.Once);
    }

    [Fact]
    public async Task HandleDealerDiscardAsync_ValidatesDiscard()
    {
        var upCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var deal = CreateTestDeal(upCard);

        _playerActorMock.Setup(x => x.DiscardCardAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Deal>(),
                It.IsAny<PlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<Card[]>()))
            .ReturnsAsync(upCard);

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

        _playerActorMock.Setup(x => x.DiscardCardAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Deal>(),
                It.IsAny<PlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<Card[]>()))
            .ReturnsAsync(cardToDiscard);

        await _handler.HandleDealerDiscardAsync(deal);

        dealer.CurrentHand.Should().NotContain(cardToDiscard);
        dealer.CurrentHand.Count.Should().Be(initialHandCount);
    }

    private static Deal CreateTestDeal(Card upCard)
    {
        var deal = new Deal
        {
            DealerPosition = PlayerPosition.North,
            UpCard = upCard,
            Trump = Suit.Hearts,
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
