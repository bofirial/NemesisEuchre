using Microsoft.ML;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerBots;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;
using NemesisEuchre.MachineLearning.Bots.Exceptions;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.Bots;

public class ModelBot(
    IPredictionEngineProvider engineProvider,
    ICallTrumpInferenceFeatureBuilder callTrumpFeatureBuilder,
    IDiscardCardInferenceFeatureBuilder discardCardFeatureBuilder,
    IPlayCardInferenceFeatureBuilder playCardFeatureBuilder,
    IRandomNumberGenerator random,
    Actor actor,
    ISimplePlayCardInferenceFeatureBuilder? simplePlayCardFeatureBuilder = null,
    IAdvancedPlayCardInferenceFeatureBuilder? advancedPlayCardFeatureBuilder = null) : BotBase(random)
{
    private readonly ICallTrumpInferenceFeatureBuilder _callTrumpFeatureBuilder = callTrumpFeatureBuilder ?? throw new ArgumentNullException(nameof(callTrumpFeatureBuilder));
    private readonly IDiscardCardInferenceFeatureBuilder _discardCardFeatureBuilder = discardCardFeatureBuilder ?? throw new ArgumentNullException(nameof(discardCardFeatureBuilder));
    private readonly IPlayCardInferenceFeatureBuilder _playCardFeatureBuilder = playCardFeatureBuilder ?? throw new ArgumentNullException(nameof(playCardFeatureBuilder));
    private readonly ISimplePlayCardInferenceFeatureBuilder? _simplePlayCardFeatureBuilder = simplePlayCardFeatureBuilder;
    private readonly IAdvancedPlayCardInferenceFeatureBuilder? _advancedPlayCardFeatureBuilder = advancedPlayCardFeatureBuilder;
    private readonly PredictionEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>? _callTrumpEngine = engineProvider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", actor.GetModelName("CallTrump") ?? string.Empty);
    private readonly PredictionEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>? _discardCardEngine = engineProvider.TryGetEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>("DiscardCard", actor.GetModelName("DiscardCard") ?? string.Empty);
    private readonly PredictionEngine<PlayCardTrainingData, PlayCardRegressionPrediction>? _playCardEngine = engineProvider.TryGetEngine<PlayCardTrainingData, PlayCardRegressionPrediction>("PlayCard", actor.GetModelName("PlayCard") ?? string.Empty);
    private readonly PredictionEngine<SimplePlayCardTrainingData, PlayCardRegressionPrediction>? _simplePlayCardEngine = engineProvider.TryGetEngine<SimplePlayCardTrainingData, PlayCardRegressionPrediction>("SimplePlayCard", actor.GetModelName("SimplePlayCard") ?? string.Empty);
    private readonly PredictionEngine<AdvancedPlayCardTrainingData, PlayCardRegressionPrediction>? _advancedPlayCardEngine = engineProvider.TryGetEngine<AdvancedPlayCardTrainingData, PlayCardRegressionPrediction>("AdvancedPlayCard", actor.GetModelName("AdvancedPlayCard") ?? string.Empty);

    public override ActorType ActorType => ActorType.Model;

    protected Actor Actor { get; } = actor;

    public override async Task<CallTrumpDecisionContext> CallTrumpAsync(
        Card[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition dealerPosition,
        Card upCard,
        CallTrumpDecision[] validCallTrumpDecisions,
        byte decisionNumber)
    {
        var (bestOption, scores) = PredictBestOption(
            _callTrumpEngine,
            validCallTrumpDecisions,
            decision => _callTrumpFeatureBuilder.BuildFeatures(
                cardsInHand,
                upCard,
                dealerPosition,
                teamScore,
                opponentScore,
                decision,
                decisionNumber),
            prediction => prediction.PredictedPoints,
            "CallTrump");

        return new CallTrumpDecisionContext
        {
            ChosenCallTrumpDecision = bestOption,
            DecisionPredictedPoints = scores,
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
        if (cardsInHand.Length != 6)
        {
            throw new InvalidOperationException($"Expected 6 cards in hand for discard, got {cardsInHand.Length}");
        }

        var (bestOption, scores) = PredictBestOption(
            _discardCardEngine,
            validCardsToDiscard,
            card => _discardCardFeatureBuilder.BuildFeatures(
                cardsInHand,
                callingPlayer,
                callingPlayerGoingAlone,
                teamScore,
                opponentScore,
                card),
            prediction => prediction.PredictedPoints,
            "DiscardCard");

        return new RelativeCardDecisionContext
        {
            ChosenCard = bestOption,
            DecisionPredictedPoints = scores,
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
        if (_advancedPlayCardEngine != null && _advancedPlayCardFeatureBuilder != null)
        {
            var (bestAdvancedOption, advancedScores) = PredictBestOption(
                _advancedPlayCardEngine,
                validCardsToPlay,
                card => _advancedPlayCardFeatureBuilder.BuildFeatures(
                    cardsInHand,
                    leadPlayer,
                    leadSuit,
                    playedCardsInTrick,
                    teamScore,
                    opponentScore,
                    callingPlayer,
                    callingPlayerGoingAlone,
                    dealer,
                    dealerPickedUpCard,
                    knownPlayerSuitVoids,
                    cardsAccountedFor,
                    currentlyWinningTrickPlayer,
                    trickNumber,
                    wonTricks,
                    opponentsWonTricks,
                    card),
                prediction => prediction.PredictedPoints,
                "AdvancedPlayCard");

            return new RelativeCardDecisionContext
            {
                ChosenCard = bestAdvancedOption,
                DecisionPredictedPoints = advancedScores,
            };
        }

        if (_simplePlayCardEngine != null && _simplePlayCardFeatureBuilder != null)
        {
            var (bestSimpleOption, simpleScores) = PredictBestOption(
                _simplePlayCardEngine,
                validCardsToPlay,
                card => _simplePlayCardFeatureBuilder.BuildFeatures(
                    cardsInHand,
                    leadPlayer,
                    leadSuit,
                    playedCardsInTrick,
                    teamScore,
                    opponentScore,
                    callingPlayer,
                    callingPlayerGoingAlone,
                    dealer,
                    dealerPickedUpCard,
                    knownPlayerSuitVoids,
                    cardsAccountedFor,
                    currentlyWinningTrickPlayer,
                    trickNumber,
                    wonTricks,
                    opponentsWonTricks,
                    card),
                prediction => prediction.PredictedPoints,
                "SimplePlayCard");

            return new RelativeCardDecisionContext
            {
                ChosenCard = bestSimpleOption,
                DecisionPredictedPoints = simpleScores,
            };
        }

        var (bestOption, scores) = PredictBestOption(
            _playCardEngine,
            validCardsToPlay,
            card => _playCardFeatureBuilder.BuildFeatures(
                cardsInHand,
                leadPlayer,
                leadSuit,
                playedCardsInTrick,
                teamScore,
                opponentScore,
                callingPlayer,
                callingPlayerGoingAlone,
                dealer,
                dealerPickedUpCard,
                knownPlayerSuitVoids,
                cardsAccountedFor,
                currentlyWinningTrickPlayer,
                trickNumber,
                wonTricks,
                opponentsWonTricks,
                card),
            prediction => prediction.PredictedPoints,
            "PlayCard");

        return new RelativeCardDecisionContext
        {
            ChosenCard = bestOption,
            DecisionPredictedPoints = scores,
        };
    }

    private (TOption bestOption, Dictionary<TOption, float> scores) PredictBestOption<TOption, TData, TPrediction>(
        PredictionEngine<TData, TPrediction>? engine,
        TOption[] options,
        Func<TOption, TData> buildFeatures,
        Func<TPrediction, float> getScore,
        string engineName)
        where TOption : notnull
        where TData : class, new()
        where TPrediction : class, new()
    {
        if (engine == null)
        {
            throw new ModelUnavailableException(
                $"The '{engineName}' prediction engine for model '{Actor.GetModelName(engineName)}' could not be loaded.");
        }

        var bestOption = options[0];
        var bestScore = float.MinValue;
        var scores = new Dictionary<TOption, float>();

        foreach (var option in options)
        {
            var trainingData = buildFeatures(option);
            var prediction = engine.Predict(trainingData);
            var score = getScore(prediction);

            scores.Add(option, score);

            if (score > bestScore)
            {
                bestScore = score;
                bestOption = option;
            }
        }

        return (bestOption, scores);
    }
}
