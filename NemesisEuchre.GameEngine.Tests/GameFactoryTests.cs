using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Options;

using MsOptions = Microsoft.Extensions.Options;

namespace NemesisEuchre.GameEngine.Tests;

public class GameFactoryTests
{
    [Fact]
    public async Task CreateGameAsync_WithValidOptions_ReturnsGameWithFourPlayers()
    {
        var gameOptions = MsOptions.Options.Create(new GameOptions());
        var gameInitializer = new GameFactory(gameOptions);

        var game = await gameInitializer.CreateGameAsync();

        game.Should().NotBeNull();
        game.Players.Should().HaveCount(4);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    public Task CreateGameAsync_WithInvalidTeam1ActorsLength_ThrowsArgumentException(int length)
    {
        var gameOptions = MsOptions.Options.Create(new GameOptions());
        var gameInitializer = new GameFactory(gameOptions);

        var act = async () => await gameInitializer.CreateGameAsync(team1Actors: new Actor[length]);

        return act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("team1Actors")
            .WithMessage("Team1ActorTypes must contain exactly 2 actor types.*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    public Task CreateGameAsync_WithInvalidTeam2ActorsLength_ThrowsArgumentException(int length)
    {
        var gameOptions = MsOptions.Options.Create(new GameOptions());
        var gameInitializer = new GameFactory(gameOptions);

        var act = async () => await gameInitializer.CreateGameAsync(team2Actors: new Actor[length]);

        return act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("team2Actors")
            .WithMessage("Team2ActorTypes must contain exactly 2 actor types.*");
    }

    [Fact]
    public async Task CreateGameAsync_WithValidActorTypes_AssignsActorTypesToPlayers()
    {
        var gameOptions = MsOptions.Options.Create(new GameOptions
        {
            Team1Actors = [new Actor(ActorType.Chaos, null), new Actor(ActorType.Chaos, null)],
            Team2Actors = [new Actor(ActorType.Chaos, null), new Actor(ActorType.Chaos, null)],
        });
        var gameInitializer = new GameFactory(gameOptions);

        var game = await gameInitializer.CreateGameAsync();

        game.Players[PlayerPosition.North].Actor.ActorType.Should().Be(ActorType.Chaos);
        game.Players[PlayerPosition.South].Actor.ActorType.Should().Be(ActorType.Chaos);
        game.Players[PlayerPosition.East].Actor.ActorType.Should().Be(ActorType.Chaos);
        game.Players[PlayerPosition.West].Actor.ActorType.Should().Be(ActorType.Chaos);
    }

    [Fact]
    public async Task CreateGameAsync_WithDifferentActorTypes_AssignsCorrectActorTypesToEachPlayer()
    {
        var gameOptionsValue = new GameOptions
        {
            Team1Actors = [new Actor(ActorType.Chaos, null), new Actor(ActorType.Chaos, null)],
            Team2Actors = [new Actor(ActorType.Chaos, null), new Actor(ActorType.Chaos, null)],
        };
        var gameOptions = MsOptions.Options.Create(gameOptionsValue);
        var gameInitializer = new GameFactory(gameOptions);

        var game = await gameInitializer.CreateGameAsync();

        game.Players[PlayerPosition.North].Actor.ActorType.Should().Be(gameOptionsValue.Team1Actors[0].ActorType);
        game.Players[PlayerPosition.South].Actor.ActorType.Should().Be(gameOptionsValue.Team1Actors[1].ActorType);
        game.Players[PlayerPosition.East].Actor.ActorType.Should().Be(gameOptionsValue.Team2Actors[0].ActorType);
        game.Players[PlayerPosition.West].Actor.ActorType.Should().Be(gameOptionsValue.Team2Actors[1].ActorType);
    }
}
