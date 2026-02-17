using NemesisEuchre.Console.Models.BehavioralTests;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.CallTrump;

public class StrongHandWithRightBowerUpShouldScoreHigherWithTeamDealer(
    ICallTrumpInferenceFeatureBuilder featureBuilder)
    : CallTrumpBehavioralTest(featureBuilder)
{
    private static readonly CallTrumpDecision[] RoundOneDecisions =
    [
        CallTrumpDecision.Pass,
        CallTrumpDecision.OrderItUp,
        CallTrumpDecision.OrderItUpAndGoAlone,
    ];

    public override string Name => "Right bower up scores higher with team dealer";

    public override string Description =>
        "With Ace + King of trump and Jack (right bower) as upcard, ordering up should score higher when dealer is on your team";

    public override string AssertionDescription => "Team score > opponent score";

    public override IReadOnlyList<BehavioralTestResult> Run(
        IPredictionEngineProvider engineProvider,
        string modelName)
    {
        var engine = engineProvider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
            "CallTrump", modelName);

        if (engine == null)
        {
            return [new BehavioralTestResult(
                Name,
                DecisionType,
                false,
                "-",
                AssertionDescription,
                [],
                "Failed to load CallTrump model")];
        }

        var results = new List<BehavioralTestResult>(4);

        foreach (var suit in Enum.GetValues<Suit>())
        {
            var opposites = GetOppositeColorSuits(suit);
            Card[] hand =
            [
                new(suit, Rank.Ace),
                new(suit, Rank.King),
                new(opposites[0], Rank.Nine),
                new(opposites[0], Rank.Ten),
                new(opposites[1], Rank.Nine),
            ];
            var upCard = new Card(suit, Rank.Jack);

            var positionScores = new Dictionary<string, float>();

            foreach (var dealerPos in Enum.GetValues<RelativePlayerPosition>())
            {
                var bestScore = float.MinValue;

                foreach (var decision in RoundOneDecisions)
                {
                    var features = FeatureBuilder.BuildFeatures(
                        hand,
                        upCard,
                        dealerPos,
                        TeamScore,
                        OpponentScore,
                        decision);
                    var prediction = engine.Predict(features);

                    if (prediction.PredictedPoints > bestScore)
                    {
                        bestScore = prediction.PredictedPoints;
                    }
                }

                positionScores[dealerPos.ToString()] = bestScore;
            }

            var teamMax = Math.Max(
                positionScores[nameof(RelativePlayerPosition.Self)],
                positionScores[nameof(RelativePlayerPosition.Partner)]);
            var opponentMax = Math.Max(
                positionScores[nameof(RelativePlayerPosition.LeftHandOpponent)],
                positionScores[nameof(RelativePlayerPosition.RightHandOpponent)]);

            var passed = teamMax > opponentMax;
            var failureReason = passed
                ? null
                : $"Team max ({teamMax:F4}) <= Opponent max ({opponentMax:F4})";

            results.Add(new BehavioralTestResult(
                $"{Name} ({suit})",
                DecisionType,
                passed,
                $"Team: {teamMax:F4}, Opp: {opponentMax:F4}",
                AssertionDescription,
                positionScores,
                failureReason));
        }

        return results;
    }

    protected override bool IsExpectedChoice(CallTrumpDecision chosenDecision)
    {
        return chosenDecision != CallTrumpDecision.Pass;
    }

    private static Suit[] GetOppositeColorSuits(Suit trump)
    {
        var sameColor = trump.GetSameColorSuit();
        return [.. Enum.GetValues<Suit>().Where(s => s != trump && s != sameColor)];
    }
}
