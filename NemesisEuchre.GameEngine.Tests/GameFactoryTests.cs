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
    public Task CreateGameAsync_WithInvalidTeam1ActorTypesLength_ThrowsArgumentException(int length)
    {
        var gameOptions = MsOptions.Options.Create(new GameOptions());
        var gameInitializer = new GameFactory(gameOptions);

        var act = async () => await gameInitializer.CreateGameAsync(team1ActorTypes: new ActorType[length]);

        return act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("team1ActorTypes")
            .WithMessage("Team1ActorTypes must contain exactly 2 actor types.*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    public Task CreateGameAsync_WithInvalidTeam2ActorTypesLength_ThrowsArgumentException(int length)
    {
        var gameOptions = MsOptions.Options.Create(new GameOptions());
        var gameInitializer = new GameFactory(gameOptions);

        var act = async () => await gameInitializer.CreateGameAsync(team2ActorTypes: new ActorType[length]);

        return act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("team2ActorTypes")
            .WithMessage("Team2ActorTypes must contain exactly 2 actor types.*");
    }

    [Fact]
    public async Task CreateGameAsync_WithValidActorTypes_AssignsActorTypesToPlayers()
    {
        var gameOptions = MsOptions.Options.Create(new GameOptions
        {
            Team1ActorTypes = [ActorType.Chaos, ActorType.Chaos],
            Team2ActorTypes = [ActorType.Chaos, ActorType.Chaos],
        });
        var gameInitializer = new GameFactory(gameOptions);

        var game = await gameInitializer.CreateGameAsync();

        game.Players[PlayerPosition.North].ActorType.Should().Be(ActorType.Chaos);
        game.Players[PlayerPosition.South].ActorType.Should().Be(ActorType.Chaos);
        game.Players[PlayerPosition.East].ActorType.Should().Be(ActorType.Chaos);
        game.Players[PlayerPosition.West].ActorType.Should().Be(ActorType.Chaos);
    }

    [Fact]
    public async Task CreateGameAsync_WithDifferentActorTypes_AssignsCorrectActorTypesToEachPlayer()
    {
        var gameOptionsValue = new GameOptions
        {
            Team1ActorTypes = [ActorType.Chaos, ActorType.Chaos],
            Team2ActorTypes = [ActorType.Chaos, ActorType.Chaos],
        };
        var gameOptions = MsOptions.Options.Create(gameOptionsValue);
        var gameInitializer = new GameFactory(gameOptions);

        var game = await gameInitializer.CreateGameAsync();

        game.Players[PlayerPosition.North].ActorType.Should().Be(gameOptionsValue.Team1ActorTypes[0]);
        game.Players[PlayerPosition.South].ActorType.Should().Be(gameOptionsValue.Team1ActorTypes[1]);
        game.Players[PlayerPosition.East].ActorType.Should().Be(gameOptionsValue.Team2ActorTypes[0]);
        game.Players[PlayerPosition.West].ActorType.Should().Be(gameOptionsValue.Team2ActorTypes[1]);
    }
}
