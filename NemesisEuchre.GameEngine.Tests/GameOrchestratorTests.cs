using FluentAssertions;

using Moq;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests;

public class GameOrchestratorTests
{
    private readonly Mock<IGameFactory> _gameFactoryMock;
    private readonly Mock<IDealFactory> _dealFactoryMock;
    private readonly Mock<IDealOrchestrator> _dealOrchestratorMock;
    private readonly Mock<IGameScoreUpdater> _gameScoreUpdaterMock;
    private readonly GameOrchestrator _sut;

    public GameOrchestratorTests()
    {
        _gameFactoryMock = new Mock<IGameFactory>();
        _dealFactoryMock = new Mock<IDealFactory>();
        _dealOrchestratorMock = new Mock<IDealOrchestrator>();
        _gameScoreUpdaterMock = new Mock<IGameScoreUpdater>();

        _dealOrchestratorMock.Setup(x => x.OrchestrateDealAsync(It.IsAny<Deal>(), It.IsAny<Dictionary<PlayerPosition, Player>>()))
            .Returns(Task.CompletedTask);

        _sut = new GameOrchestrator(
            _gameFactoryMock.Object,
            _dealFactoryMock.Object,
            _dealOrchestratorMock.Object,
            _gameScoreUpdaterMock.Object);
    }

    [Fact]
    public Task OrchestrateGameAsync_WithNullGameOptions_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.OrchestrateGameAsync(null!);

        return act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("gameOptions");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public Task OrchestrateGameAsync_WithInvalidWinningScore_ThrowsArgumentOutOfRangeException(short winningScore)
    {
        var gameOptions = new GameOptions { WinningScore = winningScore };

        var act = async () => await _sut.OrchestrateGameAsync(gameOptions);

        return act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("gameOptions.WinningScore");
    }

    [Fact]
    public async Task OrchestrateGameAsync_WithValidOptions_CreatesGameAndSetsStatusToPlaying()
    {
        var gameOptions = new GameOptions { WinningScore = 10 };
        var game = new Game { WinningScore = 10 };

        _gameFactoryMock.Setup(x => x.CreateGameAsync(gameOptions))
            .ReturnsAsync(game);

        SetupGameToEndImmediately(game);

        await _sut.OrchestrateGameAsync(gameOptions);

        game.GameStatus.Should().Be(GameStatus.Complete);
    }

    [Fact]
    public async Task OrchestrateGameAsync_WhenTeam1Wins_SetsWinningTeamToTeam1()
    {
        var gameOptions = new GameOptions { WinningScore = 10 };
        var game = new Game { WinningScore = 10 };
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(gameOptions))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) => g.Team1Score = 10);

        var result = await _sut.OrchestrateGameAsync(gameOptions);

        result.WinningTeam.Should().Be(Team.Team1);
        result.GameStatus.Should().Be(GameStatus.Complete);
    }

    [Fact]
    public async Task OrchestrateGameAsync_WhenTeam2Wins_SetsWinningTeamToTeam2()
    {
        var gameOptions = new GameOptions { WinningScore = 10 };
        var game = new Game { WinningScore = 10 };
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(gameOptions))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) => g.Team2Score = 10);

        var result = await _sut.OrchestrateGameAsync(gameOptions);

        result.WinningTeam.Should().Be(Team.Team2);
        result.GameStatus.Should().Be(GameStatus.Complete);
    }

    [Fact]
    public Task OrchestrateGameAsync_WhenGameEndsinTie_ThrowsInvalidOperationException()
    {
        var gameOptions = new GameOptions { WinningScore = 10 };
        var game = new Game { WinningScore = 10 };
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(gameOptions))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) =>
            {
                g.Team1Score = 10;
                g.Team2Score = 10;
            });

        var act = async () => await _sut.OrchestrateGameAsync(gameOptions);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Game ended in a tie (10-10), which should not occur in Euchre");
    }

    [Fact]
    public async Task OrchestrateGameAsync_PlaysMultipleDealsUntilWinningScore()
    {
        var gameOptions = new GameOptions { WinningScore = 10 };
        var game = new Game { WinningScore = 10 };
        var deals = new List<Deal>
        {
            new(),
            new(),
            new(),
            new(),
        };

        _gameFactoryMock.Setup(x => x.CreateGameAsync(gameOptions))
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

        var result = await _sut.OrchestrateGameAsync(gameOptions);

        result.CompletedDeals.Should().HaveCount(4);
        result.CompletedDeals.Should().BeEquivalentTo(deals);
        _dealOrchestratorMock.Verify(x => x.OrchestrateDealAsync(It.IsAny<Deal>(), It.IsAny<Dictionary<PlayerPosition, Player>>()), Times.Exactly(4));
    }

    [Fact]
    public async Task OrchestrateGameAsync_SetsCurrentDealToNullAfterCompletion()
    {
        var gameOptions = new GameOptions { WinningScore = 10 };
        var game = new Game { WinningScore = 10 };

        _gameFactoryMock.Setup(x => x.CreateGameAsync(gameOptions))
            .ReturnsAsync(game);

        SetupGameToEndImmediately(game);

        var result = await _sut.OrchestrateGameAsync(gameOptions);

        result.CurrentDeal.Should().BeNull();
    }

    [Fact]
    public async Task OrchestrateGameAsync_PassesPreviousDealToDealFactory()
    {
        var gameOptions = new GameOptions { WinningScore = 10 };
        var game = new Game { WinningScore = 10 };
        var firstDeal = new Deal();
        var secondDeal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(gameOptions))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(firstDeal);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, firstDeal))
            .ReturnsAsync(secondDeal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, firstDeal))
            .Callback<Game, Deal>((g, _) => g.Team1Score = 5);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, secondDeal))
            .Callback<Game, Deal>((g, _) => g.Team1Score = 10);

        await _sut.OrchestrateGameAsync(gameOptions);

        _dealFactoryMock.Verify(x => x.CreateDealAsync(game, null), Times.Once);
        _dealFactoryMock.Verify(x => x.CreateDealAsync(game, firstDeal), Times.Once);
    }

    [Fact]
    public async Task OrchestrateGameAsync_CallsDealOrchestratorForEachDeal()
    {
        var gameOptions = new GameOptions { WinningScore = 10 };
        var game = new Game { WinningScore = 10 };
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(gameOptions))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) => g.Team1Score = 10);

        await _sut.OrchestrateGameAsync(gameOptions);

        _dealOrchestratorMock.Verify(x => x.OrchestrateDealAsync(deal, It.IsAny<Dictionary<PlayerPosition, Player>>()), Times.Once);
    }

    [Fact]
    public async Task OrchestrateGameAsync_CallsGameScoreUpdaterForEachDeal()
    {
        var gameOptions = new GameOptions { WinningScore = 10 };
        var game = new Game { WinningScore = 10 };
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(gameOptions))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) => g.Team1Score = 10);

        await _sut.OrchestrateGameAsync(gameOptions);

        _gameScoreUpdaterMock.Verify(x => x.UpdateGameScoreAsync(game, deal), Times.Once);
    }

    [Fact]
    public async Task OrchestrateGameAsync_WithCustomWinningScore_StopsAtCorrectScore()
    {
        var gameOptions = new GameOptions { WinningScore = 5 };
        var game = new Game { WinningScore = 5 };
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(gameOptions))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) => g.Team2Score = 5);

        var result = await _sut.OrchestrateGameAsync(gameOptions);

        result.Team2Score.Should().Be(5);
        result.WinningTeam.Should().Be(Team.Team2);
        result.CompletedDeals.Should().HaveCount(1);
    }

    [Fact]
    public async Task OrchestrateGameAsync_ReturnsCompletedGame()
    {
        var gameOptions = new GameOptions { WinningScore = 10 };
        var game = new Game { WinningScore = 10 };

        _gameFactoryMock.Setup(x => x.CreateGameAsync(gameOptions))
            .ReturnsAsync(game);

        SetupGameToEndImmediately(game);

        var result = await _sut.OrchestrateGameAsync(gameOptions);

        result.Should().BeSameAs(game);
    }

    [Fact]
    public Task OrchestrateGameAsync_WhenGameDoesNotCompleteWithin100Deals_ThrowsInvalidOperationException()
    {
        var gameOptions = new GameOptions { WinningScore = 200 };
        var game = new Game { WinningScore = 200 };
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync(gameOptions))
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(It.IsAny<Game>(), It.IsAny<Deal?>()))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) => g.Team1Score = (short)(g.Team1Score + 1));

        var act = async () => await _sut.OrchestrateGameAsync(gameOptions);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Game did not complete within the maximum number of deals allowed (100)");
    }

    private void SetupGameToEndImmediately(Game game)
    {
        var deal = new Deal();

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) => g.Team1Score = game.WinningScore);
    }
}
