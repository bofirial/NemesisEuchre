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
    IAdvancedPlayCardInferenceFeatureBuilder? advancedPlayCardFeatureBuilder = null,
    IAdvancedCallTrumpInferenceFeatureBuilder? advancedCallTrumpFeatureBuilder = null,
    IAdvancedDiscardCardInferenceFeatureBuilder? advancedDiscardCardFeatureBuilder = null) : BotBase(random)
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
    private readonly bool _hasExplicitPlayCardModel = actor.ModelNames?.ContainsKey("PlayCard") == true;
    private readonly bool _hasExplicitSimplePlayCardModel = actor.ModelNames?.ContainsKey("SimplePlayCard") == true;
    private readonly bool _hasExplicitAdvancedPlayCardModel = actor.ModelNames?.ContainsKey("AdvancedPlayCard") == true;
    private readonly IAdvancedCallTrumpInferenceFeatureBuilder? _advancedCallTrumpFeatureBuilder = advancedCallTrumpFeatureBuilder;
    private readonly IAdvancedDiscardCardInferenceFeatureBuilder? _advancedDiscardCardFeatureBuilder = advancedDiscardCardFeatureBuilder;
    private readonly PredictionEngine<AdvancedCallTrumpTrainingData, CallTrumpRegressionPrediction>? _advancedCallTrumpEngine = engineProvider.TryGetEngine<AdvancedCallTrumpTrainingData, CallTrumpRegressionPrediction>("AdvancedCallTrump", actor.GetModelName("AdvancedCallTrump") ?? string.Empty);
    private readonly PredictionEngine<AdvancedDiscardCardTrainingData, DiscardCardRegressionPrediction>? _advancedDiscardCardEngine = engineProvider.TryGetEngine<AdvancedDiscardCardTrainingData, DiscardCardRegressionPrediction>("AdvancedDiscardCard", actor.GetModelName("AdvancedDiscardCard") ?? string.Empty);
    private readonly bool _hasExplicitCallTrumpModel = actor.ModelNames?.ContainsKey("CallTrump") == true;
    private readonly bool _hasExplicitDiscardCardModel = actor.ModelNames?.ContainsKey("DiscardCard") == true;
    private readonly bool _hasExplicitAdvancedCallTrumpModel = actor.ModelNames?.ContainsKey("AdvancedCallTrump") == true;
    private readonly bool _hasExplicitAdvancedDiscardCardModel = actor.ModelNames?.ContainsKey("AdvancedDiscardCard") == true;

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
        Func<CallTrumpDecision, AdvancedCallTrumpTrainingData>? advancedBuilder = _advancedCallTrumpFeatureBuilder != null
            ? decision => _advancedCallTrumpFeatureBuilder.BuildFeatures(
                cardsInHand, upCard, dealerPosition, teamScore, opponentScore, decision, decisionNumber)
            : null;

        CallTrumpTrainingData BaseBuilder(CallTrumpDecision decision)
        {
            return _callTrumpFeatureBuilder.BuildFeatures(
                cardsInHand, upCard, dealerPosition, teamScore, opponentScore, decision, decisionNumber);
        }

        return TryPredictCallTrumpIfExplicit(_advancedCallTrumpEngine, validCallTrumpDecisions, advancedBuilder, "AdvancedCallTrump", _hasExplicitAdvancedCallTrumpModel)
            ?? TryPredictCallTrumpIfExplicit(_callTrumpEngine, validCallTrumpDecisions, BaseBuilder, "CallTrump", _hasExplicitCallTrumpModel)
            ?? TryPredictCallTrumpIfExplicit(_advancedCallTrumpEngine, validCallTrumpDecisions, advancedBuilder, "AdvancedCallTrump", !_hasExplicitAdvancedCallTrumpModel)
            ?? TryPredictCallTrumpIfExplicit(_callTrumpEngine, validCallTrumpDecisions, BaseBuilder, "CallTrump", !_hasExplicitCallTrumpModel)
            ?? throw new ModelUnavailableException(
                $"No CallTrump prediction engine could be loaded for actor '{Actor}'.");
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

        Func<RelativeCard, AdvancedDiscardCardTrainingData>? advancedBuilder = _advancedDiscardCardFeatureBuilder != null
            ? card => _advancedDiscardCardFeatureBuilder.BuildFeatures(
                cardsInHand, callingPlayer, callingPlayerGoingAlone, teamScore, opponentScore, card)
            : null;

        DiscardCardTrainingData BaseBuilder(RelativeCard card)
        {
            return _discardCardFeatureBuilder.BuildFeatures(
                cardsInHand, callingPlayer, callingPlayerGoingAlone, teamScore, opponentScore, card);
        }

        return TryPredictDiscardIfExplicit(_advancedDiscardCardEngine, validCardsToDiscard, advancedBuilder, "AdvancedDiscardCard", _hasExplicitAdvancedDiscardCardModel)
            ?? TryPredictDiscardIfExplicit(_discardCardEngine, validCardsToDiscard, BaseBuilder, "DiscardCard", _hasExplicitDiscardCardModel)
            ?? TryPredictDiscardIfExplicit(_advancedDiscardCardEngine, validCardsToDiscard, advancedBuilder, "AdvancedDiscardCard", !_hasExplicitAdvancedDiscardCardModel)
            ?? TryPredictDiscardIfExplicit(_discardCardEngine, validCardsToDiscard, BaseBuilder, "DiscardCard", !_hasExplicitDiscardCardModel)
            ?? throw new ModelUnavailableException(
                $"No DiscardCard prediction engine could be loaded for actor '{Actor}'.");
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
        Func<RelativeCard, AdvancedPlayCardTrainingData>? advancedBuilder = _advancedPlayCardFeatureBuilder != null
            ? card => _advancedPlayCardFeatureBuilder.BuildFeatures(
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
                card)
            : null;

        Func<RelativeCard, SimplePlayCardTrainingData>? simpleBuilder = _simplePlayCardFeatureBuilder != null
            ? card => _simplePlayCardFeatureBuilder.BuildFeatures(
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
                card)
            : null;

        PlayCardTrainingData PlayBuilder(RelativeCard card)
        {
            return _playCardFeatureBuilder.BuildFeatures(
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
                card);
        }

        // Explicit CLI overrides (-t2m-play, -t2m-simple-play, -t2m-advanced-play)
        // always take priority over engines loaded from the default model.
        // Within each tier, priority order is: Advanced > Simple > Play.
        return TryPredictIfExplicit(_advancedPlayCardEngine, validCardsToPlay, advancedBuilder, "AdvancedPlayCard", _hasExplicitAdvancedPlayCardModel)
            ?? TryPredictIfExplicit(_simplePlayCardEngine, validCardsToPlay, simpleBuilder, "SimplePlayCard", _hasExplicitSimplePlayCardModel)
            ?? TryPredictIfExplicit(_playCardEngine, validCardsToPlay, PlayBuilder, "PlayCard", _hasExplicitPlayCardModel)
            ?? TryPredictIfExplicit(_advancedPlayCardEngine, validCardsToPlay, advancedBuilder, "AdvancedPlayCard", !_hasExplicitAdvancedPlayCardModel)
            ?? TryPredictIfExplicit(_simplePlayCardEngine, validCardsToPlay, simpleBuilder, "SimplePlayCard", !_hasExplicitSimplePlayCardModel)
            ?? TryPredictIfExplicit(_playCardEngine, validCardsToPlay, PlayBuilder, "PlayCard", !_hasExplicitPlayCardModel)
            ?? throw new ModelUnavailableException(
                $"No PlayCard prediction engine could be loaded for actor '{Actor}'.");
    }

    private RelativeCardDecisionContext? TryPredictIfExplicit<TData>(
        PredictionEngine<TData, PlayCardRegressionPrediction>? engine,
        RelativeCard[] validCardsToPlay,
        Func<RelativeCard, TData>? buildFeatures,
        string engineName,
        bool condition)
        where TData : class, new()
    {
        if (!condition)
        {
            return null;
        }

        return TryPredictPlayCard(engine, validCardsToPlay, buildFeatures, engineName);
    }

    private RelativeCardDecisionContext? TryPredictPlayCard<TData>(
        PredictionEngine<TData, PlayCardRegressionPrediction>? engine,
        RelativeCard[] validCardsToPlay,
        Func<RelativeCard, TData>? buildFeatures,
        string engineName)
        where TData : class, new()
    {
        if (engine == null || buildFeatures == null)
        {
            return null;
        }

        var (bestOption, scores) = PredictBestOption(
            engine,
            validCardsToPlay,
            buildFeatures,
            prediction => prediction.PredictedPoints,
            engineName);

        return new RelativeCardDecisionContext
        {
            ChosenCard = bestOption,
            DecisionPredictedPoints = scores,
        };
    }

    private CallTrumpDecisionContext? TryPredictCallTrumpIfExplicit<TData>(
        PredictionEngine<TData, CallTrumpRegressionPrediction>? engine,
        CallTrumpDecision[] validDecisions,
        Func<CallTrumpDecision, TData>? buildFeatures,
        string engineName,
        bool condition)
        where TData : class, new()
    {
        if (!condition)
        {
            return null;
        }

        return TryPredictCallTrump(engine, validDecisions, buildFeatures, engineName);
    }

    private CallTrumpDecisionContext? TryPredictCallTrump<TData>(
        PredictionEngine<TData, CallTrumpRegressionPrediction>? engine,
        CallTrumpDecision[] validDecisions,
        Func<CallTrumpDecision, TData>? buildFeatures,
        string engineName)
        where TData : class, new()
    {
        if (engine == null || buildFeatures == null)
        {
            return null;
        }

        var (bestOption, scores) = PredictBestOption(
            engine,
            validDecisions,
            buildFeatures,
            prediction => prediction.PredictedPoints,
            engineName);

        return new CallTrumpDecisionContext
        {
            ChosenCallTrumpDecision = bestOption,
            DecisionPredictedPoints = scores,
        };
    }

    private RelativeCardDecisionContext? TryPredictDiscardIfExplicit<TData>(
        PredictionEngine<TData, DiscardCardRegressionPrediction>? engine,
        RelativeCard[] validCards,
        Func<RelativeCard, TData>? buildFeatures,
        string engineName,
        bool condition)
        where TData : class, new()
    {
        if (!condition)
        {
            return null;
        }

        return TryPredictDiscard(engine, validCards, buildFeatures, engineName);
    }

    private RelativeCardDecisionContext? TryPredictDiscard<TData>(
        PredictionEngine<TData, DiscardCardRegressionPrediction>? engine,
        RelativeCard[] validCards,
        Func<RelativeCard, TData>? buildFeatures,
        string engineName)
        where TData : class, new()
    {
        if (engine == null || buildFeatures == null)
        {
            return null;
        }

        var (bestOption, scores) = PredictBestOption(
            engine,
            validCards,
            buildFeatures,
            prediction => prediction.PredictedPoints,
            engineName);

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
