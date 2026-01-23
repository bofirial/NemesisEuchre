using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

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
    public Task CreateGameAsync_WithNullTeam1ActorTypes_ThrowsArgumentNullException()
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions { Team1ActorTypes = null! };

        var act = async () => await gameInitializer.CreateGameAsync(gameOptions);

        return act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public Task CreateGameAsync_WithNullTeam2ActorTypes_ThrowsArgumentNullException()
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions { Team2ActorTypes = null! };

        var act = async () => await gameInitializer.CreateGameAsync(gameOptions);

        return act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    public Task CreateGameAsync_WithInvalidTeam1ActorTypesLength_ThrowsArgumentException(int length)
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions
        {
            Team1ActorTypes = new ActorType[length],
        };

        var act = async () => await gameInitializer.CreateGameAsync(gameOptions);

        return act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("gameOptions")
            .WithMessage("Team1ActorTypes must contain exactly 2 actor types.*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    public Task CreateGameAsync_WithInvalidTeam2ActorTypesLength_ThrowsArgumentException(int length)
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions
        {
            Team2ActorTypes = new ActorType[length],
        };

        var act = async () => await gameInitializer.CreateGameAsync(gameOptions);

        return act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("gameOptions")
            .WithMessage("Team2ActorTypes must contain exactly 2 actor types.*");
    }

    [Fact]
    public async Task CreateGameAsync_WithValidActorTypes_AssignsActorTypesToPlayers()
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions
        {
            Team1ActorTypes = [ActorType.Chaos, ActorType.Chaos],
            Team2ActorTypes = [ActorType.Chaos, ActorType.Chaos],
        };

        var game = await gameInitializer.CreateGameAsync(gameOptions);

        game.Players[PlayerPosition.North].ActorType.Should().Be(ActorType.Chaos);
        game.Players[PlayerPosition.South].ActorType.Should().Be(ActorType.Chaos);
        game.Players[PlayerPosition.East].ActorType.Should().Be(ActorType.Chaos);
        game.Players[PlayerPosition.West].ActorType.Should().Be(ActorType.Chaos);
    }

    [Fact]
    public async Task CreateGameAsync_WithDifferentActorTypes_AssignsCorrectActorTypesToEachPlayer()
    {
        var gameInitializer = new GameFactory();

        var gameOptions = new GameOptions
        {
            Team1ActorTypes = [ActorType.Chaos, ActorType.Chaos],
            Team2ActorTypes = [ActorType.Chaos, ActorType.Chaos],
        };

        var game = await gameInitializer.CreateGameAsync(gameOptions);

        game.Players[PlayerPosition.North].ActorType.Should().Be(gameOptions.Team1ActorTypes[0]);
        game.Players[PlayerPosition.South].ActorType.Should().Be(gameOptions.Team1ActorTypes[1]);
        game.Players[PlayerPosition.East].ActorType.Should().Be(gameOptions.Team2ActorTypes[0]);
        game.Players[PlayerPosition.West].ActorType.Should().Be(gameOptions.Team2ActorTypes[1]);
    }
}
