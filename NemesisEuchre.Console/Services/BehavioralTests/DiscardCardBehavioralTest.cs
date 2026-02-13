using NemesisEuchre.Console.Models.BehavioralTests;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.Console.Services.BehavioralTests;

public abstract class DiscardCardBehavioralTest(
    IDiscardCardInferenceFeatureBuilder featureBuilder) : IModelBehavioralTest
{
    public abstract string Name { get; }

    public abstract string Description { get; }

    public DecisionType DecisionType => DecisionType.Discard;

    public abstract string AssertionDescription { get; }

    protected virtual RelativePlayerPosition CallingPlayer => RelativePlayerPosition.Self;

    protected virtual bool CallingPlayerGoingAlone => false;

    protected virtual short TeamScore => 0;

    protected virtual short OpponentScore => 0;

    public IReadOnlyList<BehavioralTestResult> Run(IPredictionEngineProvider engineProvider, string modelName)
    {
        var engine = engineProvider.TryGetEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>(
            "DiscardCard", modelName);

        if (engine == null)
        {
            return [new BehavioralTestResult(
                Name,
                DecisionType,
                false,
                "-",
                AssertionDescription,
                [],
                "Failed to load DiscardCard model")];
        }

        var testCases = GetTestCases();
        var results = new List<BehavioralTestResult>(testCases.Count);

        foreach (var testCase in testCases)
        {
            var scores = new Dictionary<string, float>();
            RelativeCard? bestCard = null;
            var bestScore = float.MinValue;

            foreach (var card in testCase.CardsInHand)
            {
                var features = featureBuilder.BuildFeatures(
                    testCase.CardsInHand,
                    CallingPlayer,
                    CallingPlayerGoingAlone,
                    TeamScore,
                    OpponentScore,
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

            results.Add(new BehavioralTestResult(
                testCase.Label,
                DecisionType,
                passed,
                chosenDisplay,
                AssertionDescription,
                scores,
                failureReason));
        }

        return results;
    }

    protected static IReadOnlyList<DiscardCardTestCase> GenerateAllSuitVariants(
        string baseName,
        Func<Suit, RelativeCard[]> buildHand)
    {
        return [.. Enum.GetValues<Suit>()
            .Select(suit => new DiscardCardTestCase(
                $"{baseName} ({suit})",
                buildHand(suit)))];
    }

    protected virtual IReadOnlyList<DiscardCardTestCase> GetTestCases()
    {
        return [new(Name, GetCardsInHand())];
    }

    protected virtual RelativeCard[] GetCardsInHand()
    {
        throw new InvalidOperationException("Override GetTestCases() or GetCardsInHand()");
    }

    protected abstract bool IsExpectedChoice(RelativeCard chosenCard);

    private static string FormatCard(RelativeCard card)
    {
        return $"{card.Rank} of {card.Suit}";
    }

    public record DiscardCardTestCase(string Label, RelativeCard[] CardsInHand);
}
