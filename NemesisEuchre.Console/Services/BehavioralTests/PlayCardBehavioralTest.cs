using NemesisEuchre.Console.Models.BehavioralTests;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.Console.Services.BehavioralTests;

public abstract class PlayCardBehavioralTest(
    IPlayCardInferenceFeatureBuilder featureBuilder) : IModelBehavioralTest
{
    public abstract string Name { get; }

    public abstract string Description { get; }

    public DecisionType DecisionType => DecisionType.Play;

    public abstract string AssertionDescription { get; }

    protected virtual RelativePlayerPosition LeadPlayer => RelativePlayerPosition.Self;

    protected virtual RelativeSuit? LeadSuit => null;

    protected virtual Dictionary<RelativePlayerPosition, RelativeCard> PlayedCardsInTrick => [];

    protected virtual short TeamScore => 0;

    protected virtual short OpponentScore => 0;

    protected virtual RelativePlayerPosition CallingPlayer => RelativePlayerPosition.Self;

    protected virtual bool CallingPlayerGoingAlone => false;

    protected virtual RelativePlayerPosition Dealer => RelativePlayerPosition.LeftHandOpponent;

    protected virtual RelativeCard? DealerPickedUpCard => null;

    protected virtual RelativePlayerSuitVoid[] KnownPlayerSuitVoids => [];

    protected virtual RelativeCard[] CardsAccountedFor => [];

    protected virtual RelativePlayerPosition? WinningTrickPlayer => null;

    protected virtual short TrickNumber => 1;

    public BehavioralTestResult Run(IPredictionEngineProvider engineProvider, string modelName)
    {
        var engine = engineProvider.TryGetEngine<PlayCardTrainingData, PlayCardRegressionPrediction>(
            "PlayCard", modelName);

        if (engine == null)
        {
            return new BehavioralTestResult(
                Name,
                DecisionType,
                false,
                "-",
                AssertionDescription,
                [],
                "Failed to load PlayCard model");
        }

        var cardsInHand = GetCardsInHand();
        var validCards = GetValidCardsToPlay();
        var scores = new Dictionary<string, float>();
        RelativeCard? bestCard = null;
        var bestScore = float.MinValue;

        foreach (var card in validCards)
        {
            var features = featureBuilder.BuildFeatures(
                cardsInHand,
                LeadPlayer,
                LeadSuit,
                PlayedCardsInTrick,
                TeamScore,
                OpponentScore,
                CallingPlayer,
                CallingPlayerGoingAlone,
                Dealer,
                DealerPickedUpCard,
                KnownPlayerSuitVoids,
                CardsAccountedFor,
                WinningTrickPlayer,
                TrickNumber,
                validCards,
                card);
            var prediction = engine.Predict(features);
            var score = prediction.PredictedPoints;
            var display = FormatCard(card);
            scores[display] = score;

            if (score > bestScore)
            {
                bestScore = score;
                bestCard = card;
            }
        }

        var passed = bestCard != null && IsExpectedChoice(bestCard);
        var chosenDisplay = bestCard != null ? FormatCard(bestCard) : "-";
        var failureReason = passed ? null : $"Chose {chosenDisplay} but expected: {AssertionDescription}";

        return new BehavioralTestResult(
            Name,
            DecisionType,
            passed,
            chosenDisplay,
            AssertionDescription,
            scores,
            failureReason);
    }

    protected abstract RelativeCard[] GetCardsInHand();

    protected abstract RelativeCard[] GetValidCardsToPlay();

    protected abstract bool IsExpectedChoice(RelativeCard chosenCard);

    private static string FormatCard(RelativeCard card)
    {
        return $"{card.Rank} of {card.Suit}";
    }
}
