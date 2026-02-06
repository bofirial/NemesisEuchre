using FluentAssertions;

using Moq;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Handlers;
using NemesisEuchre.GameEngine.Mappers;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Options;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Services;
using NemesisEuchre.GameEngine.Validation;

using MsOptions = Microsoft.Extensions.Options;

namespace NemesisEuchre.GameEngine.Tests;

public class TrumpSelectionOrchestratorTests
{
    private readonly Mock<IPlayerActor> _playerActorMock;
    private readonly Mock<IPlayerActorResolver> _actorResolverMock;
    private readonly MsOptions.IOptions<GameOptions> _gameOptions;
    private readonly TrumpSelectionOrchestrator _sut;

    public TrumpSelectionOrchestratorTests()
    {
        _playerActorMock = new Mock<IPlayerActor>();
        _playerActorMock.Setup(b => b.ActorType).Returns(ActorType.Chaos);
        _gameOptions = MsOptions.Options.Create(new GameOptions { StickTheDealer = true });

        _actorResolverMock = new Mock<IPlayerActorResolver>();
        _actorResolverMock.Setup(x => x.GetPlayerActor(It.IsAny<DealPlayer>()))
            .Returns(_playerActorMock.Object);

        var validator = new TrumpSelectionValidator();
        var decisionMapper = new CallTrumpDecisionMapper();
        var contextBuilder = new PlayerContextBuilder();
        var cardAccountingService = new CardAccountingService();
        var decisionRecorder = new DecisionRecorder(contextBuilder, cardAccountingService);
        var dealerDiscardHandler = new DealerDiscardHandler(_actorResolverMock.Object, contextBuilder, validator, decisionRecorder);

        _sut = new TrumpSelectionOrchestrator(
            _gameOptions,
            validator,
            decisionMapper,
            dealerDiscardHandler,
            contextBuilder,
            _actorResolverMock.Object,
            decisionRecorder);
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

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(CallTrumpDecision.OrderItUp);

        SetupDealerDiscardMock();

        await _sut.SelectTrumpAsync(deal);

        deal.Trump.Should().Be(Suit.Hearts);
        deal.CallingPlayer.Should().Be(PlayerPosition.East);
        deal.CallingPlayerIsGoingAlone.Should().BeFalse();
        deal.ChosenDecision.Should().Be(CallTrumpDecision.OrderItUp);
    }

    [Fact]
    public async Task SelectTrumpAsync_Round1SecondPlayerOrdersUp_SetsTrumpToUpcardSuit()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Clubs, Rank = Rank.Jack };
        deal.DealerPosition = PlayerPosition.West;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
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

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
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

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(CallTrumpDecision.OrderItUpAndGoAlone);

        SetupDealerDiscardMock();

        await _sut.SelectTrumpAsync(deal);

        deal.Trump.Should().Be(Suit.Diamonds);
        deal.CallingPlayer.Should().Be(PlayerPosition.South);
        deal.CallingPlayerIsGoingAlone.Should().BeTrue();
        deal.ChosenDecision.Should().Be(CallTrumpDecision.OrderItUpAndGoAlone);
    }

    [Fact]
    public async Task SelectTrumpAsync_Round1AllPass_ProceedsToRound2()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
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

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.CallSpades);

        await _sut.SelectTrumpAsync(deal);

        deal.Trump.Should().Be(Suit.Spades);
        deal.CallingPlayer.Should().Be(PlayerPosition.East);
        deal.CallingPlayerIsGoingAlone.Should().BeFalse();
        deal.ChosenDecision.Should().Be(CallTrumpDecision.CallSpades);
    }

    [Fact]
    public async Task SelectTrumpAsync_Round2DealerMustCall_SetsTrump()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
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

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
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
        deal.ChosenDecision.Should().Be(CallTrumpDecision.CallHeartsAndGoAlone);
    }

    [Fact]
    public async Task SelectTrumpAsync_Round2ValidDecisions_NonDealerCanPass()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        CallTrumpDecision[]? capturedValidDecisions = null;
        var callCount = 0;

        _playerActorMock.Setup(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .Callback<CallTrumpContext>(ctx =>
                {
                    callCount++;
                    if (callCount == 5)
                    {
                        capturedValidDecisions = ctx.ValidCallTrumpDecisions;
                    }
                })
            .ReturnsAsync((CallTrumpContext _) => callCount <= 4 ? CallTrumpDecision.Pass : CallTrumpDecision.CallClubs);

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

        _playerActorMock.Setup(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .Callback<CallTrumpContext>(ctx => allCapturedDecisions.Add(ctx.ValidCallTrumpDecisions))
            .ReturnsAsync(CallTrumpDecision.Pass);

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
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

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(CallTrumpDecision.OrderItUp);

        _playerActorMock.Setup(b => b.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .Callback<DiscardCardContext>(ctx => handSizeBeforeDiscard = ctx.CardsInHand.Length)
            .ReturnsAsync((DiscardCardContext ctx) => ctx.ValidCardsToDiscard[0]);

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

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(CallTrumpDecision.OrderItUp);

        _playerActorMock.Setup(b => b.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .ReturnsAsync((DiscardCardContext ctx) => ctx.ValidCardsToDiscard[0]);

        await _sut.SelectTrumpAsync(deal);

        dealer.CurrentHand.Should().HaveCount(5);
        dealer.CurrentHand.Should().NotContain(c => c.Rank == Rank.King && c.Suit == Suit.Hearts);
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

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
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

        _playerActorMock.Setup(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .Callback<CallTrumpContext>((ctx) =>
                {
                    var player = deal.Players.First(p => p.Value.CurrentHand.SequenceEqual(ctx.CardsInHand)).Key;
                    playerPositions.Add(player);
                    callCount++;
                })
            .ReturnsAsync((CallTrumpContext _) => callCount <= 4 ? CallTrumpDecision.Pass : CallTrumpDecision.CallClubs);

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

        _playerActorMock.Setup(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .Callback<CallTrumpContext>((ctx) =>
                {
                    var player = deal.Players.First(p => p.Value.CurrentHand.SequenceEqual(ctx.CardsInHand)).Key;
                    if (playerPositions.Count < 4)
                    {
                        playerPositions.Add(player);
                    }

                    callCount++;
                })
            .ReturnsAsync((CallTrumpContext _) => callCount <= 4 ? CallTrumpDecision.Pass : CallTrumpDecision.CallClubs);

        await _sut.SelectTrumpAsync(deal);

        playerPositions.Should().HaveCount(4);
        playerPositions.Should().Contain(PlayerPosition.North);
        playerPositions.Should().Contain(PlayerPosition.East);
        playerPositions.Should().Contain(PlayerPosition.South);
        playerPositions.Should().Contain(PlayerPosition.West);
    }

    [Fact]
    public async Task SelectTrumpAsync_WithStickTheDealerFalse_DealerCanPass()
    {
        var gameOptions = MsOptions.Options.Create(new GameOptions { StickTheDealer = false });
        var actorResolverMock = new Mock<IPlayerActorResolver>();
        actorResolverMock.Setup(x => x.GetPlayerActor(It.IsAny<DealPlayer>()))
            .Returns(_playerActorMock.Object);

        var validator = new TrumpSelectionValidator();
        var decisionMapper = new CallTrumpDecisionMapper();
        var contextBuilder = new PlayerContextBuilder();
        var cardAccountingService = new CardAccountingService();
        var decisionRecorder = new DecisionRecorder(contextBuilder, cardAccountingService);
        var dealerDiscardHandler = new DealerDiscardHandler(actorResolverMock.Object, contextBuilder, validator, decisionRecorder);

        var sut = new TrumpSelectionOrchestrator(
            gameOptions,
            validator,
            decisionMapper,
            dealerDiscardHandler,
            contextBuilder,
            actorResolverMock.Object,
            decisionRecorder);

        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        _playerActorMock.Setup(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(CallTrumpDecision.Pass);

        await sut.SelectTrumpAsync(deal);

        deal.Trump.Should().BeNull();
        deal.CallingPlayer.Should().BeNull();
        deal.ChosenDecision.Should().BeNull();
    }

    [Fact]
    public async Task SelectTrumpAsync_WithStickTheDealerFalse_DealerPassOptionIncludedInRound2()
    {
        var gameOptions = MsOptions.Options.Create(new GameOptions { StickTheDealer = false });
        var actorResolverMock = new Mock<IPlayerActorResolver>();
        actorResolverMock.Setup(x => x.GetPlayerActor(It.IsAny<DealPlayer>()))
            .Returns(_playerActorMock.Object);

        var validator = new TrumpSelectionValidator();
        var decisionMapper = new CallTrumpDecisionMapper();
        var contextBuilder = new PlayerContextBuilder();
        var cardAccountingService = new CardAccountingService();
        var decisionRecorder = new DecisionRecorder(contextBuilder, cardAccountingService);
        var dealerDiscardHandler = new DealerDiscardHandler(actorResolverMock.Object, contextBuilder, validator, decisionRecorder);

        var sut = new TrumpSelectionOrchestrator(
            gameOptions,
            validator,
            decisionMapper,
            dealerDiscardHandler,
            contextBuilder,
            actorResolverMock.Object,
            decisionRecorder);

        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        CallTrumpDecision[]? capturedValidDecisions = null;
        var callCount = 0;

        _playerActorMock.Setup(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .Callback<CallTrumpContext>(ctx =>
                {
                    callCount++;
                    if (callCount == 8)
                    {
                        capturedValidDecisions = ctx.ValidCallTrumpDecisions;
                    }
                })
            .ReturnsAsync(CallTrumpDecision.Pass);

        await sut.SelectTrumpAsync(deal);

        capturedValidDecisions.Should().Contain(CallTrumpDecision.Pass);
    }

    [Fact]
    public async Task SelectTrumpAsync_WithStickTheDealerTrue_DealerCannotPassInRound2()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        CallTrumpDecision[]? capturedValidDecisions = null;
        var callCount = 0;

        _playerActorMock.Setup(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .Callback<CallTrumpContext>(ctx =>
                {
                    callCount++;
                    if (callCount == 8)
                    {
                        capturedValidDecisions = ctx.ValidCallTrumpDecisions;
                    }
                })
            .ReturnsAsync((CallTrumpContext _) => callCount <= 7 ? CallTrumpDecision.Pass : CallTrumpDecision.CallClubs);

        await _sut.SelectTrumpAsync(deal);

        capturedValidDecisions.Should().NotContain(CallTrumpDecision.Pass);
    }

    [Fact]
    public async Task SelectTrumpAsync_Round1FirstPlayerCalls_CapturesOneDecision()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(CallTrumpDecision.OrderItUp);

        SetupDealerDiscardMock();

        await _sut.SelectTrumpAsync(deal);

        deal.CallTrumpDecisions.Should().HaveCount(1);
        deal.CallTrumpDecisions[0].ChosenDecision.Should().Be(CallTrumpDecision.OrderItUp);
        deal.CallTrumpDecisions[0].DecisionOrder.Should().Be(1);
    }

    [Fact]
    public async Task SelectTrumpAsync_Round1AllPlayersDecide_CapturesFourDecisions()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.OrderItUp);

        SetupDealerDiscardMock();

        await _sut.SelectTrumpAsync(deal);

        deal.CallTrumpDecisions.Should().HaveCount(4);
        deal.CallTrumpDecisions[0].ChosenDecision.Should().Be(CallTrumpDecision.Pass);
        deal.CallTrumpDecisions[1].ChosenDecision.Should().Be(CallTrumpDecision.Pass);
        deal.CallTrumpDecisions[2].ChosenDecision.Should().Be(CallTrumpDecision.Pass);
        deal.CallTrumpDecisions[3].ChosenDecision.Should().Be(CallTrumpDecision.OrderItUp);
    }

    [Fact]
    public async Task SelectTrumpAsync_BothRounds_CapturesAllEightDecisions()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.CallClubs);

        await _sut.SelectTrumpAsync(deal);

        deal.CallTrumpDecisions.Should().HaveCount(8);
        deal.CallTrumpDecisions.Take(7).Select(d => d.ChosenDecision)
            .Should().AllBeEquivalentTo(CallTrumpDecision.Pass);
        deal.CallTrumpDecisions[7].ChosenDecision.Should().Be(CallTrumpDecision.CallClubs);
    }

    [Fact]
    public async Task SelectTrumpAsync_CapturesDecisionOrder_SequentialFrom1To8()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.CallClubs);

        await _sut.SelectTrumpAsync(deal);

        for (byte i = 1; i <= 8; i++)
        {
            deal.CallTrumpDecisions[i - 1].DecisionOrder.Should().Be(i);
        }
    }

    [Fact]
    public async Task SelectTrumpAsync_CapturesPlayerContext_HandAndUpCard()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;
        var firstPlayer = deal.Players[PlayerPosition.East];

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(CallTrumpDecision.OrderItUp);

        SetupDealerDiscardMock();

        await _sut.SelectTrumpAsync(deal);

        var record = deal.CallTrumpDecisions[0];
        record.CardsInHand.Should().BeEquivalentTo(firstPlayer.CurrentHand);
        record.UpCard.Should().BeEquivalentTo(deal.UpCard);
    }

    [Fact]
    public async Task SelectTrumpAsync_CapturesPlayerPositions_DealerAndDecidingPlayer()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.OrderItUp);

        SetupDealerDiscardMock();

        await _sut.SelectTrumpAsync(deal);

        deal.CallTrumpDecisions.Should().HaveCount(2);
        deal.CallTrumpDecisions[0].DealerPosition.Should().Be(PlayerPosition.North);
        deal.CallTrumpDecisions[0].PlayerPosition.Should().Be(PlayerPosition.East);
        deal.CallTrumpDecisions[1].DealerPosition.Should().Be(PlayerPosition.North);
        deal.CallTrumpDecisions[1].PlayerPosition.Should().Be(PlayerPosition.South);
    }

    [Fact]
    public async Task SelectTrumpAsync_CapturesScores_TeamAndOpponent()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;
        deal.Team1Score = 5;
        deal.Team2Score = 3;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(CallTrumpDecision.OrderItUp);

        SetupDealerDiscardMock();

        await _sut.SelectTrumpAsync(deal);

        var record = deal.CallTrumpDecisions[0];
        record.TeamScore.Should().Be(3);
        record.OpponentScore.Should().Be(5);
    }

    [Fact]
    public async Task SelectTrumpAsync_CapturesValidDecisions_Round1AndRound2()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.CallClubs);

        await _sut.SelectTrumpAsync(deal);

        deal.CallTrumpDecisions[0].ValidCallTrumpDecisions.Should()
            .Contain([CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp, CallTrumpDecision.OrderItUpAndGoAlone]);
        deal.CallTrumpDecisions[4].ValidCallTrumpDecisions.Should()
            .NotContain(CallTrumpDecision.CallHearts)
            .And.NotContain(CallTrumpDecision.CallHeartsAndGoAlone);
    }

    [Fact]
    public async Task SelectTrumpAsync_Round2_CapturesDecisionsWithCorrectOrder()
    {
        var deal = CreateTestDeal();
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        deal.DealerPosition = PlayerPosition.North;

        _playerActorMock.SetupSequence(b => b.CallTrumpAsync(It.IsAny<CallTrumpContext>()))
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.Pass)
            .ReturnsAsync(CallTrumpDecision.CallSpades);

        await _sut.SelectTrumpAsync(deal);

        deal.CallTrumpDecisions.Should().HaveCount(5);
        deal.CallTrumpDecisions[4].DecisionOrder.Should().Be(5);
        deal.CallTrumpDecisions[4].ChosenDecision.Should().Be(CallTrumpDecision.CallSpades);
        deal.CallTrumpDecisions[4].PlayerPosition.Should().Be(PlayerPosition.East);
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
        _playerActorMock.Setup(b => b.DiscardCardAsync(It.IsAny<DiscardCardContext>()))
            .ReturnsAsync((DiscardCardContext ctx) => ctx.CardsInHand[0]);
    }
}
