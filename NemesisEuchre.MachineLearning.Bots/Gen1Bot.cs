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

public class Gen1Bot(
    IPredictionEngineProvider engineProvider,
    ICallTrumpInferenceFeatureBuilder callTrumpFeatureBuilder,
    IDiscardCardInferenceFeatureBuilder discardCardFeatureBuilder,
    IPlayCardInferenceFeatureBuilder playCardFeatureBuilder,
    IRandomNumberGenerator random,
    ILogger<Gen1Bot> logger) : BotBase(random)
{
    private readonly ILogger<Gen1Bot> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICallTrumpInferenceFeatureBuilder _callTrumpFeatureBuilder = callTrumpFeatureBuilder ?? throw new ArgumentNullException(nameof(callTrumpFeatureBuilder));
    private readonly IDiscardCardInferenceFeatureBuilder _discardCardFeatureBuilder = discardCardFeatureBuilder ?? throw new ArgumentNullException(nameof(discardCardFeatureBuilder));
    private readonly IPlayCardInferenceFeatureBuilder _playCardFeatureBuilder = playCardFeatureBuilder ?? throw new ArgumentNullException(nameof(playCardFeatureBuilder));
    private readonly PredictionEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>? _callTrumpEngine = engineProvider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", generation: 1);
    private readonly PredictionEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>? _discardCardEngine = engineProvider.TryGetEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>("DiscardCard", generation: 1);
    private readonly PredictionEngine<PlayCardTrainingData, PlayCardRegressionPrediction>? _playCardEngine = engineProvider.TryGetEngine<PlayCardTrainingData, PlayCardRegressionPrediction>("PlayCard", generation: 1);

    public override ActorType ActorType => ActorType.Gen1;

    public override async Task<CallTrumpDecisionContext> CallTrumpAsync(
        Card[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition dealerPosition,
        Card upCard,
        CallTrumpDecision[] validCallTrumpDecisions)
    {
        if (_callTrumpEngine == null)
        {
            LoggerMessages.LogCallTrumpEngineNotAvailable(_logger);

            return new CallTrumpDecisionContext()
            {
                ChosenCallTrumpDecision = SelectRandom(validCallTrumpDecisions),
                DecisionPredictedPoints = validCallTrumpDecisions.ToDictionary(d => d, _ => 0f),
            };
        }

        try
        {
            var bestDecision = CallTrumpDecision.Pass;
            var bestScore = float.MinValue;
            var decisionScores = new Dictionary<CallTrumpDecision, float>();

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

                decisionScores.Add(decision, prediction.PredictedPoints);

                if (prediction.PredictedPoints > bestScore)
                {
                    bestScore = prediction.PredictedPoints;
                    bestDecision = decision;
                }
            }

            return new CallTrumpDecisionContext()
            {
                ChosenCallTrumpDecision = bestDecision,
                DecisionPredictedPoints = decisionScores,
            };
        }
        catch (Exception ex)
        {
            LoggerMessages.LogCallTrumpPredictionError(_logger, ex);

            return new CallTrumpDecisionContext()
            {
                ChosenCallTrumpDecision = SelectRandom(validCallTrumpDecisions),
                DecisionPredictedPoints = validCallTrumpDecisions.ToDictionary(d => d, _ => 0f),
            };
        }
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

        if (_discardCardEngine == null)
        {
            LoggerMessages.LogDiscardCardEngineNotAvailable(_logger);

            return new RelativeCardDecisionContext()
            {
                ChosenCard = SelectRandom(validCardsToDiscard),
                DecisionPredictedPoints = validCardsToDiscard.ToDictionary(d => d, _ => 0f),
            };
        }

        try
        {
            var bestCard = validCardsToDiscard[0];
            var bestScore = float.MinValue;
            var decisionScores = new Dictionary<RelativeCard, float>();

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

                decisionScores.Add(card, prediction.PredictedPoints);

                if (prediction.PredictedPoints > bestScore)
                {
                    bestScore = prediction.PredictedPoints;
                    bestCard = card;
                }
            }

            return new RelativeCardDecisionContext()
            {
                ChosenCard = bestCard,
                DecisionPredictedPoints = decisionScores,
            };
        }
        catch (Exception ex)
        {
            LoggerMessages.LogDiscardCardPredictionError(_logger, ex);

            return new RelativeCardDecisionContext()
            {
                ChosenCard = SelectRandom(validCardsToDiscard),
                DecisionPredictedPoints = validCardsToDiscard.ToDictionary(d => d, _ => 0f),
            };
        }
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
        if (_playCardEngine == null)
        {
            LoggerMessages.LogPlayCardEngineNotAvailable(_logger);

            return new RelativeCardDecisionContext()
            {
                ChosenCard = SelectRandom(validCardsToPlay),
                DecisionPredictedPoints = validCardsToPlay.ToDictionary(d => d, _ => 0f),
            };
        }

        try
        {
            var bestCard = validCardsToPlay[0];
            var bestScore = float.MinValue;
            var decisionScores = new Dictionary<RelativeCard, float>();

            foreach (var card in validCardsToPlay)
            {
                var trainingData = _playCardFeatureBuilder.BuildFeatures(
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
                    card);

                var prediction = _playCardEngine.Predict(trainingData);

                decisionScores.Add(card, prediction.PredictedPoints);

                if (prediction.PredictedPoints > bestScore)
                {
                    bestScore = prediction.PredictedPoints;
                    bestCard = card;
                }
            }

            return new RelativeCardDecisionContext()
            {
                ChosenCard = bestCard,
                DecisionPredictedPoints = decisionScores,
            };
        }
        catch (Exception ex)
        {
            LoggerMessages.LogPlayCardPredictionError(_logger, ex);

            return new RelativeCardDecisionContext()
            {
                ChosenCard = SelectRandom(validCardsToPlay),
                DecisionPredictedPoints = validCardsToPlay.ToDictionary(d => d, _ => 0f),
            };
        }
    }
}
