using Bogus;

using FluentAssertions;

using Microsoft.Extensions.Options;
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
    private readonly IOptions<MachineLearningOptions> _machineLearningOptions = Microsoft.Extensions.Options.Options.Create(new MachineLearningOptions());
    private readonly Actor _actor = Actor.WithModel(ActorType.ModelTrainer, "Gen1");

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
            _actor);

        bot.ActorType.Should().Be(ActorType.ModelTrainer);
    }

    [Fact]
    public Task CallTrumpAsync_ShouldThrowInvalidOperationException_WhenEngineNotAvailable()
    {
        _mockEngineProvider
            .Setup(x => x.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "Gen1"))
            .Returns((PredictionEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>?)null);

        var bot = new ModelTrainerBot(
            _mockEngineProvider.Object,
            _mockCallTrumpFeatureBuilder.Object,
            _mockDiscardCardFeatureBuilder.Object,
            _mockPlayCardFeatureBuilder.Object,
            _mockRandom.Object,
            _machineLearningOptions,
            _actor);

        var act = async () => await bot.CallTrumpAsync(
            GenerateCards(5),
            0,
            0,
            RelativePlayerPosition.Partner,
            GenerateCard(),
            [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp],
            1);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CallTrump*");
    }

    [Fact]
    public Task DiscardCardAsync_ShouldThrowInvalidOperationException_WhenEngineNotAvailable()
    {
        _mockEngineProvider
            .Setup(x => x.TryGetEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>("DiscardCard", "Gen1"))
            .Returns((PredictionEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>?)null);

        var bot = new ModelTrainerBot(
            _mockEngineProvider.Object,
            _mockCallTrumpFeatureBuilder.Object,
            _mockDiscardCardFeatureBuilder.Object,
            _mockPlayCardFeatureBuilder.Object,
            _mockRandom.Object,
            _machineLearningOptions,
            _actor);

        var validCards = GenerateRelativeCards(6);

        var act = async () => await bot.DiscardCardAsync(
            validCards,
            0,
            0,
            RelativePlayerPosition.Partner,
            false,
            validCards);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*DiscardCard*");
    }

    [Fact]
    public Task PlayCardAsync_ShouldThrowInvalidOperationException_WhenEngineNotAvailable()
    {
        _mockEngineProvider
            .Setup(x => x.TryGetEngine<PlayCardTrainingData, PlayCardRegressionPrediction>("PlayCard", "Gen1"))
            .Returns((PredictionEngine<PlayCardTrainingData, PlayCardRegressionPrediction>?)null);

        var bot = new ModelTrainerBot(
            _mockEngineProvider.Object,
            _mockCallTrumpFeatureBuilder.Object,
            _mockDiscardCardFeatureBuilder.Object,
            _mockPlayCardFeatureBuilder.Object,
            _mockRandom.Object,
            _machineLearningOptions,
            _actor);

        var validCards = GenerateRelativeCards(5);

        var act = async () => await bot.PlayCardAsync(
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
            0,
            0,
            validCards);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*PlayCard*");
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
        var cards = new HashSet<RelativeCard>();
        while (cards.Count < count)
        {
            cards.Add(new RelativeCard(_faker.PickRandom<Rank>(), _faker.PickRandom<RelativeSuit>())
            {
                Card = GenerateCard(),
            });
        }

        return [.. cards];
    }
}
