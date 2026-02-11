using Bogus;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

using Moq;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;

using Xunit;

namespace NemesisEuchre.MachineLearning.Bots.Tests;

public class Gen1TrainerBotTests
{
    private readonly Faker _faker = new();
    private readonly Mock<IPredictionEngineProvider> _mockEngineProvider = new();
    private readonly Mock<ICallTrumpInferenceFeatureBuilder> _mockCallTrumpFeatureBuilder = new();
    private readonly Mock<IDiscardCardInferenceFeatureBuilder> _mockDiscardCardFeatureBuilder = new();
    private readonly Mock<IPlayCardInferenceFeatureBuilder> _mockPlayCardFeatureBuilder = new();
    private readonly Mock<IRandomNumberGenerator> _mockRandom = new();
    private readonly Mock<ILogger<ModelTrainerBot>> _mockLogger = new();
    private readonly MachineLearningOptions _machineLearningOptions = new();
    private readonly Actor _actor = new(ActorType.ModelTrainer, "Gen1");

    [Fact]
    public void ActorType_ShouldReturnGen1Trainer()
    {
        var bot = new ModelTrainerBot(
            _mockEngineProvider.Object,
            _mockCallTrumpFeatureBuilder.Object,
            _mockDiscardCardFeatureBuilder.Object,
            _mockPlayCardFeatureBuilder.Object,
            _mockRandom.Object,
            _machineLearningOptions,
            _mockLogger.Object,
            _actor);

        bot.ActorType.Should().Be(ActorType.ModelTrainer);
    }

    [Fact]
    public async Task CallTrumpAsync_ShouldFallbackToRandom_WhenEngineNotAvailable()
    {
        _mockEngineProvider
            .Setup(x => x.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "Gen1"))
            .Returns((PredictionEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>?)null);

        _mockRandom.Setup(x => x.NextInt(It.IsAny<int>())).Returns(0);

        var bot = new ModelTrainerBot(
            _mockEngineProvider.Object,
            _mockCallTrumpFeatureBuilder.Object,
            _mockDiscardCardFeatureBuilder.Object,
            _mockPlayCardFeatureBuilder.Object,
            _mockRandom.Object,
            _machineLearningOptions,
            _mockLogger.Object,
            _actor);

        var decisions = new[] { CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp };
        var result = await bot.CallTrumpAsync(
            GenerateCards(5),
            0,
            0,
            RelativePlayerPosition.Partner,
            GenerateCard(),
            decisions);

        result.ChosenCallTrumpDecision.Should().BeOneOf(decisions);
        result.DecisionPredictedPoints.Should().HaveCount(2);
    }

    [Fact]
    public async Task DiscardCardAsync_ShouldFallbackToRandom_WhenEngineNotAvailable()
    {
        _mockEngineProvider
            .Setup(x => x.TryGetEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>("DiscardCard", "Gen1"))
            .Returns((PredictionEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>?)null);

        _mockRandom.Setup(x => x.NextInt(It.IsAny<int>())).Returns(0);

        var bot = new ModelTrainerBot(
            _mockEngineProvider.Object,
            _mockCallTrumpFeatureBuilder.Object,
            _mockDiscardCardFeatureBuilder.Object,
            _mockPlayCardFeatureBuilder.Object,
            _mockRandom.Object,
            _machineLearningOptions,
            _mockLogger.Object,
            _actor);

        var validCards = GenerateRelativeCards(6);
        var result = await bot.DiscardCardAsync(
            validCards,
            0,
            0,
            RelativePlayerPosition.Partner,
            false,
            validCards);

        result.ChosenCard.Should().BeOneOf(validCards);
        result.DecisionPredictedPoints.Should().HaveCount(6);
    }

    [Fact]
    public async Task PlayCardAsync_ShouldFallbackToRandom_WhenEngineNotAvailable()
    {
        _mockEngineProvider
            .Setup(x => x.TryGetEngine<PlayCardTrainingData, PlayCardRegressionPrediction>("PlayCard", "Gen1"))
            .Returns((PredictionEngine<PlayCardTrainingData, PlayCardRegressionPrediction>?)null);

        _mockRandom.Setup(x => x.NextInt(It.IsAny<int>())).Returns(0);

        var bot = new ModelTrainerBot(
            _mockEngineProvider.Object,
            _mockCallTrumpFeatureBuilder.Object,
            _mockDiscardCardFeatureBuilder.Object,
            _mockPlayCardFeatureBuilder.Object,
            _mockRandom.Object,
            _machineLearningOptions,
            _mockLogger.Object,
            _actor);

        var validCards = GenerateRelativeCards(5);
        var result = await bot.PlayCardAsync(
            validCards,
            0,
            0,
            RelativePlayerPosition.Partner,
            false,
            RelativePlayerPosition.LeftHandOpponent,
            null,
            RelativePlayerPosition.RightHandOpponent,
            null,
            [],
            [],
            [],
            null,
            1,
            validCards);

        result.ChosenCard.Should().BeOneOf(validCards);
        result.DecisionPredictedPoints.Should().HaveCount(5);
    }

    private Card[] GenerateCards(int count)
    {
        var cards = new Card[count];
        for (int i = 0; i < count; i++)
        {
            cards[i] = GenerateCard();
        }

        return cards;
    }

    private Card GenerateCard()
    {
        return new Card(_faker.PickRandom<Suit>(), _faker.PickRandom<Rank>());
    }

    private RelativeCard[] GenerateRelativeCards(int count)
    {
        var cards = new RelativeCard[count];
        for (int i = 0; i < count; i++)
        {
            cards[i] = new RelativeCard(_faker.PickRandom<Rank>(), _faker.PickRandom<RelativeSuit>())
            {
                Card = GenerateCard(),
            };
        }

        return cards;
    }
}
