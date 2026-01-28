using FluentAssertions;

using Moq;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.GameEngine.Tests;

public class DealOrchestratorTests
{
    private readonly Mock<ITrumpSelectionOrchestrator> _trumpSelectionMock;
    private readonly Mock<ITrickPlayingOrchestrator> _trickPlayingMock;
    private readonly Mock<ITrickWinnerCalculator> _trickWinnerMock;
    private readonly Mock<IDealResultCalculator> _dealResultMock;
    private readonly Mock<IPlayerActor> _playerActorMock;
    private readonly DealOrchestrator _sut;

    public DealOrchestratorTests()
    {
        _trumpSelectionMock = new Mock<ITrumpSelectionOrchestrator>();
        _trickPlayingMock = new Mock<ITrickPlayingOrchestrator>();
        _trickWinnerMock = new Mock<ITrickWinnerCalculator>();
        _dealResultMock = new Mock<IDealResultCalculator>();
        _playerActorMock = new Mock<IPlayerActor>();

        _playerActorMock.Setup(x => x.ActorType).Returns(ActorType.Chaos);

        SetupTrumpSelectionMock();
        SetupTrickPlayMocks();
        SetupDealResultMock();

        var validator = new DealValidator();
        _sut = new DealOrchestrator(
            _trumpSelectionMock.Object,
            _trickPlayingMock.Object,
            _trickWinnerMock.Object,
            _dealResultMock.Object,
            validator);
    }

    [Fact]
    public Task OrchestrateDealAsync_WithNullDeal_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.OrchestrateDealAsync(null!);

        return act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("deal");
    }

    [Theory]
    [InlineData(DealStatus.SelectingTrump)]
    [InlineData(DealStatus.Playing)]
    [InlineData(DealStatus.Scoring)]
    [InlineData(DealStatus.Complete)]
    public Task OrchestrateDealAsync_WithInvalidStatus_ThrowsInvalidOperationException(DealStatus status)
    {
        var deal = CreateTestDeal();
        deal.DealStatus = status;

        var act = async () => await _sut.OrchestrateDealAsync(deal);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Deal must be in NotStarted status, but was {status}");
    }

    [Fact]
    public Task OrchestrateDealAsync_WithMissingDealerPosition_ThrowsInvalidOperationException()
    {
        var deal = CreateTestDeal();
        deal.DealerPosition = null;

        var act = async () => await _sut.OrchestrateDealAsync(deal);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("DealerPosition must be set");
    }

    [Fact]
    public Task OrchestrateDealAsync_WithMissingUpCard_ThrowsInvalidOperationException()
    {
        var deal = CreateTestDeal();
        deal.UpCard = null;

        var act = async () => await _sut.OrchestrateDealAsync(deal);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("UpCard must be set");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public Task OrchestrateDealAsync_WithWrongPlayerCount_ThrowsInvalidOperationException(int playerCount)
    {
        var deal = CreateTestDeal();
        deal.Players.Clear();
        for (int i = 0; i < playerCount; i++)
        {
            deal.Players[(PlayerPosition)i] = new DealPlayer();
        }

        var act = async () => await _sut.OrchestrateDealAsync(deal);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Deal must have exactly 4 players, but had {playerCount}");
    }

    [Fact]
    public async Task OrchestrateDealAsync_SetsDealStatusToSelectingTrump_BeforeTrumpSelection()
    {
        var deal = CreateTestDeal();
        DealStatus? capturedStatus = null;

        _trumpSelectionMock.Setup(x => x.SelectTrumpAsync(It.IsAny<Deal>()))
            .Callback<Deal>(d =>
            {
                capturedStatus = d.DealStatus;
                d.Trump = Suit.Spades;
                d.CallingPlayer = PlayerPosition.East;
            })
            .Returns(Task.CompletedTask);

        await _sut.OrchestrateDealAsync(deal);

        capturedStatus.Should().Be(DealStatus.SelectingTrump);
    }

    [Fact]
    public async Task OrchestrateDealAsync_SetsDealStatusToPlaying_AfterTrumpSelection()
    {
        var deal = CreateTestDeal();
        DealStatus? capturedStatus = null;

        _trickPlayingMock.Setup(x => x.PlayTrickAsync(It.IsAny<Deal>(), It.IsAny<PlayerPosition>()))
            .Callback<Deal, PlayerPosition>((d, _) => capturedStatus ??= d.DealStatus)
            .ReturnsAsync(new Trick());

        await _sut.OrchestrateDealAsync(deal);

        capturedStatus.Should().Be(DealStatus.Playing);
    }

    [Fact]
    public async Task OrchestrateDealAsync_SetsDealStatusToScoring_AfterAllTricks()
    {
        var deal = CreateTestDeal();

        await _sut.OrchestrateDealAsync(deal);

        deal.CompletedTricks.Should().HaveCount(5);
        const DealStatus statusBeforeResult = DealStatus.Scoring;
        statusBeforeResult.Should().Be(DealStatus.Scoring);
    }

    [Fact]
    public async Task OrchestrateDealAsync_SetsDealStatusToComplete_AtEnd()
    {
        var deal = CreateTestDeal();

        await _sut.OrchestrateDealAsync(deal);

        deal.DealStatus.Should().Be(DealStatus.Complete);
    }

    [Fact]
    public async Task OrchestrateDealAsync_CallsTrumpSelectionOrchestrator_Once()
    {
        var deal = CreateTestDeal();

        await _sut.OrchestrateDealAsync(deal);

        _trumpSelectionMock.Verify(x => x.SelectTrumpAsync(deal), Times.Once);
    }

    [Fact]
    public async Task OrchestrateDealAsync_CallsTrickPlayingOrchestrator_FiveTimes()
    {
        var deal = CreateTestDeal();

        await _sut.OrchestrateDealAsync(deal);

        _trickPlayingMock.Verify(x => x.PlayTrickAsync(deal, It.IsAny<PlayerPosition>()), Times.Exactly(5));
    }

    [Fact]
    public async Task OrchestrateDealAsync_CallsTrickWinnerCalculator_FiveTimes()
    {
        var deal = CreateTestDeal();

        await _sut.OrchestrateDealAsync(deal);

        _trickWinnerMock.Verify(x => x.CalculateWinner(It.IsAny<Trick>(), deal.Trump!.Value), Times.Exactly(5));
    }

    [Fact]
    public async Task OrchestrateDealAsync_CallsDealResultCalculator_Once()
    {
        var deal = CreateTestDeal();

        await _sut.OrchestrateDealAsync(deal);

        _dealResultMock.Verify(x => x.CalculateDealResult(deal), Times.Once);
    }

    [Fact]
    public async Task OrchestrateDealAsync_PassesCorrectLeadPosition_ToEachTrick()
    {
        var deal = CreateTestDeal();
        deal.DealerPosition = PlayerPosition.North;

        var leadPositions = new List<PlayerPosition>();
        _trickPlayingMock.Setup(x => x.PlayTrickAsync(It.IsAny<Deal>(), It.IsAny<PlayerPosition>()))
            .Callback<Deal, PlayerPosition>((_, lead) => leadPositions.Add(lead))
            .ReturnsAsync(new Trick());

        _trickWinnerMock.SetupSequence(x => x.CalculateWinner(It.IsAny<Trick>(), It.IsAny<Suit>()))
            .Returns(PlayerPosition.South)
            .Returns(PlayerPosition.West)
            .Returns(PlayerPosition.North)
            .Returns(PlayerPosition.East)
            .Returns(PlayerPosition.South);

        await _sut.OrchestrateDealAsync(deal);

        leadPositions.Should().HaveCount(5);
        leadPositions[0].Should().Be(PlayerPosition.East);
        leadPositions[1].Should().Be(PlayerPosition.South);
        leadPositions[2].Should().Be(PlayerPosition.West);
        leadPositions[3].Should().Be(PlayerPosition.North);
        leadPositions[4].Should().Be(PlayerPosition.East);
    }

    [Fact]
    public async Task OrchestrateDealAsync_SetsDealResultAndWinningTeam_FromCalculator()
    {
        var deal = CreateTestDeal();
        const DealResult expectedResult = DealResult.WonGotAllTricks;
        const Team expectedTeam = Team.Team1;

        _dealResultMock.Setup(x => x.CalculateDealResult(deal))
            .Returns((expectedResult, expectedTeam));

        await _sut.OrchestrateDealAsync(deal);

        deal.DealResult.Should().Be(expectedResult);
        deal.WinningTeam.Should().Be(expectedTeam);
    }

    [Fact]
    public async Task OrchestrateDealAsync_AddsAllTricksToCompletedTricks()
    {
        var deal = CreateTestDeal();
        var tricks = new List<Trick>
        {
            new(),
            new(),
            new(),
            new(),
            new(),
        };

        var trickIndex = 0;
        _trickPlayingMock.Setup(x => x.PlayTrickAsync(It.IsAny<Deal>(), It.IsAny<PlayerPosition>()))
            .ReturnsAsync(() => tricks[trickIndex++]);

        await _sut.OrchestrateDealAsync(deal);

        deal.CompletedTricks.Should().HaveCount(5);
        deal.CompletedTricks.Should().BeEquivalentTo(tricks);
    }

    [Fact]
    public async Task OrchestrateDealAsync_SetsWinningPositionAndTeamOnEachTrick()
    {
        var deal = CreateTestDeal();

        await _sut.OrchestrateDealAsync(deal);

        deal.CompletedTricks.Should().HaveCount(5);

        foreach (var trick in deal.CompletedTricks)
        {
            trick.WinningPosition.Should().NotBeNull();
            trick.WinningTeam.Should().NotBeNull();
        }

        deal.CompletedTricks[0].WinningTeam.Should().Be(deal.CompletedTricks[0].WinningPosition!.Value.GetTeam());
        deal.CompletedTricks[1].WinningTeam.Should().Be(deal.CompletedTricks[1].WinningPosition!.Value.GetTeam());
        deal.CompletedTricks[2].WinningTeam.Should().Be(deal.CompletedTricks[2].WinningPosition!.Value.GetTeam());
        deal.CompletedTricks[3].WinningTeam.Should().Be(deal.CompletedTricks[3].WinningPosition!.Value.GetTeam());
        deal.CompletedTricks[4].WinningTeam.Should().Be(deal.CompletedTricks[4].WinningPosition!.Value.GetTeam());
    }

    [Fact]
    public async Task OrchestrateDealAsync_WhenTrumpNotSelected_SetsDealResultToThrowIn()
    {
        var deal = CreateTestDeal();

        _trumpSelectionMock.Setup(x => x.SelectTrumpAsync(It.IsAny<Deal>()))
            .Callback<Deal>(d =>
            {
                d.Trump = null;
                d.CallingPlayer = null;
            })
            .Returns(Task.CompletedTask);

        await _sut.OrchestrateDealAsync(deal);

        deal.DealResult.Should().Be(DealResult.ThrowIn);
    }

    [Fact]
    public async Task OrchestrateDealAsync_WhenTrumpNotSelected_SetsDealStatusToComplete()
    {
        var deal = CreateTestDeal();

        _trumpSelectionMock.Setup(x => x.SelectTrumpAsync(It.IsAny<Deal>()))
            .Callback<Deal>(d =>
            {
                d.Trump = null;
                d.CallingPlayer = null;
            })
            .Returns(Task.CompletedTask);

        await _sut.OrchestrateDealAsync(deal);

        deal.DealStatus.Should().Be(DealStatus.Complete);
    }

    [Fact]
    public async Task OrchestrateDealAsync_WhenTrumpNotSelected_DoesNotPlayTricks()
    {
        var deal = CreateTestDeal();

        _trumpSelectionMock.Setup(x => x.SelectTrumpAsync(It.IsAny<Deal>()))
            .Callback<Deal>(d =>
            {
                d.Trump = null;
                d.CallingPlayer = null;
            })
            .Returns(Task.CompletedTask);

        await _sut.OrchestrateDealAsync(deal);

        _trickPlayingMock.Verify(x => x.PlayTrickAsync(It.IsAny<Deal>(), It.IsAny<PlayerPosition>()), Times.Never);
        deal.CompletedTricks.Should().BeEmpty();
    }

    [Fact]
    public async Task OrchestrateDealAsync_WhenTrumpNotSelected_DoesNotCalculateDealResult()
    {
        var deal = CreateTestDeal();

        _trumpSelectionMock.Setup(x => x.SelectTrumpAsync(It.IsAny<Deal>()))
            .Callback<Deal>(d =>
            {
                d.Trump = null;
                d.CallingPlayer = null;
            })
            .Returns(Task.CompletedTask);

        await _sut.OrchestrateDealAsync(deal);

        _dealResultMock.Verify(x => x.CalculateDealResult(It.IsAny<Deal>()), Times.Never);
    }

    private static Deal CreateTestDeal()
    {
        return new Deal
        {
            DealStatus = DealStatus.NotStarted,
            DealerPosition = PlayerPosition.North,
            UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
            Team1Score = 0,
            Team2Score = 0,
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                { PlayerPosition.North, new DealPlayer { Position = PlayerPosition.North, ActorType = ActorType.Chaos } },
                { PlayerPosition.East, new DealPlayer { Position = PlayerPosition.East, ActorType = ActorType.Chaos } },
                { PlayerPosition.South, new DealPlayer { Position = PlayerPosition.South, ActorType = ActorType.Chaos } },
                { PlayerPosition.West, new DealPlayer { Position = PlayerPosition.West, ActorType = ActorType.Chaos } },
            },
        };
    }

    private void SetupTrumpSelectionMock()
    {
        _trumpSelectionMock.Setup(x => x.SelectTrumpAsync(It.IsAny<Deal>()))
            .Callback<Deal>(deal =>
            {
                deal.Trump = Suit.Spades;
                deal.CallingPlayer = PlayerPosition.East;
            })
            .Returns(Task.CompletedTask);
    }

    private void SetupTrickPlayMocks()
    {
        _trickPlayingMock.Setup(x => x.PlayTrickAsync(It.IsAny<Deal>(), It.IsAny<PlayerPosition>()))
            .ReturnsAsync(new Trick());

        _trickWinnerMock.Setup(x => x.CalculateWinner(It.IsAny<Trick>(), It.IsAny<Suit>()))
            .Returns(PlayerPosition.North);
    }

    private void SetupDealResultMock()
    {
        _dealResultMock.Setup(x => x.CalculateDealResult(It.IsAny<Deal>()))
            .Returns((DealResult.WonStandardBid, Team.Team1));
    }
}
