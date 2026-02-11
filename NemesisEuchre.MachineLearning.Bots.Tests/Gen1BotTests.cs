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

using Xunit;

namespace NemesisEuchre.MachineLearning.Bots.Tests;

public class Gen1BotTests
{
    private readonly Faker _faker = new();
    private readonly Mock<IPredictionEngineProvider> _mockEngineProvider = new();
    private readonly Mock<ICallTrumpInferenceFeatureBuilder> _mockCallTrumpFeatureBuilder = new();
    private readonly Mock<IDiscardCardInferenceFeatureBuilder> _mockDiscardCardFeatureBuilder = new();
    private readonly Mock<IPlayCardInferenceFeatureBuilder> _mockPlayCardFeatureBuilder = new();
    private readonly Mock<IRandomNumberGenerator> _mockRandom = new();
    private readonly Mock<ILogger<Gen1Bot>> _mockLogger = new();

    [Fact]
    public void Constructor_ShouldCallEngineProvider_ForAllThreeModels()
    {
        _mockEngineProvider
            .Setup(x => x.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "Gen1"))
            .Returns((PredictionEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>?)null);
        _mockEngineProvider
            .Setup(x => x.TryGetEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>("DiscardCard", "Gen1"))
            .Returns((PredictionEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>?)null);
        _mockEngineProvider
            .Setup(x => x.TryGetEngine<PlayCardTrainingData, PlayCardRegressionPrediction>("PlayCard", "Gen1"))
            .Returns((PredictionEngine<PlayCardTrainingData, PlayCardRegressionPrediction>?)null);

        _ = new Gen1Bot(
            _mockEngineProvider.Object,
            _mockCallTrumpFeatureBuilder.Object,
            _mockDiscardCardFeatureBuilder.Object,
            _mockPlayCardFeatureBuilder.Object,
            _mockRandom.Object,
            _mockLogger.Object);

        _mockEngineProvider.Verify(
            x => x.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "Gen1"),
            Times.Once);
        _mockEngineProvider.Verify(
            x => x.TryGetEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>("DiscardCard", "Gen1"),
            Times.Once);
        _mockEngineProvider.Verify(
            x => x.TryGetEngine<PlayCardTrainingData, PlayCardRegressionPrediction>("PlayCard", "Gen1"),
            Times.Once);
    }

    [Fact]
    public void Constructor_ShouldSucceed_WhenEngineProviderReturnsNull()
    {
        _mockEngineProvider
            .Setup(x => x.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "Gen1"))
            .Returns((PredictionEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>?)null);

        var bot = new Gen1Bot(
            _mockEngineProvider.Object,
            _mockCallTrumpFeatureBuilder.Object,
            _mockDiscardCardFeatureBuilder.Object,
            _mockPlayCardFeatureBuilder.Object,
            _mockRandom.Object,
            _mockLogger.Object);

        bot.Should().NotBeNull();
        bot.ActorType.Should().Be(ActorType.Gen1);
    }

    [Fact]
    public void ActorType_ShouldReturnGen1()
    {
        var bot = new Gen1Bot(
            _mockEngineProvider.Object,
            _mockCallTrumpFeatureBuilder.Object,
            _mockDiscardCardFeatureBuilder.Object,
            _mockPlayCardFeatureBuilder.Object,
            _mockRandom.Object,
            _mockLogger.Object);

        bot.ActorType.Should().Be(ActorType.Gen1);
    }

    [Fact]
    public async Task CallTrumpAsync_ShouldFallbackToRandom_WhenEngineNotAvailable()
    {
        _mockEngineProvider
            .Setup(x => x.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "Gen1"))
            .Returns((PredictionEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>?)null);

        var bot = new Gen1Bot(
            _mockEngineProvider.Object,
            _mockCallTrumpFeatureBuilder.Object,
            _mockDiscardCardFeatureBuilder.Object,
            _mockPlayCardFeatureBuilder.Object,
            _mockRandom.Object,
            _mockLogger.Object);
        var cardsInHand = GenerateCards(5);
        var upCard = GenerateCard();
        var validDecisions = new[] { CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp };

        var result = await bot.CallTrumpAsync(
            cardsInHand,
            0,
            0,
            RelativePlayerPosition.Self,
            upCard,
            validDecisions);

        result.ChosenCallTrumpDecision.Should().BeOneOf(validDecisions);
    }

    [Fact]
    public Task DiscardCardAsync_ShouldThrowException_WhenHandIsNot6Cards()
    {
        var bot = new Gen1Bot(
            _mockEngineProvider.Object,
            _mockCallTrumpFeatureBuilder.Object,
            _mockDiscardCardFeatureBuilder.Object,
            _mockPlayCardFeatureBuilder.Object,
            _mockRandom.Object,
            _mockLogger.Object);
        var cardsInHand = GenerateRelativeCards(5);
        var validCardsToDiscard = new[] { cardsInHand[0] };

        var act = async () => await bot.DiscardCardAsync(
            cardsInHand,
            0,
            0,
            RelativePlayerPosition.Self,
            false,
            validCardsToDiscard);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Expected 6 cards in hand for discard, got 5");
    }

    [Fact]
    public async Task DiscardCardAsync_ShouldFallbackToRandom_WhenEngineNotAvailable()
    {
        _mockEngineProvider
            .Setup(x => x.TryGetEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>("DiscardCard", "Gen1"))
            .Returns((PredictionEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>?)null);

        var bot = new Gen1Bot(
            _mockEngineProvider.Object,
            _mockCallTrumpFeatureBuilder.Object,
            _mockDiscardCardFeatureBuilder.Object,
            _mockPlayCardFeatureBuilder.Object,
            _mockRandom.Object,
            _mockLogger.Object);
        var cardsInHand = GenerateRelativeCards(6);
        var validCardsToDiscard = new[] { cardsInHand[0], cardsInHand[1] };

        var result = await bot.DiscardCardAsync(
            cardsInHand,
            0,
            0,
            RelativePlayerPosition.Partner,
            false,
            validCardsToDiscard);

        result.ChosenCard.Should().BeOneOf(validCardsToDiscard);
    }

    [Fact]
    public async Task PlayCardAsync_ShouldFallbackToRandom_WhenEngineNotAvailable()
    {
        _mockEngineProvider
            .Setup(x => x.TryGetEngine<PlayCardTrainingData, PlayCardRegressionPrediction>("PlayCard", "Gen1"))
            .Returns((PredictionEngine<PlayCardTrainingData, PlayCardRegressionPrediction>?)null);

        var bot = new Gen1Bot(
            _mockEngineProvider.Object,
            _mockCallTrumpFeatureBuilder.Object,
            _mockDiscardCardFeatureBuilder.Object,
            _mockPlayCardFeatureBuilder.Object,
            _mockRandom.Object,
            _mockLogger.Object);
        var cardsInHand = GenerateRelativeCards(5);
        var validCardsToPlay = new[] { cardsInHand[0], cardsInHand[1] };
        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>();

        var result = await bot.PlayCardAsync(
            cardsInHand,
            0,
            0,
            RelativePlayerPosition.Self,
            false,
            RelativePlayerPosition.LeftHandOpponent,
            null,
            RelativePlayerPosition.Self,
            RelativeSuit.Trump,
            [],
            [],
            playedCards,
            null,
            1,
            validCardsToPlay);

        result.ChosenCard.Should().BeOneOf(validCardsToPlay);
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
