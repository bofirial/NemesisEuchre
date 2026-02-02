using FluentAssertions;

using Moq;

using NemesisEuchre.Console.Services;
using NemesisEuchre.Console.Services.TrainerExecutors;
using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Tests.Services;

public class TrainerFactoryTests
{
    [Fact]
    public void GetTrainers_WhenDecisionTypeIsCallTrump_ReturnsCallTrumpTrainer()
    {
        var callTrumpTrainer = CreateMockTrainer(DecisionType.CallTrump, "CallTrumpRegression");
        var discardTrainer = CreateMockTrainer(DecisionType.Discard, "DiscardCardRegression");
        var playTrainer = CreateMockTrainer(DecisionType.Play, "PlayCardRegression");

        var factory = new TrainerFactory([callTrumpTrainer, discardTrainer, playTrainer]);

        var result = factory.GetTrainers(DecisionType.CallTrump).ToList();

        result.Should().HaveCount(1);
        result[0].DecisionType.Should().Be(DecisionType.CallTrump);
        result[0].ModelType.Should().Be("CallTrumpRegression");
    }

    [Fact]
    public void GetTrainers_WhenDecisionTypeIsDiscard_ReturnsDiscardTrainer()
    {
        var callTrumpTrainer = CreateMockTrainer(DecisionType.CallTrump, "CallTrumpRegression");
        var discardTrainer = CreateMockTrainer(DecisionType.Discard, "DiscardCardRegression");
        var playTrainer = CreateMockTrainer(DecisionType.Play, "PlayCardRegression");

        var factory = new TrainerFactory([callTrumpTrainer, discardTrainer, playTrainer]);

        var result = factory.GetTrainers(DecisionType.Discard).ToList();

        result.Should().HaveCount(1);
        result[0].DecisionType.Should().Be(DecisionType.Discard);
        result[0].ModelType.Should().Be("DiscardCardRegression");
    }

    [Fact]
    public void GetTrainers_WhenDecisionTypeIsPlay_ReturnsPlayTrainer()
    {
        var callTrumpTrainer = CreateMockTrainer(DecisionType.CallTrump, "CallTrumpRegression");
        var discardTrainer = CreateMockTrainer(DecisionType.Discard, "DiscardCardRegression");
        var playTrainer = CreateMockTrainer(DecisionType.Play, "PlayCardRegression");

        var factory = new TrainerFactory([callTrumpTrainer, discardTrainer, playTrainer]);

        var result = factory.GetTrainers(DecisionType.Play).ToList();

        result.Should().HaveCount(1);
        result[0].DecisionType.Should().Be(DecisionType.Play);
        result[0].ModelType.Should().Be("PlayCardRegression");
    }

    [Fact]
    public void GetTrainers_WhenDecisionTypeIsAll_ReturnsAllTrainers()
    {
        var callTrumpTrainer = CreateMockTrainer(DecisionType.CallTrump, "CallTrumpRegression");
        var discardTrainer = CreateMockTrainer(DecisionType.Discard, "DiscardCardRegression");
        var playTrainer = CreateMockTrainer(DecisionType.Play, "PlayCardRegression");

        var factory = new TrainerFactory([callTrumpTrainer, discardTrainer, playTrainer]);

        var result = factory.GetTrainers(DecisionType.All).ToList();

        result.Should().HaveCount(3);
        result.Should().Contain(t => t.DecisionType == DecisionType.CallTrump);
        result.Should().Contain(t => t.DecisionType == DecisionType.Discard);
        result.Should().Contain(t => t.DecisionType == DecisionType.Play);
    }

    private static ITrainerExecutor CreateMockTrainer(DecisionType decisionType, string modelType)
    {
        var mock = new Mock<ITrainerExecutor>();
        mock.Setup(m => m.DecisionType).Returns(decisionType);
        mock.Setup(m => m.ModelType).Returns(modelType);
        return mock.Object;
    }
}
