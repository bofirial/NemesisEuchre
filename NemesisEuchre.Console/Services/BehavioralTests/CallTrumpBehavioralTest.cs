using NemesisEuchre.Console.Models.BehavioralTests;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Loading;

namespace NemesisEuchre.Console.Services.BehavioralTests;

public abstract class CallTrumpBehavioralTest(
    ICallTrumpBehavioralTestRunner runner) : IModelBehavioralTest
{
    public abstract string Name { get; }

    public abstract string Description { get; }

    public DecisionType DecisionType => Runner.DecisionType;

    public abstract string AssertionDescription { get; }

    protected ICallTrumpBehavioralTestRunner Runner { get; } = runner;

    protected virtual RelativePlayerPosition DealerPosition => RelativePlayerPosition.LeftHandOpponent;

    protected virtual short TeamScore => 0;

    protected virtual short OpponentScore => 0;

    public virtual IReadOnlyList<BehavioralTestResult> Run(IPredictionEngineProvider engineProvider, string modelName)
    {
        var testCases = GetTestCases();
        var results = new List<BehavioralTestResult>(testCases.Count);

        foreach (var testCase in testCases)
        {
            var scores = new Dictionary<string, float>();
            CallTrumpDecision? bestDecision = null;
            var bestScore = float.MinValue;

            foreach (var decision in testCase.ValidDecisions)
            {
                var scoreOrNull = Runner.TryScore(
                    engineProvider,
                    modelName,
                    testCase.CardsInHand,
                    testCase.UpCard,
                    testCase.DealerPosition ?? DealerPosition,
                    TeamScore,
                    OpponentScore,
                    decision,
                    1);

                if (scoreOrNull == null)
                {
                    return [new BehavioralTestResult(
                        Name,
                        DecisionType,
                        false,
                        "-",
                        AssertionDescription,
                        [],
                        $"Failed to load {DecisionType} model")];
                }

                var score = scoreOrNull.Value;
                var display = decision.ToString();
                scores[display] = score;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestDecision = decision;
                }
            }

            var isExpected = testCase.IsExpectedOverride ?? IsExpectedChoice;
            var passed = bestDecision.HasValue && isExpected(bestDecision.Value);
            var chosenDisplay = bestDecision.HasValue ? bestDecision.Value.ToString() : "-";
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

    protected static IReadOnlyList<CallTrumpTestCase> GenerateAllSuitVariants(
        string baseName,
        Func<Suit, Card[]> buildHand,
        Func<Suit, Card> buildUpCard,
        CallTrumpDecision[] validDecisions)
    {
        return [.. Enum.GetValues<Suit>()
            .Select(suit => new CallTrumpTestCase(
                $"{baseName} ({suit})",
                buildHand(suit),
                buildUpCard(suit),
                validDecisions))];
    }

    protected virtual IReadOnlyList<CallTrumpTestCase> GetTestCases()
    {
        return [new(Name, GetCardsInHand(), GetUpCard(), GetValidDecisions())];
    }

    protected virtual Card[] GetCardsInHand()
    {
        throw new InvalidOperationException("Override GetTestCases() or GetCardsInHand()");
    }

    protected virtual Card GetUpCard()
    {
        throw new InvalidOperationException("Override GetTestCases() or GetUpCard()");
    }

    protected virtual CallTrumpDecision[] GetValidDecisions()
    {
        throw new InvalidOperationException("Override GetTestCases() or GetValidDecisions()");
    }

    protected virtual bool IsExpectedChoice(CallTrumpDecision chosenDecision)
    {
        throw new InvalidOperationException("Override GetTestCases() or IsExpectedChoice()");
    }

    public record CallTrumpTestCase(
        string Label,
        Card[] CardsInHand,
        Card UpCard,
        CallTrumpDecision[] ValidDecisions,
        Func<CallTrumpDecision, bool>? IsExpectedOverride = null,
        RelativePlayerPosition? DealerPosition = null);
}
