using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerBots;

namespace NemesisEuchre.GameEngine.Tests;

public class GameFactoryTests
{
    [Fact]
    public async Task CreateGameAsync_WithValidOptions_ReturnsGameWithFourPlayers()
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions();
        var game = await gameInitializer.CreateGameAsync(gameOptions);

        game.Should().NotBeNull();
        game.Players.Should().HaveCount(4);
    }

    [Fact]
    public async Task CreateGameAsync_WithDefaultOptions_SetsWinningScoreToDefaultValue()
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions();
        var game = await gameInitializer.CreateGameAsync(gameOptions);

        game.WinningScore.Should().Be(10);
    }

    [Fact]
    public async Task CreateGameAsync_WithCustomWinningScore_SetsWinningScoreToSpecifiedValue()
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions { WinningScore = 15 };
        var game = await gameInitializer.CreateGameAsync(gameOptions);

        game.WinningScore.Should().Be(15);
    }

    [Fact]
    public Task CreateGameAsync_WithNullTeam1BotTypes_ThrowsArgumentNullException()
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions { Team1BotTypes = null! };

        var act = async () => await gameInitializer.CreateGameAsync(gameOptions);

        return act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public Task CreateGameAsync_WithNullTeam2BotTypes_ThrowsArgumentNullException()
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions { Team2BotTypes = null! };

        var act = async () => await gameInitializer.CreateGameAsync(gameOptions);

        return act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    public Task CreateGameAsync_WithInvalidTeam1BotTypesLength_ThrowsArgumentException(int length)
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions
        {
            Team1BotTypes = new BotType[length],
        };

        var act = async () => await gameInitializer.CreateGameAsync(gameOptions);

        return act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("gameOptions")
            .WithMessage("Team1BotTypes must contain exactly 2 bot types.*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    public Task CreateGameAsync_WithInvalidTeam2BotTypesLength_ThrowsArgumentException(int length)
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions
        {
            Team2BotTypes = new BotType[length],
        };

        var act = async () => await gameInitializer.CreateGameAsync(gameOptions);

        return act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("gameOptions")
            .WithMessage("Team2BotTypes must contain exactly 2 bot types.*");
    }

    [Fact]
    public async Task CreateGameAsync_WithValidBotTypes_AssignsBotTypesToPlayers()
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions
        {
            Team1BotTypes = [BotType.Chaos, BotType.Chaos],
            Team2BotTypes = [BotType.Chaos, BotType.Chaos],
        };

        var game = await gameInitializer.CreateGameAsync(gameOptions);

        game.Players[PlayerPosition.North].BotType.Should().Be(BotType.Chaos);
        game.Players[PlayerPosition.South].BotType.Should().Be(BotType.Chaos);
        game.Players[PlayerPosition.East].BotType.Should().Be(BotType.Chaos);
        game.Players[PlayerPosition.West].BotType.Should().Be(BotType.Chaos);
    }

    [Fact]
    public async Task CreateGameAsync_WithDifferentBotTypes_AssignsCorrectBotTypesToEachPlayer()
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions
        {
            Team1BotTypes = [BotType.Chaos, BotType.Chaos],
            Team2BotTypes = [BotType.Chaos, BotType.Chaos],
        };

        var game = await gameInitializer.CreateGameAsync(gameOptions);

        game.Players[PlayerPosition.North].BotType.Should().Be(gameOptions.Team1BotTypes[0]);
        game.Players[PlayerPosition.South].BotType.Should().Be(gameOptions.Team1BotTypes[1]);
        game.Players[PlayerPosition.East].BotType.Should().Be(gameOptions.Team2BotTypes[0]);
        game.Players[PlayerPosition.West].BotType.Should().Be(gameOptions.Team2BotTypes[1]);
    }
}
