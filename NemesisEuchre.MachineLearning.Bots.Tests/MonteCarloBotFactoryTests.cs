using FluentAssertions;

using Moq;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Utilities;
using NemesisEuchre.MachineLearning.Bots.Simulation;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Options;

using Xunit;

namespace NemesisEuchre.MachineLearning.Bots.Tests;

public class MonteCarloBotFactoryTests
{
    private readonly MonteCarloBotFactory _factory;

    public MonteCarloBotFactoryTests()
    {
        _factory = new MonteCarloBotFactory(
            new Mock<IPredictionEngineProvider>().Object,
            new Mock<ICallTrumpInferenceFeatureBuilder>().Object,
            new Mock<IDiscardCardInferenceFeatureBuilder>().Object,
            new Mock<IPlayCardInferenceFeatureBuilder>().Object,
            new Mock<ISimplePlayCardInferenceFeatureBuilder>().Object,
            new Mock<IAdvancedPlayCardInferenceFeatureBuilder>().Object,
            new Mock<IAdvancedCallTrumpInferenceFeatureBuilder>().Object,
            new Mock<IAdvancedDiscardCardInferenceFeatureBuilder>().Object,
            new Mock<IRandomNumberGenerator>().Object,
            Microsoft.Extensions.Options.Options.Create(new MachineLearningOptions()),
            new Mock<IDealSimulator>().Object,
            new Mock<IHiddenCardDistributor>().Object);
    }

    [Fact]
    public void ActorType_IsMonteCarlo()
    {
        _factory.ActorType.Should().Be(ActorType.MonteCarlo);
    }

    [Fact]
    public void CreatePlayerActor_ReturnsMonteCarloBot()
    {
        var actor = Actor.WithModel(ActorType.MonteCarlo, "testModel", simulationCount: 25);

        var bot = _factory.CreatePlayerActor(actor);

        bot.Should().BeOfType<MonteCarloBot>();
        bot.ActorType.Should().Be(ActorType.MonteCarlo);
    }

    [Fact]
    public void CreatePlayerActor_ThrowsWhenNoModelProvided()
    {
        var actor = new Actor(ActorType.MonteCarlo, SimulationCount: 50);

        var act = () => _factory.CreatePlayerActor(actor);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*model name*");
    }

    [Fact]
    public void CreatePlayerActor_ThrowsWhenModelNamesEmpty()
    {
        var actor = new Actor(ActorType.MonteCarlo, [], SimulationCount: 50);

        var act = () => _factory.CreatePlayerActor(actor);

        act.Should().Throw<ArgumentException>();
    }
}
