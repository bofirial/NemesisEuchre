using FluentAssertions;

using Microsoft.Extensions.Options;

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
    private readonly IOptions<GameOptions> _gameOptions;
    private readonly GameOrchestrator _sut;

    public GameOrchestratorTests()
    {
        _gameFactoryMock = new Mock<IGameFactory>();
        _dealFactoryMock = new Mock<IDealFactory>();
        _dealOrchestratorMock = new Mock<IDealOrchestrator>();
        _gameScoreUpdaterMock = new Mock<IGameScoreUpdater>();
        _gameOptions = Options.Create(new GameOptions { WinningScore = 10 });

        _dealOrchestratorMock.Setup(x => x.OrchestrateDealAsync(It.IsAny<Deal>()))
            .Returns(Task.CompletedTask);

        _sut = new GameOrchestrator(
            _gameFactoryMock.Object,
            _dealFactoryMock.Object,
            _dealOrchestratorMock.Object,
            _gameScoreUpdaterMock.Object,
            _gameOptions);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public Task OrchestrateGameAsync_WithInvalidWinningScore_ThrowsArgumentOutOfRangeException(short winningScore)
    {
        var gameOptions = Options.Create(new GameOptions { WinningScore = winningScore });
        var sut = new GameOrchestrator(
            _gameFactoryMock.Object,
            _dealFactoryMock.Object,
            _dealOrchestratorMock.Object,
            _gameScoreUpdaterMock.Object,
            gameOptions);

        var act = sut.OrchestrateGameAsync;

        return act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("gameOptions.Value.WinningScore");
    }

    [Fact]
    public async Task OrchestrateGameAsync_WithValidOptions_CreatesGameAndSetsStatusToPlaying()
    {
        var game = new Game();

        _gameFactoryMock.Setup(x => x.CreateGameAsync())
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

        _gameFactoryMock.Setup(x => x.CreateGameAsync())
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

        _gameFactoryMock.Setup(x => x.CreateGameAsync())
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

        _gameFactoryMock.Setup(x => x.CreateGameAsync())
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(game, null))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) =>
            {
                g.Team1Score = 10;
                g.Team2Score = 10;
            });

        var act = _sut.OrchestrateGameAsync;

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

        _gameFactoryMock.Setup(x => x.CreateGameAsync())
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

        _gameFactoryMock.Setup(x => x.CreateGameAsync())
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

        _gameFactoryMock.Setup(x => x.CreateGameAsync())
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

        _gameFactoryMock.Setup(x => x.CreateGameAsync())
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

        _gameFactoryMock.Setup(x => x.CreateGameAsync())
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
        var gameOptions = Options.Create(new GameOptions { WinningScore = 5 });
        var sut = new GameOrchestrator(
            _gameFactoryMock.Object,
            _dealFactoryMock.Object,
            _dealOrchestratorMock.Object,
            _gameScoreUpdaterMock.Object,
            gameOptions);

        var game = new Game();
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync())
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

        _gameFactoryMock.Setup(x => x.CreateGameAsync())
            .ReturnsAsync(game);

        SetupGameToEndImmediately(game);

        var result = await _sut.OrchestrateGameAsync();

        result.Should().BeSameAs(game);
    }

    [Fact]
    public Task OrchestrateGameAsync_WhenGameDoesNotCompleteWithin100Deals_ThrowsInvalidOperationException()
    {
        var gameOptions = Options.Create(new GameOptions { WinningScore = 200 });
        var sut = new GameOrchestrator(
            _gameFactoryMock.Object,
            _dealFactoryMock.Object,
            _dealOrchestratorMock.Object,
            _gameScoreUpdaterMock.Object,
            gameOptions);

        var game = new Game();
        var deal = new Deal();

        _gameFactoryMock.Setup(x => x.CreateGameAsync())
            .ReturnsAsync(game);

        _dealFactoryMock.Setup(x => x.CreateDealAsync(It.IsAny<Game>(), It.IsAny<Deal?>()))
            .ReturnsAsync(deal);

        _gameScoreUpdaterMock.Setup(x => x.UpdateGameScoreAsync(game, deal))
            .Callback<Game, Deal>((g, _) => g.Team1Score = (short)(g.Team1Score + 1));

        var act = sut.OrchestrateGameAsync;

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Game did not complete within the maximum number of deals allowed (100)");
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
