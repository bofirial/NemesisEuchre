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

    public IReadOnlyList<BehavioralTestResult> Run(IPredictionEngineProvider engineProvider, string modelName)
    {
        var engine = engineProvider.TryGetEngine<PlayCardTrainingData, PlayCardRegressionPrediction>(
            "PlayCard", modelName);

        if (engine == null)
        {
            return [new BehavioralTestResult(
                Name,
                DecisionType,
                false,
                "-",
                AssertionDescription,
                [],
                "Failed to load PlayCard model")];
        }

        var testCases = GetTestCases();
        var results = new List<BehavioralTestResult>(testCases.Count);

        foreach (var testCase in testCases)
        {
            var scores = new Dictionary<string, float>();
            RelativeCard? bestCard = null;
            var bestScore = float.MinValue;

            foreach (var card in testCase.ValidCardsToPlay)
            {
                RelativeCard[] cardsAccountedFor = [
                    .. testCase.CardsAccountedFor ?? CardsAccountedFor,
                    .. testCase.CardsInHand,
                    .. PlayedCardsInTrick.Values
                ];

                var features = featureBuilder.BuildFeatures(
                    testCase.CardsInHand,
                    LeadPlayer,
                    LeadSuit,
                    PlayedCardsInTrick,
                    TeamScore,
                    OpponentScore,
                    CallingPlayer,
                    CallingPlayerGoingAlone,
                    Dealer,
                    DealerPickedUpCard,
                    testCase.KnownPlayerSuitVoids ?? KnownPlayerSuitVoids,
                    [.. cardsAccountedFor.Distinct()],
                    WinningTrickPlayer,
                    TrickNumber,
                    testCase.ValidCardsToPlay,
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

            var isExpected = testCase.IsExpectedOverride ?? IsExpectedChoice;

            var passed = bestCard != null && isExpected(bestCard);
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

    protected virtual IReadOnlyList<PlayCardTestCase> GetTestCases()
    {
        return [new(Name, GetCardsInHand(), GetValidCardsToPlay())];
    }

    protected virtual RelativeCard[] GetCardsInHand()
    {
        throw new InvalidOperationException("Override GetTestCases() or GetCardsInHand()");
    }

    protected virtual RelativeCard[] GetValidCardsToPlay()
    {
        throw new InvalidOperationException("Override GetTestCases() or GetValidCardsToPlay()");
    }

    protected virtual bool IsExpectedChoice(RelativeCard chosenCard)
    {
        throw new InvalidOperationException("Override GetTestCases() or IsExpectedChoice()");
    }

    private static string FormatCard(RelativeCard card)
    {
        return $"{card.Rank} of {card.Suit}";
    }

    public record PlayCardTestCase(
        string Label,
        RelativeCard[] CardsInHand,
        RelativeCard[] ValidCardsToPlay,
        Func<RelativeCard, bool>? IsExpectedOverride = null,
        RelativeCard[]? CardsAccountedFor = null,
        RelativePlayerSuitVoid[]? KnownPlayerSuitVoids = null);
}
