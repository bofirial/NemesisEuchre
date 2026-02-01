using Bogus;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;

using Xunit;

namespace NemesisEuchre.MachineLearning.Bots.Tests;

public class Gen1BotTests
{
    private readonly Faker _faker = new();
    private readonly Mock<IModelLoader> _mockModelLoader = new();
    private readonly Mock<ILogger<Gen1Bot>> _mockLogger = new();
    private readonly IOptions<MachineLearningOptions> _options = Microsoft.Extensions.Options.Options.Create(new MachineLearningOptions
    {
        ModelOutputPath = "./models",
    });

    [Fact]
    public void Constructor_ShouldCallModelLoader_ForAllThreeModels()
    {
        _mockModelLoader
            .Setup(x => x.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
                It.IsAny<string>(), 1, "CallTrump", null))
            .Throws(new FileNotFoundException());
        _mockModelLoader
            .Setup(x => x.LoadModel<DiscardCardTrainingData, DiscardCardRegressionPrediction>(
                It.IsAny<string>(), 1, "DiscardCard", null))
            .Throws(new FileNotFoundException());
        _mockModelLoader
            .Setup(x => x.LoadModel<PlayCardTrainingData, PlayCardRegressionPrediction>(
                It.IsAny<string>(), 1, "PlayCard", null))
            .Throws(new FileNotFoundException());

        _ = new Gen1Bot(_mockModelLoader.Object, _options, _mockLogger.Object);

        _mockModelLoader.Verify(
            x => x.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
                "./models", 1, "CallTrump", null),
            Times.Once);
        _mockModelLoader.Verify(
            x => x.LoadModel<DiscardCardTrainingData, DiscardCardRegressionPrediction>(
                "./models", 1, "DiscardCard", null),
            Times.Once);
        _mockModelLoader.Verify(
            x => x.LoadModel<PlayCardTrainingData, PlayCardRegressionPrediction>(
                "./models", 1, "PlayCard", null),
            Times.Once);
    }

    [Fact]
    public void Constructor_ShouldLogWarning_WhenModelNotFound()
    {
        _mockModelLoader
            .Setup(x => x.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
                It.IsAny<string>(), 1, "CallTrump", null))
            .Throws(new FileNotFoundException("Model not found"));

        _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _ = new Gen1Bot(_mockModelLoader.Object, _options, _mockLogger.Object);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.Is<EventId>(e => e.Id == 1),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("CallTrump")),
                It.IsAny<FileNotFoundException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ActorType_ShouldReturnGen1()
    {
        var bot = new Gen1Bot(_mockModelLoader.Object, _options, _mockLogger.Object);

        bot.ActorType.Should().Be(ActorType.Gen1);
    }

    [Fact]
    public async Task CallTrumpAsync_ShouldFallbackToRandom_WhenModelNotLoaded()
    {
        _mockModelLoader
            .Setup(x => x.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
                It.IsAny<string>(), 1, "CallTrump", null))
            .Throws(new FileNotFoundException());

        var bot = new Gen1Bot(_mockModelLoader.Object, _options, _mockLogger.Object);
        var cardsInHand = GenerateCards(5);
        var upCard = GenerateCard();
        var validDecisions = new[] { CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp };

        var result = await bot.CallTrumpAsync(
            cardsInHand,
            PlayerPosition.North,
            0,
            0,
            PlayerPosition.South,
            upCard,
            validDecisions);

        result.Should().BeOneOf(validDecisions);
    }

    [Fact]
    public Task DiscardCardAsync_ShouldThrowException_WhenHandIsNot6Cards()
    {
        var bot = new Gen1Bot(_mockModelLoader.Object, _options, _mockLogger.Object);
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
    public async Task DiscardCardAsync_ShouldFallbackToRandom_WhenModelNotLoaded()
    {
        _mockModelLoader
            .Setup(x => x.LoadModel<DiscardCardTrainingData, DiscardCardRegressionPrediction>(
                It.IsAny<string>(), 1, "DiscardCard", null))
            .Throws(new FileNotFoundException());

        var bot = new Gen1Bot(_mockModelLoader.Object, _options, _mockLogger.Object);
        var cardsInHand = GenerateRelativeCards(6);
        var validCardsToDiscard = new[] { cardsInHand[0], cardsInHand[1] };

        var result = await bot.DiscardCardAsync(
            cardsInHand,
            0,
            0,
            RelativePlayerPosition.Partner,
            false,
            validCardsToDiscard);

        result.Should().BeOneOf(validCardsToDiscard);
    }

    [Fact]
    public async Task PlayCardAsync_ShouldFallbackToRandom_WhenModelNotLoaded()
    {
        _mockModelLoader
            .Setup(x => x.LoadModel<PlayCardTrainingData, PlayCardRegressionPrediction>(
                It.IsAny<string>(), 1, "PlayCard", null))
            .Throws(new FileNotFoundException());

        var bot = new Gen1Bot(_mockModelLoader.Object, _options, _mockLogger.Object);
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
            RelativeSuit.Trump,
            playedCards,
            null,
            validCardsToPlay);

        result.Should().BeOneOf(validCardsToPlay);
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
        return new Card
        {
            Rank = _faker.PickRandom<Rank>(),
            Suit = _faker.PickRandom<Suit>(),
        };
    }

    private RelativeCard[] GenerateRelativeCards(int count)
    {
        var cards = new RelativeCard[count];
        for (int i = 0; i < count; i++)
        {
            cards[i] = new RelativeCard
            {
                Rank = _faker.PickRandom<Rank>(),
                Suit = _faker.PickRandom<RelativeSuit>(),
                Card = GenerateCard(),
            };
        }

        return cards;
    }
}
