using FluentAssertions;

using Moq;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Handlers;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Services;
using NemesisEuchre.GameEngine.Tests.TestHelpers;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.GameEngine.Tests;

public class TrickPlayingOrchestratorTests
{
    private readonly Mock<IPlayerActor> _playerActorMock;
    private readonly Mock<IPlayerActorResolver> _actorResolverMock;
    private readonly TrickPlayingOrchestrator _sut;

    public TrickPlayingOrchestratorTests()
    {
        _playerActorMock = new Mock<IPlayerActor>();
        _playerActorMock.Setup(b => b.ActorType).Returns(ActorType.Chaos);

        _actorResolverMock = new Mock<IPlayerActorResolver>();
        _actorResolverMock.Setup(x => x.GetPlayerActor(It.IsAny<DealPlayer>()))
            .Returns(_playerActorMock.Object);

        var validator = new TrickPlayingValidator();
        var goingAloneHandler = new GoingAloneHandler();
        var contextBuilder = new PlayerContextBuilder();
        var trickWinnerCalculator = new TrickWinnerCalculator();
        var cardAccountingService = new CardAccountingService();
        var decisionRecorder = new DecisionRecorder(contextBuilder, cardAccountingService);
        var voidDetector = new VoidDetector();

        _sut = new TrickPlayingOrchestrator(validator, goingAloneHandler, contextBuilder, _actorResolverMock.Object, trickWinnerCalculator, decisionRecorder, voidDetector, cardAccountingService);
    }

    [Fact]
    public Task PlayTrickAsync_WithNullDeal_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.PlayTrickAsync(null!, PlayerPosition.North);

        return act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("deal");
    }

    [Theory]
    [InlineData(DealStatus.NotStarted)]
    [InlineData(DealStatus.SelectingTrump)]
    [InlineData(DealStatus.Scoring)]
    [InlineData(DealStatus.Complete)]
    public Task PlayTrickAsync_WithWrongDealStatus_ThrowsInvalidOperationException(DealStatus status)
    {
        var deal = CreateTestDeal();
        deal.DealStatus = status;

        var act = async () => await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Deal must be in Playing status, but was {status}");
    }

    [Fact]
    public Task PlayTrickAsync_WithNullTrump_ThrowsInvalidOperationException()
    {
        var deal = CreateTestDeal();
        deal.Trump = null;

        var act = async () => await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Trump must be set");
    }

    [Fact]
    public Task PlayTrickAsync_WithNullCallingPlayer_ThrowsInvalidOperationException()
    {
        var deal = CreateTestDeal();
        deal.CallingPlayer = null;

        var act = async () => await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CallingPlayer must be set");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public Task PlayTrickAsync_WithWrongPlayerCount_ThrowsInvalidOperationException(int playerCount)
    {
        var deal = CreateTestDeal();
        deal.Players.Clear();
        for (int i = 0; i < playerCount; i++)
        {
            deal.Players[(PlayerPosition)i] = CreateTestPlayer((PlayerPosition)i);
        }

        var act = async () => await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Deal must have exactly 4 players, but had {playerCount}");
    }

    [Fact]
    public async Task PlayTrickAsync_ReturnsCompletedTrickWith4Cards()
    {
        var deal = CreateTestDeal();
        SetupBasicCardPlaySequence();

        var result = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        result.Should().NotBeNull();
        result.CardsPlayed.Should().HaveCount(4);
    }

    [Fact]
    public async Task PlayTrickAsync_SetsLeadPositionCorrectly()
    {
        var deal = CreateTestDeal();
        SetupBasicCardPlaySequence();

        var result = await _sut.PlayTrickAsync(deal, PlayerPosition.South);

        result.LeadPosition.Should().Be(PlayerPosition.South);
    }

    [Fact]
    public async Task PlayTrickAsync_SetsLeadSuitFromFirstCardEffectiveSuit()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Hearts;

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .ReturnsFirstValidCard();

        var result = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        var expectedLeadSuit = deal.Players[PlayerPosition.North].CurrentHand[0].GetEffectiveSuit(deal.Trump.Value);
        result.LeadSuit.Should().Be(expectedLeadSuit);
    }

    [Fact]
    public async Task PlayTrickAsync_RemovesCardsFromPlayersHands()
    {
        var deal = CreateTestDeal();
        SetupBasicCardPlaySequence();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        deal.Players[PlayerPosition.North].CurrentHand.Should().HaveCount(4);
        deal.Players[PlayerPosition.East].CurrentHand.Should().HaveCount(4);
        deal.Players[PlayerPosition.South].CurrentHand.Should().HaveCount(4);
        deal.Players[PlayerPosition.West].CurrentHand.Should().HaveCount(4);
    }

    [Fact]
    public async Task PlayTrickAsync_IteratesPlayersInCorrectOrder_StartingWithNorth()
    {
        var deal = CreateTestDeal();
        var playOrder = new List<PlayerPosition>();

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .Callback((PlayCardContext ctx) => playOrder.Add(ctx.PlayerPosition))
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        playOrder.Should().Equal(PlayerPosition.North, PlayerPosition.East, PlayerPosition.South, PlayerPosition.West);
    }

    [Fact]
    public async Task PlayTrickAsync_IteratesPlayersInCorrectOrder_StartingWithEast()
    {
        var deal = CreateTestDeal();
        var playOrder = new List<PlayerPosition>();

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .Callback((PlayCardContext ctx) => playOrder.Add(ctx.PlayerPosition))
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.East);

        playOrder.Should().Equal(PlayerPosition.East, PlayerPosition.South, PlayerPosition.West, PlayerPosition.North);
    }

    [Theory]
    [InlineData(PlayerPosition.North)]
    [InlineData(PlayerPosition.East)]
    [InlineData(PlayerPosition.South)]
    [InlineData(PlayerPosition.West)]
    public async Task PlayTrickAsync_WorksWithAllLeadPositions(PlayerPosition leadPosition)
    {
        var deal = CreateTestDeal();
        SetupBasicCardPlaySequence();

        var result = await _sut.PlayTrickAsync(deal, leadPosition);

        result.LeadPosition.Should().Be(leadPosition);
        result.CardsPlayed.Should().HaveCount(4);
    }

    [Fact]
    public async Task PlayTrickAsync_LeadPlayerCanPlayAnyCard()
    {
        var deal = CreateTestDeal();
        Card[]? capturedValidCards = null;

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .Callback((PlayCardContext ctx) => capturedValidCards ??= ctx.ValidCardsToPlay)
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        capturedValidCards.Should().HaveCount(5);
    }

    [Fact]
    public async Task PlayTrickAsync_FollowerWithMatchingSuit_MustFollowSuit()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Hearts;
        deal.Players[PlayerPosition.North].CurrentHand = [
            new Card { Suit = Suit.Spades, Rank = Rank.King },
            new Card { Suit = Suit.Hearts, Rank = Rank.Ten },
            new Card { Suit = Suit.Clubs, Rank = Rank.Nine },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new Card { Suit = Suit.Diamonds, Rank = Rank.King }
        ];
        deal.Players[PlayerPosition.East].CurrentHand = [
            new Card { Suit = Suit.Spades, Rank = Rank.Nine },
            new Card { Suit = Suit.Spades, Rank = Rank.Ten },
            new Card { Suit = Suit.Hearts, Rank = Rank.Jack },
            new Card { Suit = Suit.Clubs, Rank = Rank.Ace },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ace }
        ];

        Card[]? capturedValidCards = null;
        var callCount = 0;

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .Callback((PlayCardContext ctx) =>
            {
                callCount++;
                if (callCount == 2)
                {
                    capturedValidCards = ctx.ValidCardsToPlay;
                }
            })
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        capturedValidCards.Should().HaveCount(2);
        capturedValidCards!.All(c => c.Suit == Suit.Spades).Should().BeTrue();
    }

    [Fact]
    public async Task PlayTrickAsync_FollowerWithoutMatchingSuit_CanPlayAnyCard()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Hearts;
        deal.Players[PlayerPosition.North].CurrentHand = [
            new Card { Suit = Suit.Clubs, Rank = Rank.Ace },
            new Card { Suit = Suit.Hearts, Rank = Rank.Ten },
            new Card { Suit = Suit.Spades, Rank = Rank.Nine },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new Card { Suit = Suit.Diamonds, Rank = Rank.King }
        ];
        deal.Players[PlayerPosition.East].CurrentHand = [
            new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
            new Card { Suit = Suit.Hearts, Rank = Rank.Jack },
            new Card { Suit = Suit.Spades, Rank = Rank.Ace },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ace },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ten }
        ];

        Card[]? capturedValidCards = null;
        var callCount = 0;

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .Callback((PlayCardContext ctx) =>
            {
                callCount++;
                if (callCount == 2)
                {
                    capturedValidCards = ctx.ValidCardsToPlay;
                }
            })
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        capturedValidCards.Should().HaveCount(5);
    }

    [Fact]
    public Task PlayTrickAsync_WithInvalidCardChoice_ThrowsInvalidOperationException()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Hearts;
        deal.Players[PlayerPosition.East].CurrentHand = [
            new Card { Suit = Suit.Spades, Rank = Rank.Nine },
            new Card { Suit = Suit.Spades, Rank = Rank.Ten },
            new Card { Suit = Suit.Hearts, Rank = Rank.Jack },
            new Card { Suit = Suit.Clubs, Rank = Rank.Ace },
            new Card { Suit = Suit.Diamonds, Rank = Rank.King }
        ];

        var callCount = 0;
        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .Callback((PlayCardContext _) => callCount++)
            .Returns((PlayCardContext ctx) =>
            {
                if (callCount == 2)
                {
                    return Task.FromResult(new Card { Suit = Suit.Hearts, Rank = Rank.Jack });
                }

                return Task.FromResult(ctx.ValidCardsToPlay[0]);
            });

        var act = async () => await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ChosenCard was not included in ValidCards");
    }

    [Fact]
    public async Task PlayTrickAsync_ValidCardsArray_ContainsOnlyLegalCards()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Clubs;
        deal.Players[PlayerPosition.East].CurrentHand = [
            new Card { Suit = Suit.Spades, Rank = Rank.Nine },
            new Card { Suit = Suit.Spades, Rank = Rank.Ace },
            new Card { Suit = Suit.Hearts, Rank = Rank.King },
            new Card { Suit = Suit.Clubs, Rank = Rank.Ten },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Queen }
        ];

        List<Card[]> allValidCards = [];

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .Callback((PlayCardContext ctx) => allValidCards.Add(ctx.ValidCardsToPlay))
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        allValidCards.Should().HaveCount(4);
        allValidCards[0].Should().HaveCount(5);
        allValidCards[1].Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task PlayTrickAsync_LeftBowerCountsAsTrumpSuit()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Hearts;
        deal.Players[PlayerPosition.East].CurrentHand = [
            new Card { Suit = Suit.Diamonds, Rank = Rank.Jack },
            new Card { Suit = Suit.Spades, Rank = Rank.Nine },
            new Card { Suit = Suit.Clubs, Rank = Rank.Ten },
            new Card { Suit = Suit.Clubs, Rank = Rank.Ace },
            new Card { Suit = Suit.Spades, Rank = Rank.King }
        ];

        Card[]? capturedValidCards = null;
        var callCount = 0;

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .Callback((PlayCardContext ctx) =>
            {
                callCount++;
                if (callCount == 2)
                {
                    capturedValidCards = ctx.ValidCardsToPlay;
                }
            })
            .ReturnsFirstValidCard();

        var result = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        result.LeadSuit.Should().Be(Suit.Hearts);
        result.CardsPlayed[1].Card.Should().BeEquivalentTo(new Card { Suit = Suit.Diamonds, Rank = Rank.Jack });
    }

    [Fact]
    public async Task PlayTrickAsync_WhenTrumpLed_FollowersMustPlayTrumpIfAvailable()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Clubs;
        deal.Players[PlayerPosition.North].CurrentHand = [
            new Card { Suit = Suit.Clubs, Rank = Rank.Ace },
            new Card { Suit = Suit.Hearts, Rank = Rank.Ten },
            new Card { Suit = Suit.Spades, Rank = Rank.Nine },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new Card { Suit = Suit.Diamonds, Rank = Rank.King }
        ];
        deal.Players[PlayerPosition.East].CurrentHand = [
            new Card { Suit = Suit.Clubs, Rank = Rank.Nine },
            new Card { Suit = Suit.Spades, Rank = Rank.Ace },
            new Card { Suit = Suit.Hearts, Rank = Rank.King },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ace },
            new Card { Suit = Suit.Spades, Rank = Rank.Ten }
        ];

        Card[]? capturedValidCards = null;
        var callCount = 0;

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .Callback((PlayCardContext ctx) =>
            {
                callCount++;
                if (callCount == 2)
                {
                    capturedValidCards = ctx.ValidCardsToPlay;
                }
            })
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        capturedValidCards.Should().HaveCount(1);
        capturedValidCards![0].Suit.Should().Be(Suit.Clubs);
    }

    [Fact]
    public async Task PlayTrickAsync_CanPlayTrumpOnAnyLead()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Hearts;

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .ReturnsFirstValidCard();

        var result = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        result.CardsPlayed.Should().HaveCount(4);
    }

    [Fact]
    public async Task PlayTrickAsync_LeftBowerMustFollowTrumpSuit()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Spades;
        deal.Players[PlayerPosition.North].CurrentHand = [
            new Card { Suit = Suit.Spades, Rank = Rank.Ace },
            new Card { Suit = Suit.Hearts, Rank = Rank.Ten },
            new Card { Suit = Suit.Clubs, Rank = Rank.Nine },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new Card { Suit = Suit.Diamonds, Rank = Rank.King }
        ];
        deal.Players[PlayerPosition.East].CurrentHand = [
            new Card { Suit = Suit.Clubs, Rank = Rank.Jack },
            new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ace },
            new Card { Suit = Suit.Hearts, Rank = Rank.Queen },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ten }
        ];

        Card[]? capturedValidCards = null;
        var callCount = 0;

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .Callback((PlayCardContext ctx) =>
            {
                callCount++;
                if (callCount == 2)
                {
                    capturedValidCards = ctx.ValidCardsToPlay;
                }
            })
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        capturedValidCards.Should().HaveCount(1);
        capturedValidCards![0].Rank.Should().Be(Rank.Jack);
        capturedValidCards![0].Suit.Should().Be(Suit.Clubs);
    }

    [Fact]
    public async Task PlayTrickAsync_IfLeftBowerIsOnlyTrump_MustPlayIt()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Hearts;
        deal.Players[PlayerPosition.East].CurrentHand = [
            new Card { Suit = Suit.Diamonds, Rank = Rank.Jack },
            new Card { Suit = Suit.Spades, Rank = Rank.Ace },
            new Card { Suit = Suit.Clubs, Rank = Rank.King },
            new Card { Suit = Suit.Spades, Rank = Rank.Queen },
            new Card { Suit = Suit.Clubs, Rank = Rank.Ten }
        ];

        Card[]? capturedValidCards = null;
        var callCount = 0;

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .Callback((PlayCardContext ctx) =>
            {
                callCount++;
                if (callCount == 2)
                {
                    capturedValidCards = ctx.ValidCardsToPlay;
                }
            })
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        capturedValidCards.Should().HaveCount(1);
        capturedValidCards![0].Rank.Should().Be(Rank.Jack);
        capturedValidCards![0].Suit.Should().Be(Suit.Diamonds);
    }

    [Fact]
    public async Task PlayTrickAsync_LeadSuitConsidersEffectiveSuit_NotFaceSuit()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Clubs;

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .ReturnsFirstValidCard();

        var result = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        var expectedLeadSuit = deal.Players[PlayerPosition.North].CurrentHand[0].GetEffectiveSuit(deal.Trump.Value);
        result.LeadSuit.Should().Be(expectedLeadSuit);
    }

    [Fact]
    public async Task PlayTrickAsync_WhenGoingAlone_PartnerDoesNotPlay()
    {
        var deal = CreateTestDeal();
        deal.CallingPlayer = PlayerPosition.North;
        deal.CallingPlayerIsGoingAlone = true;

        SetupBasicCardPlaySequence();

        var result = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        result.CardsPlayed.Should().HaveCount(3);
        result.CardsPlayed.Should().NotContain(pc => pc.PlayerPosition == PlayerPosition.South);
    }

    [Fact]
    public async Task PlayTrickAsync_WhenGoingAlone_Only3CardsPlayed()
    {
        var deal = CreateTestDeal();
        deal.CallingPlayer = PlayerPosition.East;
        deal.CallingPlayerIsGoingAlone = true;

        SetupBasicCardPlaySequence();

        var result = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        result.CardsPlayed.Should().HaveCount(3);
    }

    [Fact]
    public async Task PlayTrickAsync_WhenNotGoingAlone_All4PlayersPlay()
    {
        var deal = CreateTestDeal();
        deal.CallingPlayer = PlayerPosition.North;
        deal.CallingPlayerIsGoingAlone = false;

        SetupBasicCardPlaySequence();

        var result = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        result.CardsPlayed.Should().HaveCount(4);
    }

    [Fact]
    public async Task PlayTrickAsync_WhenGoingAlone_PlayerIterationSkipsPartnerCorrectly()
    {
        var deal = CreateTestDeal();
        deal.CallingPlayer = PlayerPosition.North;
        deal.CallingPlayerIsGoingAlone = true;

        var playOrder = new List<PlayerPosition>();

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .Callback((PlayCardContext ctx) => playOrder.Add(ctx.PlayerPosition))
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        playOrder.Should().Equal(PlayerPosition.North, PlayerPosition.East, PlayerPosition.West);
        playOrder.Should().NotContain(PlayerPosition.South);
    }

    [Fact]
    public async Task PlayTrickAsync_CallsPlayCardAsyncWithCorrectParameters()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Hearts;
        deal.Team1Score = 5;
        deal.Team2Score = 3;

        Card[]? capturedHand = null;
        short? capturedTeamScore = null;
        short? capturedOpponentScore = null;
        Card[]? capturedValidCards = null;

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .Callback((PlayCardContext ctx) =>
            {
                capturedHand ??= ctx.CardsInHand;
                capturedTeamScore ??= ctx.TeamScore;
                capturedOpponentScore ??= ctx.OpponentScore;
                capturedValidCards ??= ctx.ValidCardsToPlay;
            })
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        capturedHand.Should().NotBeNull();
        capturedHand.Should().HaveCount(5);
        capturedTeamScore.Should().Be(5);
        capturedOpponentScore.Should().Be(3);
        capturedValidCards.Should().NotBeNull();
    }

    [Fact]
    public Task PlayTrickAsync_PassesRelativeDealFromCorrectPlayerPerspective()
    {
        var deal = CreateTestDeal();
        deal.DealerPosition = PlayerPosition.North;
        deal.CallingPlayer = PlayerPosition.East;

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .ReturnsFirstValidCard();

        var result = _sut.PlayTrickAsync(deal, PlayerPosition.North);

        _playerActorMock.Verify(
            b => b.PlayCardAsync(It.IsAny<PlayCardContext>()),
            Times.AtLeastOnce());

        return result;
    }

    [Fact]
    public async Task PlayTrickAsync_PassesCorrectTeamAndOpponentScoresForEachPlayer()
    {
        var deal = CreateTestDeal();
        deal.Team1Score = 7;
        deal.Team2Score = 4;

        List<(short Team, short Opponent)> capturedScores = [];

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .Callback((PlayCardContext ctx) => capturedScores.Add((ctx.TeamScore, ctx.OpponentScore)))
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        capturedScores.Should().HaveCount(4);
        capturedScores[0].Should().Be((7, 4));
        capturedScores[1].Should().Be((4, 7));
        capturedScores[2].Should().Be((7, 4));
        capturedScores[3].Should().Be((4, 7));
    }

    [Fact]
    public async Task PlayTrickAsync_CapturesPlayCardDecision()
    {
        var deal = CreateTestDeal();
        SetupBasicCardPlaySequence();

        var trick = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        trick.PlayCardDecisions.Should().NotBeEmpty();
        trick.PlayCardDecisions.Should().HaveCount(4);
    }

    [Fact]
    public async Task PlayTrickAsync_CapturesHandWithCorrectCards()
    {
        var deal = CreateTestDeal();
        var expectedHand = deal.Players[PlayerPosition.North].CurrentHand.ToArray();
        SetupBasicCardPlaySequence();

        var trick = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        var firstDecision = trick.PlayCardDecisions[0];
        firstDecision.CardsInHand.Should().HaveCount(5);
        firstDecision.CardsInHand.Should().BeEquivalentTo(expectedHand);
    }

    [Fact]
    public async Task PlayTrickAsync_CapturesTeamAndOpponentScores()
    {
        var deal = CreateTestDeal();
        deal.Team1Score = 6;
        deal.Team2Score = 3;
        SetupBasicCardPlaySequence();

        var trick = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        var northDecision = trick.PlayCardDecisions.First(d => d.PlayerPosition == PlayerPosition.North);
        northDecision.TeamScore.Should().Be(6);
        northDecision.OpponentScore.Should().Be(3);

        var eastDecision = trick.PlayCardDecisions.First(d => d.PlayerPosition == PlayerPosition.East);
        eastDecision.TeamScore.Should().Be(3);
        eastDecision.OpponentScore.Should().Be(6);
    }

    [Fact]
    public async Task PlayTrickAsync_CapturesValidCardsToPlay()
    {
        var deal = CreateTestDeal();
        SetupBasicCardPlaySequence();

        var trick = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        var leadDecision = trick.PlayCardDecisions[0];
        leadDecision.ValidCardsToPlay.Should().HaveCount(5);
        leadDecision.ValidCardsToPlay.Should().BeEquivalentTo(leadDecision.CardsInHand);
    }

    [Fact]
    public async Task PlayTrickAsync_CapturesValidCardsToPlay_WhenMustFollowSuit()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Hearts;
        deal.Players[PlayerPosition.North].CurrentHand = [
            new Card { Suit = Suit.Spades, Rank = Rank.King },
            new Card { Suit = Suit.Hearts, Rank = Rank.Ten },
            new Card { Suit = Suit.Clubs, Rank = Rank.Nine },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new Card { Suit = Suit.Diamonds, Rank = Rank.King }
        ];
        deal.Players[PlayerPosition.East].CurrentHand = [
            new Card { Suit = Suit.Spades, Rank = Rank.Nine },
            new Card { Suit = Suit.Spades, Rank = Rank.Ten },
            new Card { Suit = Suit.Hearts, Rank = Rank.Jack },
            new Card { Suit = Suit.Clubs, Rank = Rank.Ace },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ace }
        ];

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .ReturnsFirstValidCard();

        var trick = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        var eastDecision = trick.PlayCardDecisions.First(d => d.PlayerPosition == PlayerPosition.East);
        eastDecision.ValidCardsToPlay.Should().HaveCount(2);
        eastDecision.ValidCardsToPlay.All(c => c.Suit == Suit.Spades).Should().BeTrue();
    }

    [Fact]
    public async Task PlayTrickAsync_CapturesChosenCard()
    {
        var deal = CreateTestDeal();
        var expectedCard = deal.Players[PlayerPosition.North].CurrentHand[0];
        SetupBasicCardPlaySequence();

        var trick = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        var firstDecision = trick.PlayCardDecisions[0];
        firstDecision.ChosenCard.Should().BeEquivalentTo(expectedCard);
    }

    [Fact]
    public async Task PlayTrickAsync_CapturesPlayerPosition()
    {
        var deal = CreateTestDeal();
        SetupBasicCardPlaySequence();

        var trick = await _sut.PlayTrickAsync(deal, PlayerPosition.East);

        trick.PlayCardDecisions.Should().HaveCount(4);
        trick.PlayCardDecisions[0].PlayerPosition.Should().Be(PlayerPosition.East);
        trick.PlayCardDecisions[1].PlayerPosition.Should().Be(PlayerPosition.South);
        trick.PlayCardDecisions[2].PlayerPosition.Should().Be(PlayerPosition.West);
        trick.PlayCardDecisions[3].PlayerPosition.Should().Be(PlayerPosition.North);
    }

    [Fact]
    public async Task PlayTrickAsync_CapturesLeadPlayer()
    {
        var deal = CreateTestDeal();
        SetupBasicCardPlaySequence();

        var trick = await _sut.PlayTrickAsync(deal, PlayerPosition.South);

        trick.PlayCardDecisions.Should().OnlyContain(d => d.LeadPlayer == PlayerPosition.South);
    }

    [Fact]
    public async Task PlayTrickAsync_CapturesPlayedCards_EmptyForLeadPlayer()
    {
        var deal = CreateTestDeal();
        SetupBasicCardPlaySequence();

        var trick = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        var leadDecision = trick.PlayCardDecisions[0];
        leadDecision.PlayedCards.Should().BeEmpty();
        leadDecision.LeadPlayer.Should().Be(PlayerPosition.North);
        leadDecision.LeadSuit.Should().BeNull();
    }

    [Fact]
    public async Task PlayTrickAsync_CapturesPlayedCards_WithPreviousCards()
    {
        var deal = CreateTestDeal();
        SetupBasicCardPlaySequence();

        var trick = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        var secondDecision = trick.PlayCardDecisions[1];
        secondDecision.PlayedCards.Should().HaveCount(1);

        var thirdDecision = trick.PlayCardDecisions[2];
        thirdDecision.PlayedCards.Should().HaveCount(2);

        var fourthDecision = trick.PlayCardDecisions[3];
        fourthDecision.PlayedCards.Should().HaveCount(3);
    }

    [Fact]
    public async Task PlayTrickAsync_WhenGoingAlone_CapturesOnly3Decisions()
    {
        var deal = CreateTestDeal();
        deal.CallingPlayer = PlayerPosition.North;
        deal.CallingPlayerIsGoingAlone = true;
        SetupBasicCardPlaySequence();

        var trick = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        trick.PlayCardDecisions.Should().HaveCount(3);
        trick.PlayCardDecisions.Should().NotContain(d => d.PlayerPosition == PlayerPosition.South);
    }

    [Fact]
    public async Task PlayTrickAsync_CapturesAllDecisionsInCompleteDeal()
    {
        var deal = CreateTestDeal();
        SetupBasicCardPlaySequence();

        for (int trickNumber = 0; trickNumber < 5; trickNumber++)
        {
            var trick = await _sut.PlayTrickAsync(deal, PlayerPosition.North);
            deal.CompletedTricks.Add(trick);
        }

        deal.CompletedTricks.SelectMany(t => t.PlayCardDecisions).Should().HaveCount(20);
    }

    [Fact]
    public async Task PlayTrickAsync_WhenPlayerCannotFollowSuit_VoidIsRecorded()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Hearts;
        deal.Players[PlayerPosition.North].CurrentHand = [
            new Card { Suit = Suit.Spades, Rank = Rank.Ace },
            new Card { Suit = Suit.Hearts, Rank = Rank.Ten },
            new Card { Suit = Suit.Clubs, Rank = Rank.Nine },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new Card { Suit = Suit.Diamonds, Rank = Rank.King }
        ];
        deal.Players[PlayerPosition.East].CurrentHand = [
            new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
            new Card { Suit = Suit.Clubs, Rank = Rank.Ten },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ace },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ten },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Nine }
        ];

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        deal.KnownPlayerSuitVoids.Should().Contain((PlayerPosition.East, Suit.Spades));
    }

    [Fact]
    public async Task PlayTrickAsync_VoidAppearsInSubsequentDecisions()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Hearts;
        deal.Players[PlayerPosition.North].CurrentHand = [
            new Card { Suit = Suit.Spades, Rank = Rank.Ace },
            new Card { Suit = Suit.Hearts, Rank = Rank.Ten },
            new Card { Suit = Suit.Clubs, Rank = Rank.Nine },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new Card { Suit = Suit.Diamonds, Rank = Rank.King }
        ];
        deal.Players[PlayerPosition.East].CurrentHand = [
            new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
            new Card { Suit = Suit.Clubs, Rank = Rank.Ten },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ace },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ten },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Nine }
        ];

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .ReturnsFirstValidCard();

        var trick = await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        trick.PlayCardDecisions[0].KnownPlayerSuitVoids.Should().BeEmpty();
        trick.PlayCardDecisions[1].KnownPlayerSuitVoids.Should().BeEmpty();
        trick.PlayCardDecisions[2].KnownPlayerSuitVoids.Should().Contain((PlayerPosition.East, Suit.Spades));
        trick.PlayCardDecisions[3].KnownPlayerSuitVoids.Should().Contain((PlayerPosition.East, Suit.Spades));
    }

    [Fact]
    public async Task PlayTrickAsync_MultipleVoidsRecorded()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Hearts;
        deal.Players[PlayerPosition.North].CurrentHand = [
            new Card { Suit = Suit.Spades, Rank = Rank.Ace },
            new Card { Suit = Suit.Hearts, Rank = Rank.Ten },
            new Card { Suit = Suit.Clubs, Rank = Rank.Nine },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new Card { Suit = Suit.Diamonds, Rank = Rank.King }
        ];
        deal.Players[PlayerPosition.East].CurrentHand = [
            new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
            new Card { Suit = Suit.Clubs, Rank = Rank.Ten },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ace },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ten },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Nine }
        ];
        deal.Players[PlayerPosition.South].CurrentHand = [
            new Card { Suit = Suit.Hearts, Rank = Rank.King },
            new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Jack },
            new Card { Suit = Suit.Clubs, Rank = Rank.Jack },
            new Card { Suit = Suit.Clubs, Rank = Rank.Queen }
        ];

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        deal.KnownPlayerSuitVoids.Should().Contain((PlayerPosition.East, Suit.Spades));
        deal.KnownPlayerSuitVoids.Should().Contain((PlayerPosition.South, Suit.Spades));
        deal.KnownPlayerSuitVoids.Should().HaveCount(2);
    }

    [Fact]
    public async Task PlayTrickAsync_SameVoidDetectedTwice_NotDuplicated()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Hearts;
        deal.KnownPlayerSuitVoids = [(PlayerPosition.East, Suit.Spades)];

        deal.Players[PlayerPosition.North].CurrentHand = [
            new Card { Suit = Suit.Spades, Rank = Rank.Ace },
            new Card { Suit = Suit.Hearts, Rank = Rank.Ten },
            new Card { Suit = Suit.Clubs, Rank = Rank.Nine },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new Card { Suit = Suit.Diamonds, Rank = Rank.King }
        ];
        deal.Players[PlayerPosition.East].CurrentHand = [
            new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
            new Card { Suit = Suit.Clubs, Rank = Rank.Ten },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ace },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ten },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Nine }
        ];

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        deal.KnownPlayerSuitVoids.Should().HaveCount(1);
        deal.KnownPlayerSuitVoids.Should().Contain((PlayerPosition.East, Suit.Spades));
    }

    [Fact]
    public async Task PlayTrickAsync_LeftBowerScenario_VoidDetectedInCorrectSuit()
    {
        var deal = CreateTestDeal();
        deal.Trump = Suit.Spades;
        deal.Players[PlayerPosition.North].CurrentHand = [
            new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
            new Card { Suit = Suit.Spades, Rank = Rank.Ten },
            new Card { Suit = Suit.Clubs, Rank = Rank.Nine },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new Card { Suit = Suit.Diamonds, Rank = Rank.King }
        ];
        deal.Players[PlayerPosition.East].CurrentHand = [
            new Card { Suit = Suit.Clubs, Rank = Rank.Jack },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ace },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Ten },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Nine },
            new Card { Suit = Suit.Clubs, Rank = Rank.Ten }
        ];

        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .ReturnsFirstValidCard();

        await _sut.PlayTrickAsync(deal, PlayerPosition.North);

        deal.KnownPlayerSuitVoids.Should().Contain((PlayerPosition.East, Suit.Hearts));
    }

    private static Deal CreateTestDeal()
    {
        var deal = new Deal
        {
            DealStatus = DealStatus.Playing,
            DealerPosition = PlayerPosition.North,
            Trump = Suit.Hearts,
            CallingPlayer = PlayerPosition.North,
            CallingPlayerIsGoingAlone = false,
            Team1Score = 0,
            Team2Score = 0,
        };

        deal.Players[PlayerPosition.North] = CreateTestPlayer(PlayerPosition.North);
        deal.Players[PlayerPosition.East] = CreateTestPlayer(PlayerPosition.East);
        deal.Players[PlayerPosition.South] = CreateTestPlayer(PlayerPosition.South);
        deal.Players[PlayerPosition.West] = CreateTestPlayer(PlayerPosition.West);

        return deal;
    }

    private static DealPlayer CreateTestPlayer(PlayerPosition position)
    {
        var cards = position switch
        {
            PlayerPosition.North => new[]
            {
                new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
                new Card { Suit = Suit.Hearts, Rank = Rank.Ten },
                new Card { Suit = Suit.Spades, Rank = Rank.Nine },
                new Card { Suit = Suit.Clubs, Rank = Rank.Nine },
                new Card { Suit = Suit.Diamonds, Rank = Rank.Nine },
            },
            PlayerPosition.East =>
            [
                new Card { Suit = Suit.Hearts, Rank = Rank.Jack },
                new Card { Suit = Suit.Hearts, Rank = Rank.Queen },
                new Card { Suit = Suit.Spades, Rank = Rank.Ten },
                new Card { Suit = Suit.Clubs, Rank = Rank.Ten },
                new Card { Suit = Suit.Diamonds, Rank = Rank.Ten }
            ],
            PlayerPosition.South =>
            [
                new Card { Suit = Suit.Hearts, Rank = Rank.King },
                new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
                new Card { Suit = Suit.Spades, Rank = Rank.Jack },
                new Card { Suit = Suit.Clubs, Rank = Rank.Jack },
                new Card { Suit = Suit.Diamonds, Rank = Rank.Jack }
            ],
            PlayerPosition.West =>
            [
                new Card { Suit = Suit.Diamonds, Rank = Rank.Ace },
                new Card { Suit = Suit.Diamonds, Rank = Rank.King },
                new Card { Suit = Suit.Spades, Rank = Rank.Queen },
                new Card { Suit = Suit.Clubs, Rank = Rank.Queen },
                new Card { Suit = Suit.Diamonds, Rank = Rank.Queen }
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(position))
        };

        return new DealPlayer
        {
            Position = position,
            ActorType = ActorType.Chaos,
            CurrentHand = [.. cards],
            StartingHand = cards,
        };
    }

    private void SetupBasicCardPlaySequence()
    {
        _playerActorMock.Setup(b => b.PlayCardAsync(It.IsAny<PlayCardContext>()))
            .ReturnsFirstValidCard();
    }
}
