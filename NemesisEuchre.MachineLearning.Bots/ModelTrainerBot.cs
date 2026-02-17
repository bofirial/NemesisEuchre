using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Selection;
using NemesisEuchre.GameEngine.Utilities;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Options;

namespace NemesisEuchre.MachineLearning.Bots;

public class ModelTrainerBot(
    IPredictionEngineProvider engineProvider,
    ICallTrumpInferenceFeatureBuilder callTrumpFeatureBuilder,
    IDiscardCardInferenceFeatureBuilder discardCardFeatureBuilder,
    IPlayCardInferenceFeatureBuilder playCardFeatureBuilder,
    IRandomNumberGenerator random,
    IOptions<MachineLearningOptions> machineLearningOptions,
    ILogger<ModelTrainerBot> logger,
    Actor actor) : ModelBot(
        engineProvider,
        callTrumpFeatureBuilder,
        discardCardFeatureBuilder,
        playCardFeatureBuilder,
        random,
        logger,
        actor)
{
    public override ActorType ActorType => ActorType.ModelTrainer;

    private float Temperature => Actor.ExplorationTemperature != default ? Actor.ExplorationTemperature : machineLearningOptions.Value.ExplorationTemperature;

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

        if (decisionContext.DecisionPredictedPoints.Count == 0 || Actor.ExplorationDecisionType is not DecisionType.All or DecisionType.CallTrump)
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

        if (decisionContext.DecisionPredictedPoints.Count == 0 || Actor.ExplorationDecisionType is not DecisionType.All or DecisionType.Discard)
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
        short wonTricks,
        short opponentsWonTricks,
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
            wonTricks,
            opponentsWonTricks,
            validCardsToPlay);

        if (decisionContext.DecisionPredictedPoints.Count == 0 || Actor.ExplorationDecisionType is not DecisionType.All or DecisionType.Play)
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
