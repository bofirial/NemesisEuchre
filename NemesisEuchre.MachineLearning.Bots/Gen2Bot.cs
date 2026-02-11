using Microsoft.Extensions.Logging;
using Microsoft.ML;

using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerBots;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.Bots;

public class Gen2Bot(
    IPredictionEngineProvider engineProvider,
    ICallTrumpInferenceFeatureBuilder callTrumpFeatureBuilder,
    IDiscardCardInferenceFeatureBuilder discardCardFeatureBuilder,
    IPlayCardInferenceFeatureBuilder playCardFeatureBuilder,
    IRandomNumberGenerator random,
    ILogger<Gen2Bot> logger) : BotBase(random)
{
    private readonly ILogger<Gen2Bot> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICallTrumpInferenceFeatureBuilder _callTrumpFeatureBuilder = callTrumpFeatureBuilder ?? throw new ArgumentNullException(nameof(callTrumpFeatureBuilder));
    private readonly IDiscardCardInferenceFeatureBuilder _discardCardFeatureBuilder = discardCardFeatureBuilder ?? throw new ArgumentNullException(nameof(discardCardFeatureBuilder));
    private readonly IPlayCardInferenceFeatureBuilder _playCardFeatureBuilder = playCardFeatureBuilder ?? throw new ArgumentNullException(nameof(playCardFeatureBuilder));
    private readonly PredictionEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>? _callTrumpEngine = engineProvider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "Gen2");
    private readonly PredictionEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>? _discardCardEngine = engineProvider.TryGetEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>("DiscardCard", "Gen2");
    private readonly PredictionEngine<PlayCardTrainingData, PlayCardRegressionPrediction>? _playCardEngine = engineProvider.TryGetEngine<PlayCardTrainingData, PlayCardRegressionPrediction>("PlayCard", "Gen2");

    public override ActorType ActorType => ActorType.Gen2;

    public override async Task<CallTrumpDecisionContext> CallTrumpAsync(
        Card[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition dealerPosition,
        Card upCard,
        CallTrumpDecision[] validCallTrumpDecisions)
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
                validCallTrumpDecisions,
                decision),
            prediction => prediction.PredictedPoints,
            LoggerMessages.LogCallTrumpEngineNotAvailable,
            LoggerMessages.LogCallTrumpPredictionError);

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
            LoggerMessages.LogDiscardCardEngineNotAvailable,
            LoggerMessages.LogDiscardCardPredictionError);

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
        RelativeCard[] validCardsToPlay)
    {
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
                validCardsToPlay,
                card),
            prediction => prediction.PredictedPoints,
            LoggerMessages.LogPlayCardEngineNotAvailable,
            LoggerMessages.LogPlayCardPredictionError);

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
        Action<ILogger> logEngineNotAvailable,
        Action<ILogger, Exception> logPredictionError)
        where TOption : notnull
        where TData : class, new()
        where TPrediction : class, new()
    {
        if (engine == null)
        {
            logEngineNotAvailable(_logger);
            return (SelectRandom(options), options.ToDictionary(o => o, _ => 0f));
        }

        try
        {
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
        catch (Exception ex)
        {
            logPredictionError(_logger, ex);
            return (SelectRandom(options), options.ToDictionary(o => o, _ => 0f));
        }
    }
}
