using FluentAssertions;

using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.GameEngine.Tests.Constants;

public class ActorTests
{
    [Fact]
    public void WithModel_CreatesActorWithSingleModel()
    {
        var actor = Actor.WithModel(ActorType.Model, "Gen2");

        actor.ActorType.Should().Be(ActorType.Model);
        actor.ModelName.Should().Be("Gen2");
        actor.ModelNames.Should().NotBeNull();
        actor.ModelNames.Should().ContainKey("default");
        actor.ModelNames!["default"].Should().Be("Gen2");
    }

    [Fact]
    public void WithModel_CreatesActorWithExplorationTemperature()
    {
        var actor = Actor.WithModel(ActorType.ModelTrainer, "Gen2", 0.5f);

        actor.ActorType.Should().Be(ActorType.ModelTrainer);
        actor.ModelName.Should().Be("Gen2");
        actor.ExplorationTemperature.Should().Be(0.5f);
    }

    [Fact]
    public void WithModels_CreatesActorWithPerDecisionTypeModels()
    {
        var actor = Actor.WithModels(
            ActorType.Model,
            playCardModel: "Gen2A",
            callTrumpModel: "Gen2B",
            discardCardModel: "Gen2C");

        actor.ActorType.Should().Be(ActorType.Model);
        actor.ModelNames.Should().NotBeNull();
        actor.ModelNames.Should().ContainKey("PlayCard");
        actor.ModelNames!["PlayCard"].Should().Be("Gen2A");
        actor.ModelNames.Should().ContainKey("CallTrump");
        actor.ModelNames["CallTrump"].Should().Be("Gen2B");
        actor.ModelNames.Should().ContainKey("DiscardCard");
        actor.ModelNames["DiscardCard"].Should().Be("Gen2C");
    }

    [Fact]
    public void WithModels_CreatesActorWithDefaultModel()
    {
        var actor = Actor.WithModels(
            ActorType.Model,
            defaultModel: "Gen2");

        actor.ActorType.Should().Be(ActorType.Model);
        actor.ModelName.Should().Be("Gen2");
        actor.ModelNames.Should().NotBeNull();
        actor.ModelNames.Should().ContainKey("default");
        actor.ModelNames!["default"].Should().Be("Gen2");
    }

    [Fact]
    public void WithModels_SkipsNullParameters()
    {
        var actor = Actor.WithModels(
            ActorType.Model,
            playCardModel: "Gen2A",
            callTrumpModel: null,
            discardCardModel: null,
            defaultModel: "Gen2");

        actor.ModelNames.Should().NotBeNull();
        actor.ModelNames.Should().ContainKey("PlayCard");
        actor.ModelNames.Should().ContainKey("default");
        actor.ModelNames.Should().NotContainKey("CallTrump");
        actor.ModelNames.Should().NotContainKey("DiscardCard");
    }

    [Fact]
    public void GetModelName_ReturnsSpecificModel_WhenAvailable()
    {
        var actor = Actor.WithModels(
            ActorType.Model,
            playCardModel: "Gen2A",
            callTrumpModel: "Gen2B",
            defaultModel: "Gen2");

        actor.GetModelName("PlayCard").Should().Be("Gen2A");
        actor.GetModelName("CallTrump").Should().Be("Gen2B");
    }

    [Fact]
    public void GetModelName_FallsBackToDefault_WhenSpecificNotAvailable()
    {
        var actor = Actor.WithModels(
            ActorType.Model,
            playCardModel: "Gen2A",
            defaultModel: "Gen2");

        actor.GetModelName("CallTrump").Should().Be("Gen2");
        actor.GetModelName("DiscardCard").Should().Be("Gen2");
    }

    [Fact]
    public void GetModelName_ReturnsNull_WhenNoModelsAvailable()
    {
        var actor = new Actor(ActorType.Chaos);

        actor.GetModelName("PlayCard").Should().BeNull();
        actor.GetModelName("CallTrump").Should().BeNull();
        actor.GetModelName("DiscardCard").Should().BeNull();
    }

    [Fact]
    public void ModelName_Property_ReturnsDefaultModel()
    {
        var actor = Actor.WithModels(
            ActorType.Model,
            playCardModel: "Gen2A",
            defaultModel: "Gen2");

        actor.ModelName.Should().Be("Gen2");
    }

    [Fact]
    public void ModelName_Property_ReturnsNull_WhenNoDefaultModel()
    {
        var actor = Actor.WithModels(
            ActorType.Model,
            playCardModel: "Gen2A");

        actor.ModelName.Should().BeNull();
    }

    [Fact]
    public void ModelName_Property_ReturnsNull_WhenNoModels()
    {
        var actor = new Actor(ActorType.Chaos);

        actor.ModelName.Should().BeNull();
    }
}
