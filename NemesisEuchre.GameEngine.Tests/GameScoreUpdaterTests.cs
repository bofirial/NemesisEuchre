using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Tests.TestHelpers;

namespace NemesisEuchre.GameEngine.Tests;

public class GameScoreUpdaterTests
{
    [Fact]
    public Task UpdateGameScoreAsync_WithNullGame_ThrowsArgumentNullException()
    {
        var updater = new GameScoreUpdater();
        var deal = TestDataBuilders.CreateDeal(PlayerPosition.North, DealResult.WonStandardBid, Team.Team1);

        var act = async () => await updater.UpdateGameScoreAsync(null!, deal);

        return act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("game");
    }

    [Fact]
    public Task UpdateGameScoreAsync_WithNullDeal_ThrowsArgumentNullException()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame();

        var act = async () => await updater.UpdateGameScoreAsync(game, null!);

        return act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("deal");
    }

    [Fact]
    public Task UpdateGameScoreAsync_WithNullDealResult_ThrowsInvalidOperationException()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame();
        var deal = new Deal
        {
            DealResult = null,
            WinningTeam = Team.Team1,
            CallingPlayer = PlayerPosition.North,
        };

        var act = async () => await updater.UpdateGameScoreAsync(game, deal);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Deal must have a DealResult before scoring.");
    }

    [Fact]
    public Task UpdateGameScoreAsync_WithNullWinningTeam_ThrowsInvalidOperationException()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame();
        var deal = new Deal
        {
            DealResult = DealResult.WonStandardBid,
            WinningTeam = null,
            CallingPlayer = PlayerPosition.North,
        };

        var act = async () => await updater.UpdateGameScoreAsync(game, deal);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Deal must have a WinningTeam before scoring.");
    }

    [Fact]
    public Task UpdateGameScoreAsync_WithNullCallingPlayer_ThrowsInvalidOperationException()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame();
        var deal = new Deal
        {
            DealResult = DealResult.WonStandardBid,
            WinningTeam = Team.Team1,
            CallingPlayer = null,
        };

        var act = async () => await updater.UpdateGameScoreAsync(game, deal);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Deal must have a CallingPlayer before scoring.");
    }

    [Fact]
    public async Task UpdateGameScoreAsync_Team1WinsStandardBid_Awards1Point()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame();
        var deal = TestDataBuilders.CreateDeal(PlayerPosition.North, DealResult.WonStandardBid, Team.Team1);

        await updater.UpdateGameScoreAsync(game, deal);

        game.Team1Score.Should().Be(1);
        game.Team2Score.Should().Be(0);
    }

    [Fact]
    public async Task UpdateGameScoreAsync_Team1GetsAllTricks_Awards2Points()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame();
        var deal = TestDataBuilders.CreateDeal(PlayerPosition.North, DealResult.WonGotAllTricks, Team.Team1);

        await updater.UpdateGameScoreAsync(game, deal);

        game.Team1Score.Should().Be(2);
        game.Team2Score.Should().Be(0);
    }

    [Fact]
    public async Task UpdateGameScoreAsync_Team1OpponentsEuchred_Awards2Points()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame();
        var deal = TestDataBuilders.CreateDeal(PlayerPosition.East, DealResult.OpponentsEuchred, Team.Team1);

        await updater.UpdateGameScoreAsync(game, deal);

        game.Team1Score.Should().Be(2);
        game.Team2Score.Should().Be(0);
    }

    [Fact]
    public async Task UpdateGameScoreAsync_Team1WinsGoingAlone_Awards4Points()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame();
        var deal = TestDataBuilders.CreateDeal(PlayerPosition.North, DealResult.WonAndWentAlone, Team.Team1, isGoingAlone: true);

        await updater.UpdateGameScoreAsync(game, deal);

        game.Team1Score.Should().Be(4);
        game.Team2Score.Should().Be(0);
    }

    [Fact]
    public async Task UpdateGameScoreAsync_Team2WinsStandardBid_Awards1Point()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame();
        var deal = TestDataBuilders.CreateDeal(PlayerPosition.East, DealResult.WonStandardBid, Team.Team2);

        await updater.UpdateGameScoreAsync(game, deal);

        game.Team1Score.Should().Be(0);
        game.Team2Score.Should().Be(1);
    }

    [Fact]
    public async Task UpdateGameScoreAsync_Team2GetsAllTricks_Awards2Points()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame();
        var deal = TestDataBuilders.CreateDeal(PlayerPosition.East, DealResult.WonGotAllTricks, Team.Team2);

        await updater.UpdateGameScoreAsync(game, deal);

        game.Team1Score.Should().Be(0);
        game.Team2Score.Should().Be(2);
    }

    [Fact]
    public async Task UpdateGameScoreAsync_Team2OpponentsEuchred_Awards2Points()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame();
        var deal = TestDataBuilders.CreateDeal(PlayerPosition.North, DealResult.OpponentsEuchred, Team.Team2);

        await updater.UpdateGameScoreAsync(game, deal);

        game.Team1Score.Should().Be(0);
        game.Team2Score.Should().Be(2);
    }

    [Fact]
    public async Task UpdateGameScoreAsync_Team2WinsGoingAlone_Awards4Points()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame();
        var deal = TestDataBuilders.CreateDeal(PlayerPosition.East, DealResult.WonAndWentAlone, Team.Team2, isGoingAlone: true);

        await updater.UpdateGameScoreAsync(game, deal);

        game.Team1Score.Should().Be(0);
        game.Team2Score.Should().Be(4);
    }

    [Fact]
    public async Task UpdateGameScoreAsync_WithExistingScores_AddsToExistingScore()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame(team1Score: 5, team2Score: 3);
        var deal = TestDataBuilders.CreateDeal(PlayerPosition.North, DealResult.WonGotAllTricks, Team.Team1);

        await updater.UpdateGameScoreAsync(game, deal);

        game.Team1Score.Should().Be(7);
        game.Team2Score.Should().Be(3);
    }

    [Fact]
    public async Task UpdateGameScoreAsync_MultipleDeals_AccumulatesScoresCorrectly()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame();

        var deal1 = TestDataBuilders.CreateDeal(PlayerPosition.North, DealResult.WonStandardBid, Team.Team1);
        await updater.UpdateGameScoreAsync(game, deal1);

        var deal2 = TestDataBuilders.CreateDeal(PlayerPosition.East, DealResult.WonGotAllTricks, Team.Team2);
        await updater.UpdateGameScoreAsync(game, deal2);

        var deal3 = TestDataBuilders.CreateDeal(PlayerPosition.South, DealResult.WonAndWentAlone, Team.Team1, isGoingAlone: true);
        await updater.UpdateGameScoreAsync(game, deal3);

        game.Team1Score.Should().Be(5);
        game.Team2Score.Should().Be(2);
    }

    [Fact]
    public async Task UpdateGameScoreAsync_ScoreNearMaxValue_DoesNotOverflow()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame(team1Score: 32766);
        var deal = TestDataBuilders.CreateDeal(PlayerPosition.North, DealResult.WonStandardBid, Team.Team1);

        await updater.UpdateGameScoreAsync(game, deal);

        game.Team1Score.Should().Be(32767);
    }

    [Fact]
    public Task UpdateGameScoreAsync_WithInvalidDealResult_ThrowsArgumentOutOfRangeException()
    {
        var updater = new GameScoreUpdater();
        var game = TestDataBuilders.CreateGame();
        var deal = new Deal
        {
            DealResult = (DealResult)999,
            WinningTeam = Team.Team1,
            CallingPlayer = PlayerPosition.North,
        };

        var act = async () => await updater.UpdateGameScoreAsync(game, deal);

        return act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*Unknown DealResult*");
    }
}
