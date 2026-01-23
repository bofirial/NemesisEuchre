using FluentAssertions;

using Moq;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Tests;

public class TrumpSelectionOrchestratorTests
{
    private readonly Mock<IPlayerActor> _playerActorMock;
    private readonly TrumpSelectionOrchestrator _sut;

    public TrumpSelectionOrchestratorTests()
    {
        _playerActorMock = new Mock<IPlayerActor>();
        _playerActorMock.Setup(b => b.ActorType).Returns(ActorType.Chaos);

        var bots = new[] { _playerActorMock.Object };
        _sut = new TrumpSelectionOrchestrator(bots);
    }

    [Fact]
    public Task SelectTrumpAsync_WithNullDeal_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.SelectTrumpAsync(null!);

        return act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("deal");
    }

    [Theory]
    [InlineData(DealStatus.NotStarted)]
    [InlineData(DealStatus.Playing)]
    [InlineData(DealStatus.Scoring)]
    [InlineData(DealStatus.Complete)]
    public Task SelectTrumpAsync_WithWrongDealStatus_ThrowsInvalidOperationException(DealStatus status)
    {
        var deal = CreateTestDeal();
        deal.DealStatus = status;

        var act = async () => await _sut.SelectTrumpAsync(deal);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Deal must be in SelectingTrump status, but was {status}");
    }

    [Fact]
    public Task SelectTrumpAsync_WithNullDealerPosition_ThrowsInvalidOperationException()
    {
        var deal = CreateTestDeal();
        deal.DealerPosition = null;

        var act = async () => await _sut.SelectTrumpAsync(deal);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("DealerPosition must be set");
    }

    [Fact]
    public Task SelectTrumpAsync_WithNullUpCard_ThrowsInvalidOperationException()
    {
        var deal = CreateTestDeal();
        deal.UpCard = null;

        var act = async () => await _sut.SelectTrumpAsync(deal);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("UpCard must be set");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public Task SelectTrumpAsync_WithWrongPlayerCount_ThrowsInvalidOperationException(int playerCount)
    {
        var deal = CreateTestDeal();
        deal.Players.Clear();
        for (int i = 0; i < playerCount; i++)
        {
            deal.Players[(PlayerPosition)i] = CreateTestPlayer((PlayerPosition)i);
        }

        var act = async () => await _sut.SelectTrumpAsync(deal);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Deal must have exactly 4 players, but had {playerCount}");
    }

    [Fact]
    public async Task SelectTrumpAsync_Round1FirstPlayerOrdersUp_SetsTrumpToUpcardSuit()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .ReturnsAsync(CallTrumpDecision.OrderItUp);

        SetupDealerDiscardMock();

        await _sut.SelectTrumpAsync(deal);

        deal.Trump.Should().Be(Suit.Hearts);
        deal.CallingPlayer.Should().Be(PlayerPosition.East);
        deal.CallingPlayerIsGoingAlone.Should().BeFalse();
    }

    [Fact]
    public async Task SelectTrumpAsync_Round1SecondPlayerOrdersUp_SetsTrumpToUpcardSuit()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Clubs, Rank = Rank.Jack };
        deal.DealerPosition = PlayerPosition.West;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.OrderItUp);

        SetupDealerDiscardMock();

        await _sut.SelectTrumpAsync(deal);

        deal.Trump.Should().Be(Suit.Clubs);
        deal.CallingPlayer.Should().Be(PlayerPosition.East);
        deal.CallingPlayerIsGoingAlone.Should().BeFalse();
    }

    [Fact]
    public async Task SelectTrumpAsync_Round1DealerOrdersUp_SetsTrumpToUpcardSuit()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Spades, Rank = Rank.Ace };
        deal.DealerPosition = PlayerPosition.South;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.OrderItUp);

        SetupDealerDiscardMock();

        await _sut.SelectTrumpAsync(deal);

        deal.Trump.Should().Be(Suit.Spades);
        deal.CallingPlayer.Should().Be(PlayerPosition.South);
        deal.CallingPlayerIsGoingAlone.Should().BeFalse();
    }

    [Fact]
    public async Task SelectTrumpAsync_Round1PlayerOrdersUpAndGoesAlone_SetsGoingAloneFlag()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Diamonds, Rank = Rank.King };
        deal.DealerPosition = PlayerPosition.East;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .ReturnsAsync(CallTrumpDecision.OrderItUpAndGoAlone);

        SetupDealerDiscardMock();

        await _sut.SelectTrumpAsync(deal);

        deal.Trump.Should().Be(Suit.Diamonds);
        deal.CallingPlayer.Should().Be(PlayerPosition.South);
        deal.CallingPlayerIsGoingAlone.Should().BeTrue();
    }

    [Fact]
    public async Task SelectTrumpAsync_Round1AllPass_ProceedsToRound2()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.CallClubs);

        await _sut.SelectTrumpAsync(deal);

        deal.Trump.Should().Be(Suit.Clubs);
        deal.CallingPlayer.Should().Be(PlayerPosition.East);
    }

    [Fact]
    public async Task SelectTrumpAsync_Round2FirstPlayerCalls_SetsTrumpToCalledSuit()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.CallSpades);

        await _sut.SelectTrumpAsync(deal);

        deal.Trump.Should().Be(Suit.Spades);
        deal.CallingPlayer.Should().Be(PlayerPosition.East);
        deal.CallingPlayerIsGoingAlone.Should().BeFalse();
    }

    [Fact]
    public async Task SelectTrumpAsync_Round2DealerMustCall_SetsTrump()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.CallDiamonds);

        await _sut.SelectTrumpAsync(deal);

        deal.Trump.Should().Be(Suit.Diamonds);
        deal.CallingPlayer.Should().Be(PlayerPosition.North);
        deal.CallingPlayerIsGoingAlone.Should().BeFalse();
    }

    [Fact]
    public async Task SelectTrumpAsync_Round2PlayerCallsAndGoesAlone_SetsGoingAloneFlag()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Clubs, Rank = Rank.Ten };
        deal.DealerPosition = PlayerPosition.West;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.CallHeartsAndGoAlone);

        await _sut.SelectTrumpAsync(deal);

        deal.Trump.Should().Be(Suit.Hearts);
        deal.CallingPlayer.Should().Be(PlayerPosition.East);
        deal.CallingPlayerIsGoingAlone.Should().BeTrue();
    }

    [Fact]
    public async Task SelectTrumpAsync_Round2ValidDecisions_NonDealerCanPass()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        CallTrumpDecision[]? capturedValidDecisions = null;
        var callCount = 0;

        _playerActorMock.Setup(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .Callback<Card[], Card, RelativePlayerPosition, short, short, CallTrumpDecision[]>(
                (_, _, _, _, _, validDecisions) =>
                {
                    callCount++;
                    if (callCount == 5)
                    {
                        capturedValidDecisions = validDecisions;
                    }
                })
            .ReturnsAsync((Card[] _, Card _, RelativePlayerPosition _, short _, short _, CallTrumpDecision[] _) =>
                callCount <= 4 ? CallTrumpDecision.Pass : CallTrumpDecision.CallClubs);

        await _sut.SelectTrumpAsync(deal);

        capturedValidDecisions.Should().Contain(CallTrumpDecision.Pass);
    }

    [Fact]
    public async Task SelectTrumpAsync_Round2ValidDecisions_UpcardSuitNotIncluded()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        var allCapturedDecisions = new List<CallTrumpDecision[]>();

        _playerActorMock.Setup(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .Callback<Card[], Card, RelativePlayerPosition, short, short, CallTrumpDecision[]>(
                (_, _, _, _, _, validDecisions) => allCapturedDecisions.Add(validDecisions))
            .ReturnsAsync(CallTrumpDecision.Pass);

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.CallClubs);

        await _sut.SelectTrumpAsync(deal);

        foreach (var decisions in allCapturedDecisions.Skip(4).ToList())
        {
            decisions.Should().NotContain(CallTrumpDecision.CallHearts);
            decisions.Should().NotContain(CallTrumpDecision.CallHeartsAndGoAlone);
        }
    }

    [Fact]
    public async Task SelectTrumpAsync_DealerDiscard_AddsUpcardToHand()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;
        var dealer = deal.Players[PlayerPosition.North];
        dealer.CurrentHand =
        [
            new() { Suit = Suit.Spades, Rank = Rank.Ten },
            new() { Suit = Suit.Clubs, Rank = Rank.Jack },
            new() { Suit = Suit.Hearts, Rank = Rank.King },
            new() { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new() { Suit = Suit.Spades, Rank = Rank.Ace },
        ];

        var handSizeBeforeDiscard = 0;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .ReturnsAsync(CallTrumpDecision.OrderItUp);

        _playerActorMock.Setup(b => b.DiscardCardAsync(
                It.IsAny<RelativeCard[]>(),
                It.IsAny<RelativeDeal?>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<RelativeCard[]>()))
            .Callback<RelativeCard[], RelativeDeal?, short, short, RelativeCard[]>(
                (hand, _, _, _, _) => handSizeBeforeDiscard = hand.Length)
            .ReturnsAsync((RelativeCard[] _, RelativeDeal? _, short _, short _, RelativeCard[] hand) => hand[0]);

        await _sut.SelectTrumpAsync(deal);

        handSizeBeforeDiscard.Should().Be(6);
    }

    [Fact]
    public async Task SelectTrumpAsync_DealerDiscard_RemovesCorrectCard()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;
        var dealer = deal.Players[PlayerPosition.North];
        dealer.CurrentHand =
        [
            new() { Suit = Suit.Spades, Rank = Rank.Ten },
            new() { Suit = Suit.Clubs, Rank = Rank.Jack },
            new() { Suit = Suit.Hearts, Rank = Rank.King },
            new() { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new() { Suit = Suit.Spades, Rank = Rank.Ace },
        ];

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .ReturnsAsync(CallTrumpDecision.OrderItUp);

        _playerActorMock.Setup(b => b.DiscardCardAsync(
                It.IsAny<RelativeCard[]>(),
                It.IsAny<RelativeDeal?>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<RelativeCard[]>()))
            .ReturnsAsync((RelativeCard[] _, RelativeDeal? _, short _, short _, RelativeCard[] hand) => hand[0]);

        await _sut.SelectTrumpAsync(deal);

        dealer.CurrentHand.Should().HaveCount(5);
        dealer.CurrentHand.Should().NotContain(c => c.Rank == Rank.Ten && c.Suit == Suit.Spades);
    }

    [Fact]
    public async Task SelectTrumpAsync_DealerDiscard_HandReturnsToFiveCards()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;
        var dealer = deal.Players[PlayerPosition.North];
        dealer.CurrentHand =
        [
            new() { Suit = Suit.Spades, Rank = Rank.Ten },
            new() { Suit = Suit.Clubs, Rank = Rank.Jack },
            new() { Suit = Suit.Hearts, Rank = Rank.King },
            new() { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new() { Suit = Suit.Spades, Rank = Rank.Ace },
        ];

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .ReturnsAsync(CallTrumpDecision.OrderItUp);

        SetupDealerDiscardMock();

        await _sut.SelectTrumpAsync(deal);

        dealer.CurrentHand.Should().HaveCount(5);
    }

    [Fact]
    public async Task SelectTrumpAsync_PlayerIterationOrder_StartsLeftOfDealer()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.West;

        var playerPositions = new List<PlayerPosition>();
        var callCount = 0;

        _playerActorMock.Setup(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .Callback<Card[], Card, RelativePlayerPosition, short, short, CallTrumpDecision[]>(
                (hand, _, _, _, _, _) =>
                {
                    var player = deal.Players.First(p => p.Value.CurrentHand.SequenceEqual(hand)).Key;
                    playerPositions.Add(player);
                    callCount++;
                })
            .ReturnsAsync((Card[] _, Card _, RelativePlayerPosition _, short _, short _, CallTrumpDecision[] _) =>
                callCount <= 4 ? CallTrumpDecision.Pass : CallTrumpDecision.CallClubs);

        await _sut.SelectTrumpAsync(deal);

        playerPositions.Should().HaveCountGreaterThanOrEqualTo(4);
        playerPositions[0].Should().Be(PlayerPosition.North);
        playerPositions[1].Should().Be(PlayerPosition.East);
        playerPositions[2].Should().Be(PlayerPosition.South);
        playerPositions[3].Should().Be(PlayerPosition.West);
    }

    [Fact]
    public async Task SelectTrumpAsync_PlayerIterationOrder_IncludesAllFourPlayers()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.South;

        var playerPositions = new List<PlayerPosition>();
        var callCount = 0;

        _playerActorMock.Setup(b => b.CallTrumpAsync(
                It.IsAny<Card[]>(),
                It.IsAny<Card>(),
                It.IsAny<RelativePlayerPosition>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<CallTrumpDecision[]>()))
            .Callback<Card[], Card, RelativePlayerPosition, short, short, CallTrumpDecision[]>(
                (hand, _, _, _, _, _) =>
                {
                    var player = deal.Players.First(p => p.Value.CurrentHand.SequenceEqual(hand)).Key;
                    if (playerPositions.Count < 4)
                    {
                        playerPositions.Add(player);
                    }

                    callCount++;
                })
            .ReturnsAsync((Card[] _, Card _, RelativePlayerPosition _, short _, short _, CallTrumpDecision[] _) =>
                callCount <= 4 ? CallTrumpDecision.Pass : CallTrumpDecision.CallClubs);

        await _sut.SelectTrumpAsync(deal);

        playerPositions.Should().HaveCount(4);
        playerPositions.Should().Contain(PlayerPosition.North);
        playerPositions.Should().Contain(PlayerPosition.East);
        playerPositions.Should().Contain(PlayerPosition.South);
        playerPositions.Should().Contain(PlayerPosition.West);
    }

    private static Deal CreateTestDeal()
    {
        return new Deal
        {
            DealStatus = DealStatus.SelectingTrump,
            DealerPosition = PlayerPosition.North,
            UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
            Team1Score = 0,
            Team2Score = 0,
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                { PlayerPosition.North, CreateTestPlayer(PlayerPosition.North) },
                { PlayerPosition.East, CreateTestPlayer(PlayerPosition.East) },
                { PlayerPosition.South, CreateTestPlayer(PlayerPosition.South) },
                { PlayerPosition.West, CreateTestPlayer(PlayerPosition.West) },
            },
        };
    }

    private static DealPlayer CreateTestPlayer(PlayerPosition position)
    {
        return new DealPlayer
        {
            Position = position,
            ActorType = ActorType.Chaos,
            CurrentHand =
            [
                new() { Suit = Suit.Spades, Rank = Rank.Nine },
                new() { Suit = Suit.Clubs, Rank = Rank.Ten },
                new() { Suit = Suit.Hearts, Rank = Rank.Jack },
                new() { Suit = Suit.Diamonds, Rank = Rank.Queen },
                new() { Suit = Suit.Spades, Rank = Rank.King },
            ],
        };
    }

    private void SetupDealerDiscardMock()
    {
        _playerActorMock.Setup(b => b.DiscardCardAsync(
                It.IsAny<RelativeCard[]>(),
                It.IsAny<RelativeDeal?>(),
                It.IsAny<short>(),
                It.IsAny<short>(),
                It.IsAny<RelativeCard[]>()))
            .ReturnsAsync((RelativeCard[] hand, RelativeDeal? _, short _, short _, RelativeCard[] _) => hand[0]);
    }
}
