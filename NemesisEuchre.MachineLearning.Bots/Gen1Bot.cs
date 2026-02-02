using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerBots;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;

namespace NemesisEuchre.MachineLearning.Bots;

public class Gen1Bot : BotBase
{
    private readonly ILogger<Gen1Bot> _logger;
    private readonly ICallTrumpInferenceFeatureBuilder _callTrumpFeatureBuilder;
    private readonly IDiscardCardInferenceFeatureBuilder _discardCardFeatureBuilder;
    private readonly IPlayCardInferenceFeatureBuilder _playCardFeatureBuilder;
    private readonly PredictionEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>? _callTrumpEngine;
    private readonly PredictionEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>? _discardCardEngine;
    private readonly PredictionEngine<PlayCardTrainingData, PlayCardRegressionPrediction>? _playCardEngine;

    public Gen1Bot(
        IModelLoader modelLoader,
        IOptions<MachineLearningOptions> options,
        ICallTrumpInferenceFeatureBuilder callTrumpFeatureBuilder,
        IDiscardCardInferenceFeatureBuilder discardCardFeatureBuilder,
        IPlayCardInferenceFeatureBuilder playCardFeatureBuilder,
        ILogger<Gen1Bot> logger)
    {
        _logger = logger;
        _callTrumpFeatureBuilder = callTrumpFeatureBuilder ?? throw new ArgumentNullException(nameof(callTrumpFeatureBuilder));
        _discardCardFeatureBuilder = discardCardFeatureBuilder ?? throw new ArgumentNullException(nameof(discardCardFeatureBuilder));
        _playCardFeatureBuilder = playCardFeatureBuilder ?? throw new ArgumentNullException(nameof(playCardFeatureBuilder));

        var modelsDirectory = options.Value.ModelOutputPath;

        _callTrumpEngine = TryLoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(modelLoader, modelsDirectory, "CallTrump");
        _discardCardEngine = TryLoadModel<DiscardCardTrainingData, DiscardCardRegressionPrediction>(modelLoader, modelsDirectory, "DiscardCard");
        _playCardEngine = TryLoadModel<PlayCardTrainingData, PlayCardRegressionPrediction>(modelLoader, modelsDirectory, "PlayCard");
    }

    public override ActorType ActorType => ActorType.Gen1;

    public override async Task<CallTrumpDecision> CallTrumpAsync(
        Card[] cardsInHand,
        PlayerPosition playerPosition,
        short teamScore,
        short opponentScore,
        PlayerPosition dealerPosition,
        Card upCard,
        CallTrumpDecision[] validCallTrumpDecisions)
    {
        if (_callTrumpEngine == null)
        {
            return await SelectRandomAsync(validCallTrumpDecisions);
        }

        try
        {
            var bestDecision = CallTrumpDecision.Pass;
            var bestScore = float.MinValue;

            foreach (var decision in validCallTrumpDecisions)
            {
                var trainingData = _callTrumpFeatureBuilder.BuildFeatures(
                    cardsInHand,
                    upCard,
                    dealerPosition,
                    teamScore,
                    opponentScore,
                    validCallTrumpDecisions,
                    decision);

                var prediction = _callTrumpEngine.Predict(trainingData);

                if (prediction.PredictedPoints > bestScore)
                {
                    bestScore = prediction.PredictedPoints;
                    bestDecision = decision;
                }
            }

            return bestDecision;
        }
        catch (Exception ex)
        {
            LoggerMessages.LogCallTrumpPredictionError(_logger, ex);
            return await SelectRandomAsync(validCallTrumpDecisions);
        }
    }

    public override async Task<RelativeCard> DiscardCardAsync(
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

        if (_discardCardEngine == null)
        {
            return await SelectRandomAsync(validCardsToDiscard);
        }

        try
        {
            var bestCard = validCardsToDiscard[0];
            var bestScore = float.MinValue;

            foreach (var card in validCardsToDiscard)
            {
                var trainingData = _discardCardFeatureBuilder.BuildFeatures(
                    cardsInHand,
                    callingPlayer,
                    callingPlayerGoingAlone,
                    teamScore,
                    opponentScore,
                    card);

                var prediction = _discardCardEngine.Predict(trainingData);

                if (prediction.PredictedPoints > bestScore)
                {
                    bestScore = prediction.PredictedPoints;
                    bestCard = card;
                }
            }

            return bestCard;
        }
        catch (Exception ex)
        {
            LoggerMessages.LogDiscardCardPredictionError(_logger, ex);
            return await SelectRandomAsync(validCardsToDiscard);
        }
    }

    public override async Task<RelativeCard> PlayCardAsync(
        RelativeCard[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativePlayerPosition leadPlayer,
        RelativeSuit? leadSuit,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCards,
        RelativePlayerPosition? winningTrickPlayer,
        RelativeCard[] validCardsToPlay)
    {
        if (_playCardEngine == null)
        {
            return await SelectRandomAsync(validCardsToPlay);
        }

        try
        {
            var bestCard = validCardsToPlay[0];
            var bestScore = float.MinValue;

            foreach (var card in validCardsToPlay)
            {
                var trainingData = _playCardFeatureBuilder.BuildFeatures(
                    cardsInHand,
                    leadPlayer,
                    leadSuit,
                    playedCards,
                    teamScore,
                    opponentScore,
                    callingPlayer,
                    callingPlayerGoingAlone,
                    winningTrickPlayer,
                    validCardsToPlay,
                    card);

                var prediction = _playCardEngine.Predict(trainingData);

                if (prediction.PredictedPoints > bestScore)
                {
                    bestScore = prediction.PredictedPoints;
                    bestCard = card;
                }
            }

            return bestCard;
        }
        catch (Exception ex)
        {
            LoggerMessages.LogPlayCardPredictionError(_logger, ex);
            return await SelectRandomAsync(validCardsToPlay);
        }
    }

    private PredictionEngine<TData, TPrediction>? TryLoadModel<TData, TPrediction>(
        IModelLoader modelLoader,
        string modelsDirectory,
        string decisionType)
        where TData : class
        where TPrediction : class, new()
    {
        try
        {
            return modelLoader.LoadModel<TData, TPrediction>(
                modelsDirectory,
                generation: 1,
                decisionType,
                version: null);
        }
        catch (FileNotFoundException ex)
        {
            LoggerMessages.LogModelNotFound(_logger, decisionType, ex);
            return null;
        }
        catch (Exception ex)
        {
            LoggerMessages.LogModelLoadFailed(_logger, decisionType, ex);
            return null;
        }
    }
}
