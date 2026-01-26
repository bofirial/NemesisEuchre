using FluentAssertions;

using Moq;

using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Services;

namespace NemesisEuchre.GameEngine.Tests.Services;

public class PlayerActorResolverTests
{
    [Fact]
    public void GetPlayerActor_ReturnsCorrectActor()
    {
        var chaosActorMock = new Mock<IPlayerActor>();
        chaosActorMock.Setup(x => x.ActorType).Returns(ActorType.Chaos);

        var chadActorMock = new Mock<IPlayerActor>();
        chadActorMock.Setup(x => x.ActorType).Returns(ActorType.Chad);

        var actors = new[] { chaosActorMock.Object, chadActorMock.Object };
        var resolver = new PlayerActorResolver(actors);

        var player = new DealPlayer { ActorType = ActorType.Chaos };

        var actor = resolver.GetPlayerActor(player);

        actor.Should().BeSameAs(chaosActorMock.Object);
    }

    [Fact]
    public void GetPlayerActor_WithMultipleActors_ReturnsCorrectOne()
    {
        var chaosActorMock = new Mock<IPlayerActor>();
        chaosActorMock.Setup(x => x.ActorType).Returns(ActorType.Chaos);

        var chadActorMock = new Mock<IPlayerActor>();
        chadActorMock.Setup(x => x.ActorType).Returns(ActorType.Chad);

        var betaActorMock = new Mock<IPlayerActor>();
        betaActorMock.Setup(x => x.ActorType).Returns(ActorType.Beta);

        var actors = new[] { chaosActorMock.Object, chadActorMock.Object, betaActorMock.Object };
        var resolver = new PlayerActorResolver(actors);

        var player = new DealPlayer { ActorType = ActorType.Chad };

        var actor = resolver.GetPlayerActor(player);

        actor.Should().BeSameAs(chadActorMock.Object);
    }
}
