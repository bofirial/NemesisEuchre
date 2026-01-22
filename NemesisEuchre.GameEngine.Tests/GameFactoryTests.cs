using FluentAssertions;

using NemesisEuchre.GameEngine.Models;

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
}
