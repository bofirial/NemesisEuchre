using FluentAssertions;

using Moq;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Options;

using MsOptions = Microsoft.Extensions.Options;

namespace NemesisEuchre.GameEngine.Tests;

public class GameOrchestratorTests
{
    private readonly Mock<IGameFactory> _gameFactoryMock;
    private readonly Mock<IDealFactory> _dealFactoryMock;
    private readonly Mock<IDealOrchestrator> _dealOrchestratorMock;
    private readonly Mock<IGameScoreUpdater> _gameScoreUpdaterMock;
    private readonly Mock<IGameWinnerCalculator> _gameWinnerCalculatorMock;
    private readonly MsOptions.IOptions<GameOptions> _gameOptions;
    private readonly GameOrchestrator _sut;

    public GameOrchestratorTests()
    {
        _gameFactoryMock = new Mock<IGameFactory>();
        _dealFactoryMock = new Mock<IDealFactory>();
        _dealOrchestratorMock = new Mock<IDealOrchestrator>();
        _gameScoreUpdaterMock = new Mock<IGameScoreUpdater>();
        _gameWinnerCalculatorMock = new Mock<IGameWinnerCalculator>();
        _gameOptions = MsOptions.Options.Create(new GameOptions { WinningScore = 10 });

        _dealOrchestratorMock.Setup(x => x.OrchestrateDealAsync(It.IsAny<Deal>()))
            .Returns(Task.CompletedTask);

        _gameWinnerCalculatorMock.Setup(x => x.DetermineWinner(It.IsAny<Game>()))
            .Returns<Game>(game => game.Team1Score > game.Team2Score ? Team.Team1 : Team.Team2);

        _sut = new GameOrchestrator(
            _gameFactoryMock.Object,
            _dealFactoryMock.Object,
            _dealOrchestratorMock.Object,
            _gameScoreUpdaterMock.Object,
            _gameWinnerCalculatorMock.Object,
            _gameOptions);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public Task OrchestrateGameAsync_WithInvalidWinningScore_ThrowsArgumentOutOfRangeException(short winningScore)
    {
        var gameOptions = MsOptions.Options.Create(new GameOptions { WinningScore = winningScore });
        var sut = new GameOrchestrator(
            _gameFactoryMock.Object,
            _dealFactoryMock.Object,
            _dealOrchestratorMock.Object,
            _gameScoreUpdaterMock.Object,
            _gameWinnerCalculatorMock.Object,
            gameOptions);

        var act = async () => await sut.OrchestrateGameAsync();

        return act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("gameOptions.Value.WinningScore");
    }

    [Fact]
    public async Task OrchestrateGameAsync_WithValidOptions_CreatesGameAndSetsStatusToPlaying()
    {
        var game = new Game();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>()))
            .ReturnsAsync(game);

        SetupGameToEndImmediately(game);

        await _sut.OrchestrateGameAsync();

        game.GameStatus.Should().Be(GameStatus.Complete);
    }

    [Fact]
    public async Task OrchestrateGameAsync_WhenTeam1Wins_SetsWinningTeamToTeam1()
    {
        var game = new Game();
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>()))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) => g.Team1Score = 10);

        var result = await _sut.OrchestrateGameAsync();

        result.WinningTeam.Should().Be(Team.Team1);
        result.GameStatus.Should().Be(GameStatus.Complete);
    }

    [Fact]
    public async Task OrchestrateGameAsync_WhenTeam2Wins_SetsWinningTeamToTeam2()
    {
        var game = new Game();
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>()))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) => g.Team2Score = 10);

        var result = await _sut.OrchestrateGameAsync();

        result.WinningTeam.Should().Be(Team.Team2);
        result.GameStatus.Should().Be(GameStatus.Complete);
    }

    [Fact]
    public Task OrchestrateGameAsync_WhenGameEndsinTie_ThrowsInvalidOperationException()
    {
        var game = new Game();
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>()))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) =>
            {
                g.Team1Score = 10;
                g.Team2Score = 10;
            });

        _gameWinnerCalculatorMock.Setup(x => x.DetermineWinner(It.IsAny<Game>()))
            .Throws(new InvalidOperationException("Game ended in a tie (10-10), which should not occur in Euchre"));

        var act = async () => await _sut.OrchestrateGameAsync();

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Game ended in a tie (10-10), which should not occur in Euchre");
    }

    [Fact]
    public async Task OrchestrateGameAsync_PlaysMultipleDealsUntilWinningScore()
    {
        var game = new Game();
        var deals = new List<Deal>
        {
            new(),
            new(),
            new(),
            new(),
        };

        _gameFactoryMock.Setup(x => x.CreateGameAsync(It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>()))
            .ReturnsAsync(game);

        var dealIndex = 0;
        _dealFactoryMock.Setup(x => x.CreateDealAsync(It.IsAny<Game>(), It.IsAny<Deal?>()))
            .ReturnsAsync(() => deals[dealIndex++]);

        var scoreIncrement = 0;
        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, It.IsAny<Deal>()))
            .Callback<Game, Deal>((g, _) =>
            {
                scoreIncrement++;
                g.Team1Score = (short)(scoreIncrement * 3);
            });

        var result = await _sut.OrchestrateGameAsync();

        result.CompletedDeals.Should().HaveCount(4);
        result.CompletedDeals.Should().BeEquivalentTo(deals);
        _dealOrchestratorMock.Verify(x => x.OrchestrateDealAsync(It.IsAny<Deal>()), Times.Exactly(4));
    }

    [Fact]
    public async Task OrchestrateGameAsync_SetsCurrentDealToNullAfterCompletion()
    {
        var game = new Game();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>()))
            .ReturnsAsync(game);

        SetupGameToEndImmediately(game);

        var result = await _sut.OrchestrateGameAsync();

        result.CurrentDeal.Should().BeNull();
    }

    [Fact]
    public async Task OrchestrateGameAsync_PassesPreviousDealToDealFactory()
    {
        var game = new Game();
        var firstDeal = new Deal();
        var secondDeal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>()))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(firstDeal);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, firstDeal))
            .ReturnsAsync(secondDeal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, firstDeal))
            .Callback<Game, Deal>((g, _) => g.Team1Score = 5);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, secondDeal))
            .Callback<Game, Deal>((g, _) => g.Team1Score = 10);

        await _sut.OrchestrateGameAsync();

        _dealFactoryMock.Verify(x => x.CreateDealAsync(game, null), Times.Once);
        _dealFactoryMock.Verify(x => x.CreateDealAsync(game, firstDeal), Times.Once);
    }

    [Fact]
    public async Task OrchestrateGameAsync_CallsDealOrchestratorForEachDeal()
    {
        var game = new Game();
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>()))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) => g.Team1Score = 10);

        await _sut.OrchestrateGameAsync();

        _dealOrchestratorMock.Verify(x => x.OrchestrateDealAsync(deal), Times.Once);
    }

    [Fact]
    public async Task OrchestrateGameAsync_CallsGameScoreUpdaterForEachDeal()
    {
        var game = new Game();
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>()))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) => g.Team1Score = 10);

        await _sut.OrchestrateGameAsync();

        _gameScoreUpdaterMock.Verify(x => x.UpdateGameScoreAsync(game, deal), Times.Once);
    }

    [Fact]
    public async Task OrchestrateGameAsync_WithCustomWinningScore_StopsAtCorrectScore()
    {
        var gameOptions = MsOptions.Options.Create(new GameOptions { WinningScore = 5 });
        var sut = new GameOrchestrator(
            _gameFactoryMock.Object,
            _dealFactoryMock.Object,
            _dealOrchestratorMock.Object,
            _gameScoreUpdaterMock.Object,
            _gameWinnerCalculatorMock.Object,
            gameOptions);

        var game = new Game();
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>()))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) => g.Team2Score = 5);

        var result = await sut.OrchestrateGameAsync();

        result.Team2Score.Should().Be(5);
        result.WinningTeam.Should().Be(Team.Team2);
        result.CompletedDeals.Should().HaveCount(1);
    }

    [Fact]
    public async Task OrchestrateGameAsync_ReturnsCompletedGame()
    {
        var game = new Game();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>()))
            .ReturnsAsync(game);

        SetupGameToEndImmediately(game);

        var result = await _sut.OrchestrateGameAsync();

        result.Should().BeSameAs(game);
    }

    [Fact]
    public Task OrchestrateGameAsync_WhenGameDoesNotCompleteWithin100Deals_ThrowsInvalidOperationException()
    {
        var gameOptions = MsOptions.Options.Create(new GameOptions { WinningScore = 200 });
        var sut = new GameOrchestrator(
            _gameFactoryMock.Object,
            _dealFactoryMock.Object,
            _dealOrchestratorMock.Object,
            _gameScoreUpdaterMock.Object,
            _gameWinnerCalculatorMock.Object,
            gameOptions);

        var game = new Game();
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>()))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(It.IsAny<Game>(), It.IsAny<Deal?>()))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) => g.Team1Score = (short)(g.Team1Score + 1));

        var act = async () => await sut.OrchestrateGameAsync();

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Game did not complete within the maximum number of deals allowed (100)");
    }

    [Theory]
    [InlineData(10, 0)]
    [InlineData(0, 10)]
    public async Task OrchestrateGameAsync_AfterDealCompletion_RecordsGameScoresOnDeal(short team1Score, short team2Score)
    {
        var game = new Game();
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>()))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) =>
            {
                g.Team1Score = team1Score;
                g.Team2Score = team2Score;
            });

        var result = await _sut.OrchestrateGameAsync();

        result.CompletedDeals.Should().HaveCount(1);
        result.CompletedDeals[0].Team1Score.Should().Be(team1Score);
        result.CompletedDeals[0].Team2Score.Should().Be(team2Score);
    }

    [Fact]
    public async Task OrchestrateGameAsync_WithMultipleDeals_RecordsCumulativeScoresOnEachDeal()
    {
        var game = new Game();
        var deals = new List<Deal>
        {
            new(),
            new(),
            new(),
            new(),
        };

        _gameFactoryMock.Setup(x => x.CreateGameAsync(It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>()))
            .ReturnsAsync(game);

        var dealIndex = 0;
        _dealFactoryMock.Setup(x => x.CreateDealAsync(It.IsAny<Game>(), It.IsAny<Deal?>()))
            .ReturnsAsync(() => deals[dealIndex++]);

        var scoreIncrement = 0;
        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, It.IsAny<Deal>()))
            .Callback<Game, Deal>((g, _) =>
            {
                scoreIncrement++;
                g.Team1Score = (short)(scoreIncrement * 3);
            });

        var result = await _sut.OrchestrateGameAsync();

        result.CompletedDeals.Should().HaveCount(4);
        result.CompletedDeals[0].Team1Score.Should().Be(3);
        result.CompletedDeals[0].Team2Score.Should().Be(0);
        result.CompletedDeals[1].Team1Score.Should().Be(6);
        result.CompletedDeals[1].Team2Score.Should().Be(0);
        result.CompletedDeals[2].Team1Score.Should().Be(9);
        result.CompletedDeals[2].Team2Score.Should().Be(0);
        result.CompletedDeals[3].Team1Score.Should().Be(12);
        result.CompletedDeals[3].Team2Score.Should().Be(0);
    }

    [Fact]
    public async Task OrchestrateGameAsync_WithBothTeamsScoring_RecordsCorrectScoresOnEachDeal()
    {
        var game = new Game();
        var deals = new List<Deal>
        {
            new(),
            new(),
            new(),
        };

        _gameFactoryMock.Setup(x => x.CreateGameAsync(It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>()))
            .ReturnsAsync(game);

        var dealIndex = 0;
        _dealFactoryMock.Setup(x => x.CreateDealAsync(It.IsAny<Game>(), It.IsAny<Deal?>()))
            .ReturnsAsync(() => deals[dealIndex++]);

        var callCount = 0;
        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, It.IsAny<Deal>()))
            .Callback<Game, Deal>((g, _) =>
            {
                callCount++;
                if (callCount == 1)
                {
                    g.Team1Score = 2;
                    g.Team2Score = 0;
                }
                else if (callCount == 2)
                {
                    g.Team1Score = 2;
                    g.Team2Score = 4;
                }
                else if (callCount == 3)
                {
                    g.Team1Score = 6;
                    g.Team2Score = 10;
                }
            });

        var result = await _sut.OrchestrateGameAsync();

        result.CompletedDeals.Should().HaveCount(3);
        result.CompletedDeals[0].Team1Score.Should().Be(2);
        result.CompletedDeals[0].Team2Score.Should().Be(0);
        result.CompletedDeals[1].Team1Score.Should().Be(2);
        result.CompletedDeals[1].Team2Score.Should().Be(4);
        result.CompletedDeals[2].Team1Score.Should().Be(6);
        result.CompletedDeals[2].Team2Score.Should().Be(10);
    }

    [Fact]
    public async Task OrchestrateGameAsync_WithNullActorTypes_PassesNullToGameFactory()
    {
        var game = new Game();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(null, null))
            .ReturnsAsync(game);

        SetupGameToEndImmediately(game);

        await _sut.OrchestrateGameAsync();

        _gameFactoryMock.Verify(x => x.CreateGameAsync(null, null), Times.Once);
    }

    [Fact]
    public async Task OrchestrateGameAsync_WithTeam1ActorTypes_PassesToGameFactory()
    {
        var game = new Game();
        var team1ActorTypes = new[] { ActorType.Gen1, ActorType.Gen1 };

        _gameFactoryMock.Setup(x => x.CreateGameAsync(team1ActorTypes, null))
            .ReturnsAsync(game);

        SetupGameToEndImmediately(game);

        await _sut.OrchestrateGameAsync(team1ActorTypes);

        _gameFactoryMock.Verify(x => x.CreateGameAsync(team1ActorTypes, null), Times.Once);
    }

    [Fact]
    public async Task OrchestrateGameAsync_WithTeam2ActorTypes_PassesToGameFactory()
    {
        var game = new Game();
        var team2ActorTypes = new[] { ActorType.Chaos, ActorType.Chaos };

        _gameFactoryMock.Setup(x => x.CreateGameAsync(null, team2ActorTypes))
            .ReturnsAsync(game);

        SetupGameToEndImmediately(game);

        await _sut.OrchestrateGameAsync(null, team2ActorTypes);

        _gameFactoryMock.Verify(x => x.CreateGameAsync(null, team2ActorTypes), Times.Once);
    }

    [Fact]
    public async Task OrchestrateGameAsync_WithBothTeamActorTypes_PassesToGameFactory()
    {
        var game = new Game();
        var team1ActorTypes = new[] { ActorType.Gen1, ActorType.Gen1 };
        var team2ActorTypes = new[] { ActorType.Chaos, ActorType.Chaos };

        _gameFactoryMock.Setup(x => x.CreateGameAsync(team1ActorTypes, team2ActorTypes))
            .ReturnsAsync(game);

        SetupGameToEndImmediately(game);

        await _sut.OrchestrateGameAsync(team1ActorTypes, team2ActorTypes);

        _gameFactoryMock.Verify(x => x.CreateGameAsync(team1ActorTypes, team2ActorTypes), Times.Once);
    }

    private void SetupGameToEndImmediately(Game game)
    {
        var deal = new Deal();

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) => g.Team1Score = _gameOptions.Value.WinningScore);
    }
}
