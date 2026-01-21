using FluentAssertions;

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
    }
}
