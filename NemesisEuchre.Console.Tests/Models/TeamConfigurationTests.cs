using FluentAssertions;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Tests.Models;

public class TeamConfigurationTests
{
    [Fact]
    public void FromActor_IncludesModelNames_Dictionary()
    {
        var actor = Actor.WithModels(
            ActorType.Model,
            playCardModel: "Gen2A",
            callTrumpModel: "Gen2B",
            defaultModel: "Gen2");

        var config = TeamConfiguration.FromActor(actor);

        config.ActorType.Should().Be(ActorType.Model);
        config.ModelNames.Should().NotBeNull();
        config.ModelNames.Should().ContainKey("PlayCard");
        config.ModelNames!["PlayCard"].Should().Be("Gen2A");
        config.ModelNames.Should().ContainKey("CallTrump");
        config.ModelNames["CallTrump"].Should().Be("Gen2B");
        config.ModelNames.Should().ContainKey("default");
        config.ModelNames["default"].Should().Be("Gen2");
    }

    [Fact]
    public void ModelName_Property_BackwardCompatibility()
    {
        var actor = Actor.WithModel(ActorType.Model, "Gen2", 0.1f);

        var config = TeamConfiguration.FromActor(actor);

        config.ModelName.Should().Be("Gen2");
        config.ExplorationTemperature.Should().Be(0.1f);
    }

    [Fact]
    public void FromActor_WithNullActor_ReturnsChaosBot()
    {
        var config = TeamConfiguration.FromActor(null);

        config.ActorType.Should().Be(ActorType.Chaos);
        config.ModelNames.Should().BeNull();
    }

    [Fact]
    public void FromActor_WithPerDecisionTypeModels_IncludesAllModels()
    {
        var actor = Actor.WithModels(
            ActorType.ModelTrainer,
            playCardModel: "Gen2A",
            callTrumpModel: "Gen2B",
            discardCardModel: "Gen2C",
            explorationTemperature: 0.3f);

        var config = TeamConfiguration.FromActor(actor);

        config.ActorType.Should().Be(ActorType.ModelTrainer);
        config.ExplorationTemperature.Should().Be(0.3f);
        config.ModelNames.Should().NotBeNull();
        config.ModelNames.Should().HaveCount(3);
        config.ModelNames!["PlayCard"].Should().Be("Gen2A");
        config.ModelNames["CallTrump"].Should().Be("Gen2B");
        config.ModelNames["DiscardCard"].Should().Be("Gen2C");
    }
}
