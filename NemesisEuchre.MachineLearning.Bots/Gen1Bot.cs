using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerBots;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;

namespace NemesisEuchre.MachineLearning.Bots;

public class Gen1Bot : BotBase
{
    private readonly ILogger<Gen1Bot> _logger;
    private readonly PredictionEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>? _callTrumpEngine;
    private readonly PredictionEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>? _discardCardEngine;
    private readonly PredictionEngine<PlayCardTrainingData, PlayCardRegressionPrediction>? _playCardEngine;

    public Gen1Bot(
        IModelLoader modelLoader,
        IOptions<MachineLearningOptions> options,
        ILogger<Gen1Bot> logger)
    {
        _logger = logger;
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
                var trainingData = CreateCallTrumpTrainingData(
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
                var trainingData = CreateDiscardCardTrainingData(
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
                var trainingData = CreatePlayCardTrainingData(
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

    private static CallTrumpTrainingData CreateCallTrumpTrainingData(
        Card[] cardsInHand,
        Card upCard,
        PlayerPosition dealerPosition,
        short teamScore,
        short opponentScore,
        CallTrumpDecision[] validDecisions,
        CallTrumpDecision chosenDecision)
    {
        var data = new CallTrumpTrainingData
        {
            Card1Rank = cardsInHand.Length > 0 ? (float)cardsInHand[0].Rank : 0f,
            Card1Suit = cardsInHand.Length > 0 ? (float)cardsInHand[0].Suit : 0f,
            Card2Rank = cardsInHand.Length > 1 ? (float)cardsInHand[1].Rank : 0f,
            Card2Suit = cardsInHand.Length > 1 ? (float)cardsInHand[1].Suit : 0f,
            Card3Rank = cardsInHand.Length > 2 ? (float)cardsInHand[2].Rank : 0f,
            Card3Suit = cardsInHand.Length > 2 ? (float)cardsInHand[2].Suit : 0f,
            Card4Rank = cardsInHand.Length > 3 ? (float)cardsInHand[3].Rank : 0f,
            Card4Suit = cardsInHand.Length > 3 ? (float)cardsInHand[3].Suit : 0f,
            Card5Rank = cardsInHand.Length > 4 ? (float)cardsInHand[4].Rank : 0f,
            Card5Suit = cardsInHand.Length > 4 ? (float)cardsInHand[4].Suit : 0f,
            UpCardRank = (float)upCard.Rank,
            UpCardSuit = (float)upCard.Suit,
            DealerPosition = (float)dealerPosition,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            DecisionOrder = 0f,
        };

        var validityArray = new float[11];
        foreach (var decision in validDecisions)
        {
            validityArray[(int)decision] = 1.0f;
        }

        data.Decision0IsValid = validityArray[0];
        data.Decision1IsValid = validityArray[1];
        data.Decision2IsValid = validityArray[2];
        data.Decision3IsValid = validityArray[3];
        data.Decision4IsValid = validityArray[4];
        data.Decision5IsValid = validityArray[5];
        data.Decision6IsValid = validityArray[6];
        data.Decision7IsValid = validityArray[7];
        data.Decision8IsValid = validityArray[8];
        data.Decision9IsValid = validityArray[9];
        data.Decision10IsValid = validityArray[10];

        var chosenIndex = (int)chosenDecision;
        data.Decision0Chosen = chosenIndex == 0 ? 1.0f : 0.0f;
        data.Decision1Chosen = chosenIndex == 1 ? 1.0f : 0.0f;
        data.Decision2Chosen = chosenIndex == 2 ? 1.0f : 0.0f;
        data.Decision3Chosen = chosenIndex == 3 ? 1.0f : 0.0f;
        data.Decision4Chosen = chosenIndex == 4 ? 1.0f : 0.0f;
        data.Decision5Chosen = chosenIndex == 5 ? 1.0f : 0.0f;
        data.Decision6Chosen = chosenIndex == 6 ? 1.0f : 0.0f;
        data.Decision7Chosen = chosenIndex == 7 ? 1.0f : 0.0f;
        data.Decision8Chosen = chosenIndex == 8 ? 1.0f : 0.0f;
        data.Decision9Chosen = chosenIndex == 9 ? 1.0f : 0.0f;
        data.Decision10Chosen = chosenIndex == 10 ? 1.0f : 0.0f;

        return data;
    }

    private static DiscardCardTrainingData CreateDiscardCardTrainingData(
        RelativeCard[] cardsInHand,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        short teamScore,
        short opponentScore,
        RelativeCard chosenCard)
    {
        var data = new DiscardCardTrainingData
        {
            Card1Rank = (float)cardsInHand[0].Rank,
            Card1Suit = (float)cardsInHand[0].Suit,
            Card2Rank = (float)cardsInHand[1].Rank,
            Card2Suit = (float)cardsInHand[1].Suit,
            Card3Rank = (float)cardsInHand[2].Rank,
            Card3Suit = (float)cardsInHand[2].Suit,
            Card4Rank = (float)cardsInHand[3].Rank,
            Card4Suit = (float)cardsInHand[3].Suit,
            Card5Rank = (float)cardsInHand[4].Rank,
            Card5Suit = (float)cardsInHand[4].Suit,
            Card6Rank = (float)cardsInHand[5].Rank,
            Card6Suit = (float)cardsInHand[5].Suit,
            CallingPlayerPosition = (float)callingPlayer,
            CallingPlayerGoingAlone = callingPlayerGoingAlone ? 1.0f : 0.0f,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
        };

        var chosenIndex = Array.IndexOf(cardsInHand, chosenCard);
        data.Card1Chosen = chosenIndex == 0 ? 1.0f : 0.0f;
        data.Card2Chosen = chosenIndex == 1 ? 1.0f : 0.0f;
        data.Card3Chosen = chosenIndex == 2 ? 1.0f : 0.0f;
        data.Card4Chosen = chosenIndex == 3 ? 1.0f : 0.0f;
        data.Card5Chosen = chosenIndex == 4 ? 1.0f : 0.0f;
        data.Card6Chosen = chosenIndex == 5 ? 1.0f : 0.0f;

        return data;
    }

    private static PlayCardTrainingData CreatePlayCardTrainingData(
        RelativeCard[] cardsInHand,
        RelativePlayerPosition leadPlayer,
        RelativeSuit? leadSuit,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCards,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativePlayerPosition? winningTrickPlayer,
        RelativeCard[] validCardsToPlay,
        RelativeCard chosenCard)
    {
        var data = new PlayCardTrainingData
        {
            Card1Rank = cardsInHand.Length > 0 ? (float)cardsInHand[0].Rank : 0f,
            Card1Suit = cardsInHand.Length > 0 ? (float)cardsInHand[0].Suit : 0f,
            Card2Rank = cardsInHand.Length > 1 ? (float)cardsInHand[1].Rank : 0f,
            Card2Suit = cardsInHand.Length > 1 ? (float)cardsInHand[1].Suit : 0f,
            Card3Rank = cardsInHand.Length > 2 ? (float)cardsInHand[2].Rank : 0f,
            Card3Suit = cardsInHand.Length > 2 ? (float)cardsInHand[2].Suit : 0f,
            Card4Rank = cardsInHand.Length > 3 ? (float)cardsInHand[3].Rank : 0f,
            Card4Suit = cardsInHand.Length > 3 ? (float)cardsInHand[3].Suit : 0f,
            Card5Rank = cardsInHand.Length > 4 ? (float)cardsInHand[4].Rank : 0f,
            Card5Suit = cardsInHand.Length > 4 ? (float)cardsInHand[4].Suit : 0f,
            LeadPlayer = (float)leadPlayer,
            LeadSuit = leadSuit.HasValue ? (float)leadSuit.Value : -1.0f,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            TrickNumber = 0f,
            CardsPlayedInTrick = playedCards.Count,
            WinningTrickPlayer = winningTrickPlayer.HasValue ? (float)winningTrickPlayer.Value : -1.0f,
            CallingPlayerPosition = (float)callingPlayer,
            CallingPlayerGoingAlone = callingPlayerGoingAlone ? 1.0f : 0.0f,
        };

        playedCards.TryGetValue(RelativePlayerPosition.Self, out var card0);
        playedCards.TryGetValue(RelativePlayerPosition.LeftHandOpponent, out var card1);
        playedCards.TryGetValue(RelativePlayerPosition.Partner, out var card2);

        data.PlayedCard1Rank = card0 != null ? (float)card0.Rank : 0f;
        data.PlayedCard1Suit = card0 != null ? (float)card0.Suit : 0f;
        data.PlayedCard2Rank = card1 != null ? (float)card1.Rank : 0f;
        data.PlayedCard2Suit = card1 != null ? (float)card1.Suit : 0f;
        data.PlayedCard3Rank = card2 != null ? (float)card2.Rank : 0f;
        data.PlayedCard3Suit = card2 != null ? (float)card2.Suit : 0f;

        var validityArray = new float[5];
        foreach (var card in validCardsToPlay)
        {
            var index = Array.IndexOf(cardsInHand, card);
            if (index is >= 0 and < 5)
            {
                validityArray[index] = 1.0f;
            }
        }

        data.Card1IsValid = validityArray[0];
        data.Card2IsValid = validityArray[1];
        data.Card3IsValid = validityArray[2];
        data.Card4IsValid = validityArray[3];
        data.Card5IsValid = validityArray[4];

        var chosenIndex = Array.IndexOf(cardsInHand, chosenCard);
        data.Card1Chosen = chosenIndex == 0 ? 1.0f : 0.0f;
        data.Card2Chosen = chosenIndex == 1 ? 1.0f : 0.0f;
        data.Card3Chosen = chosenIndex == 2 ? 1.0f : 0.0f;
        data.Card4Chosen = chosenIndex == 3 ? 1.0f : 0.0f;
        data.Card5Chosen = chosenIndex == 4 ? 1.0f : 0.0f;

        return data;
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
