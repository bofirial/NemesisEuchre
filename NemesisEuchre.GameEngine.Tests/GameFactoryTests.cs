using FluentAssertions;

using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests;

public class GameFactoryTests
{
    [Fact]
    public async Task CreateGameAsyncShouldReturnAGame()
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions();
        var game = await gameInitializer.CreateGameAsync(gameOptions);

        game.Should().NotBeNull();
        game.Players.Should().HaveCount(4);
    }

    [Fact]
    public async Task CreateGameAsyncShouldSetDefaultWinningScore()
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions();
        var game = await gameInitializer.CreateGameAsync(gameOptions);

        game.WinningScore.Should().Be(10);
    }

    [Fact]
    public async Task CreateGameAsyncShouldSetCustomWinningScore()
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions { WinningScore = 15 };
        var game = await gameInitializer.CreateGameAsync(gameOptions);

        game.WinningScore.Should().Be(15);
    }
}
