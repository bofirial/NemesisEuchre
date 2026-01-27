using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Services;

public interface IDecisionRecorder
{
    void RecordCallTrumpDecision(
        Deal deal,
        PlayerPosition playerPosition,
        CallTrumpDecision[] validDecisions,
        CallTrumpDecision chosenDecision,
        ref byte decisionOrderCounter);

    void RecordPlayCardDecision(
        Deal deal,
        Trick trick,
        PlayerPosition playerPosition,
        Card[] hand,
        Card[] validCards,
        Card chosenCard,
        ITrickWinnerCalculator trickWinnerCalculator);

    void RecordDiscardDecision(
        Deal deal,
        PlayerPosition playerPosition,
        Card[] hand,
        Card chosenCard);
}

public class DecisionRecorder(IPlayerContextBuilder contextBuilder) : IDecisionRecorder
{
    public void RecordCallTrumpDecision(
        Deal deal,
        PlayerPosition playerPosition,
        CallTrumpDecision[] validDecisions,
        CallTrumpDecision chosenDecision,
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
            ChosenDecision = chosenDecision,
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
        Card chosenCard,
        ITrickWinnerCalculator trickWinnerCalculator)
    {
        var (teamScore, opponentScore) = contextBuilder.GetScores(deal, playerPosition);

        var record = new PlayCardDecisionRecord
        {
            CardsInHand = [.. hand],
            PlayerPosition = playerPosition,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            TrumpSuit = deal.Trump!.Value,
            LeadPlayer = trick.LeadPosition,
            LeadSuit = trick.LeadSuit,
            PlayedCards = trick.CardsPlayed.ToDictionary(
                pc => pc.PlayerPosition,
                pc => pc.Card),
            WinningTrickPlayer = trick.CardsPlayed.Count > 0 && trick.LeadSuit.HasValue
                ? trickWinnerCalculator.CalculateWinner(trick, deal.Trump!.Value)
                : null,
            ValidCardsToPlay = [.. validCards],
            ChosenCard = chosenCard,
        };

        trick.PlayCardDecisions.Add(record);
    }

    public void RecordDiscardDecision(
        Deal deal,
        PlayerPosition playerPosition,
        Card[] hand,
        Card chosenCard)
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
            ChosenCard = chosenCard,
        };

        deal.DiscardCardDecisions.Add(record);
    }
}
