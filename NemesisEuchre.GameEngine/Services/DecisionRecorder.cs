using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Services;

public interface IDecisionRecorder
{
    void RecordCallTrumpDecision(
        Deal deal,
        PlayerPosition playerPosition,
        CallTrumpDecision[] validDecisions,
        CallTrumpDecisionContext callTrumpDecisionContext,
        ref byte decisionOrderCounter);

    void RecordPlayCardDecision(
        Deal deal,
        Trick trick,
        PlayerPosition playerPosition,
        Card[] hand,
        Card[] validCards,
        CardDecisionContext cardDecisionContext,
        ITrickWinnerCalculator trickWinnerCalculator);

    void RecordDiscardDecision(
        Deal deal,
        PlayerPosition playerPosition,
        Card[] hand,
        CardDecisionContext cardDecisionContext);
}

public class DecisionRecorder(IPlayerContextBuilder contextBuilder, ICardAccountingService cardAccountingService) : IDecisionRecorder
{
    public void RecordCallTrumpDecision(
        Deal deal,
        PlayerPosition playerPosition,
        CallTrumpDecision[] validDecisions,
        CallTrumpDecisionContext callTrumpDecisionContext,
        ref byte decisionOrderCounter)
    {
        var player = deal.Players[playerPosition];
        var (teamScore, opponentScore) = contextBuilder.GetScores(deal, playerPosition);

        var record = new CallTrumpDecisionRecord
        {
            CardsInHand = [.. player.CurrentHand],
            UpCard = deal.UpCard!,
            DealerPosition = deal.DealerPosition!.Value,
            PlayerPosition = playerPosition,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            ValidCallTrumpDecisions = [.. validDecisions],
            ChosenDecision = callTrumpDecisionContext.ChosenCallTrumpDecision,
            DecisionPredictedPoints = callTrumpDecisionContext.DecisionPredictedPoints,
            DecisionOrder = ++decisionOrderCounter,
        };

        deal.CallTrumpDecisions.Add(record);
    }

    public void RecordPlayCardDecision(
        Deal deal,
        Trick trick,
        PlayerPosition playerPosition,
        Card[] hand,
        Card[] validCards,
        CardDecisionContext cardDecisionContext,
        ITrickWinnerCalculator trickWinnerCalculator)
    {
        var (teamScore, opponentScore) = contextBuilder.GetScores(deal, playerPosition);

        var playerTeam = playerPosition.GetTeam();
        var wonTricks = (short)deal.CompletedTricks.Count(t => t.WinningTeam == playerTeam);
        var opponentsWonTricks = (short)deal.CompletedTricks.Count(t => t.WinningTeam != null && t.WinningTeam != playerTeam);

        var accountedForCards = cardAccountingService.GetAccountedForCards(
            deal,
            trick,
            playerPosition,
            hand);

        var record = new PlayCardDecisionRecord
        {
            CardsInHand = [.. hand],
            PlayerPosition = playerPosition,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            WonTricks = wonTricks,
            OpponentsWonTricks = opponentsWonTricks,
            TrumpSuit = deal.Trump!.Value,
            LeadPlayer = trick.LeadPosition,
            LeadSuit = trick.LeadSuit,
            PlayedCards = trick.CardsPlayed.ToDictionary(
                pc => pc.PlayerPosition,
                pc => pc.Card),
            WinningTrickPlayer = trick.CardsPlayed.Count > 0 && trick.LeadSuit.HasValue
                ? trickWinnerCalculator.CalculateWinner(trick, deal.Trump!.Value)
                : null,
            TrickNumber = trick.TrickNumber,
            ValidCardsToPlay = [.. validCards],
            CallingPlayer = deal.CallingPlayer!.Value,
            CallingPlayerGoingAlone = deal.CallingPlayerIsGoingAlone,
            KnownPlayerSuitVoids = [.. deal.KnownPlayerSuitVoids],
            Dealer = deal.DealerPosition!.Value,
            DealerPickedUpCard = deal.UpCard,
            CardsAccountedFor = [.. accountedForCards],
            ChosenCard = cardDecisionContext.ChosenCard,
            DecisionPredictedPoints = cardDecisionContext.DecisionPredictedPoints,
        };

        trick.PlayCardDecisions.Add(record);
    }

    public void RecordDiscardDecision(
        Deal deal,
        PlayerPosition playerPosition,
        Card[] hand,
        CardDecisionContext cardDecisionContext)
    {
        var (teamScore, opponentScore) = contextBuilder.GetScores(deal, playerPosition);

        var record = new DiscardCardDecisionRecord
        {
            CardsInHand = [.. hand],
            PlayerPosition = playerPosition,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            TrumpSuit = deal.Trump!.Value,
            CallingPlayer = deal.CallingPlayer!.Value,
            CallingPlayerGoingAlone = deal.CallingPlayerIsGoingAlone,
            ValidCardsToDiscard = [.. hand],
            ChosenCard = cardDecisionContext.ChosenCard,
            DecisionPredictedPoints = cardDecisionContext.DecisionPredictedPoints,
        };

        deal.DiscardCardDecisions.Add(record);
    }
}
