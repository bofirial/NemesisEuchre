using NemesisEuchre.Console.Models.BehavioralTests;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.Console.Services.BehavioralTests;

public abstract class CallTrumpBehavioralTest(
    ICallTrumpInferenceFeatureBuilder featureBuilder) : IModelBehavioralTest
{
    public abstract string Name { get; }

    public abstract string Description { get; }

    public DecisionType DecisionType => DecisionType.CallTrump;

    public abstract string AssertionDescription { get; }

    protected virtual RelativePlayerPosition DealerPosition => RelativePlayerPosition.LeftHandOpponent;

    protected virtual short TeamScore => 0;

    protected virtual short OpponentScore => 0;

    public BehavioralTestResult Run(IPredictionEngineProvider engineProvider, string modelName)
    {
        var engine = engineProvider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
            "CallTrump", modelName);

        if (engine == null)
        {
            return new BehavioralTestResult(
                Name,
                DecisionType,
                false,
                "-",
                AssertionDescription,
                [],
                "Failed to load CallTrump model");
        }

        var cardsInHand = GetCardsInHand();
        var upCard = GetUpCard();
        var validDecisions = GetValidDecisions();
        var scores = new Dictionary<string, float>();
        CallTrumpDecision? bestDecision = null;
        var bestScore = float.MinValue;

        foreach (var decision in validDecisions)
        {
            var features = featureBuilder.BuildFeatures(
                cardsInHand,
                upCard,
                DealerPosition,
                TeamScore,
                OpponentScore,
                validDecisions,
                decision);
            var prediction = engine.Predict(features);
            var score = prediction.PredictedPoints;
            var display = decision.ToString();
            scores[display] = score;

            if (score > bestScore)
            {
                bestScore = score;
                bestDecision = decision;
            }
        }

        var passed = bestDecision.HasValue && IsExpectedChoice(bestDecision.Value);
        var chosenDisplay = bestDecision.HasValue ? bestDecision.Value.ToString() : "-";
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

    protected abstract Card[] GetCardsInHand();

    protected abstract Card GetUpCard();

    protected abstract CallTrumpDecision[] GetValidDecisions();

    protected abstract bool IsExpectedChoice(CallTrumpDecision chosenDecision);
}
