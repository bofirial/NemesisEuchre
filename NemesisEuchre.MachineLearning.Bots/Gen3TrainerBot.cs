using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Selection;
using NemesisEuchre.GameEngine.Utilities;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;

namespace NemesisEuchre.MachineLearning.Bots;

public class Gen3TrainerBot(
    IPredictionEngineProvider engineProvider,
    ICallTrumpInferenceFeatureBuilder callTrumpFeatureBuilder,
    IDiscardCardInferenceFeatureBuilder discardCardFeatureBuilder,
    IPlayCardInferenceFeatureBuilder playCardFeatureBuilder,
    IRandomNumberGenerator random,
    ILogger<Gen3TrainerBot> logger) : Gen3Bot(
        engineProvider,
        callTrumpFeatureBuilder,
        discardCardFeatureBuilder,
        playCardFeatureBuilder,
        random,
        logger)
{
    private const float Temperature = 0.2f;

    public override ActorType ActorType => ActorType.Gen1Trainer;

    public override async Task<CallTrumpDecisionContext> CallTrumpAsync(
        Card[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition dealerPosition,
        Card upCard,
        CallTrumpDecision[] validCallTrumpDecisions)
    {
        var decisionContext = await base.CallTrumpAsync(
            cardsInHand,
            teamScore,
            opponentScore,
            dealerPosition,
            upCard,
            validCallTrumpDecisions);

        if (decisionContext.DecisionPredictedPoints.Count == 0)
        {
            return decisionContext;
        }

        var options = decisionContext.DecisionPredictedPoints.Keys.ToList();
        var scores = decisionContext.DecisionPredictedPoints.Values.ToList();

        var selectedDecision = BoltzmannSelector.SelectWeighted(
            options,
            scores,
            Temperature,
            Random);

        return new CallTrumpDecisionContext
        {
            ChosenCallTrumpDecision = selectedDecision,
            DecisionPredictedPoints = decisionContext.DecisionPredictedPoints,
        };
    }

    public override async Task<RelativeCardDecisionContext> DiscardCardAsync(
        RelativeCard[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativeCard[] validCardsToDiscard)
    {
        var decisionContext = await base.DiscardCardAsync(
            cardsInHand,
            teamScore,
            opponentScore,
            callingPlayer,
            callingPlayerGoingAlone,
            validCardsToDiscard);

        if (decisionContext.DecisionPredictedPoints.Count == 0)
        {
            return decisionContext;
        }

        var options = decisionContext.DecisionPredictedPoints.Keys.ToList();
        var scores = decisionContext.DecisionPredictedPoints.Values.ToList();

        var selectedCard = BoltzmannSelector.SelectWeighted(
            options,
            scores,
            Temperature,
            Random);

        return new RelativeCardDecisionContext
        {
            ChosenCard = selectedCard,
            DecisionPredictedPoints = decisionContext.DecisionPredictedPoints,
        };
    }

    public override async Task<RelativeCardDecisionContext> PlayCardAsync(
        RelativeCard[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativePlayerPosition dealer,
        RelativeCard? dealerPickedUpCard,
        RelativePlayerPosition leadPlayer,
        RelativeSuit? leadSuit,
        RelativePlayerSuitVoid[] knownPlayerSuitVoids,
        RelativeCard[] cardsAccountedFor,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCardsInTrick,
        RelativePlayerPosition? currentlyWinningTrickPlayer,
        short trickNumber,
        RelativeCard[] validCardsToPlay)
    {
        var decisionContext = await base.PlayCardAsync(
            cardsInHand,
            teamScore,
            opponentScore,
            callingPlayer,
            callingPlayerGoingAlone,
            dealer,
            dealerPickedUpCard,
            leadPlayer,
            leadSuit,
            knownPlayerSuitVoids,
            cardsAccountedFor,
            playedCardsInTrick,
            currentlyWinningTrickPlayer,
            trickNumber,
            validCardsToPlay);

        if (decisionContext.DecisionPredictedPoints.Count == 0)
        {
            return decisionContext;
        }

        var options = decisionContext.DecisionPredictedPoints.Keys.ToList();
        var scores = decisionContext.DecisionPredictedPoints.Values.ToList();

        var selectedCard = BoltzmannSelector.SelectWeighted(
            options,
            scores,
            Temperature,
            Random);

        return new RelativeCardDecisionContext
        {
            ChosenCard = selectedCard,
            DecisionPredictedPoints = decisionContext.DecisionPredictedPoints,
        };
    }
}
