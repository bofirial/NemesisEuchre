using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Services;

public interface IDecisionRecorder
{
    void RecordCallTrumpDecision(CallTrumpRecordingContext context, ref byte decisionOrderCounter);

    void RecordPlayCardDecision(PlayCardRecordingContext context);

    void RecordDiscardDecision(DiscardCardRecordingContext context);
}

public class DecisionRecorder(IPlayerContextBuilder contextBuilder, ICardAccountingService cardAccountingService) : IDecisionRecorder
{
    public void RecordCallTrumpDecision(CallTrumpRecordingContext context, ref byte decisionOrderCounter)
    {
        var player = context.Deal.Players[context.PlayerPosition];
        var (teamScore, opponentScore) = contextBuilder.GetScores(context.Deal, context.PlayerPosition);

        var record = new CallTrumpDecisionRecord
        {
            CardsInHand = [.. player.CurrentHand],
            UpCard = context.Deal.UpCard!,
            DealerPosition = context.Deal.DealerPosition!.Value,
            PlayerPosition = context.PlayerPosition,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            ValidCallTrumpDecisions = [.. context.ValidDecisions],
            ChosenDecision = context.CallTrumpDecisionContext.ChosenCallTrumpDecision,
            DecisionPredictedPoints = context.CallTrumpDecisionContext.DecisionPredictedPoints,
            DecisionOrder = ++decisionOrderCounter,
        };

        context.Deal.CallTrumpDecisions.Add(record);
    }

    public void RecordPlayCardDecision(PlayCardRecordingContext context)
    {
        var (teamScore, opponentScore) = contextBuilder.GetScores(context.Deal, context.PlayerPosition);

        var playerTeam = context.PlayerPosition.GetTeam();
        var wonTricks = (short)context.Deal.CompletedTricks.Count(t => t.WinningTeam == playerTeam);
        var opponentsWonTricks = (short)context.Deal.CompletedTricks.Count(t => t.WinningTeam != null && t.WinningTeam != playerTeam);

        var accountedForCards = cardAccountingService.GetAccountedForCards(
            context.Deal,
            context.Trick,
            context.PlayerPosition,
            context.Hand);

        var record = new PlayCardDecisionRecord
        {
            CardsInHand = [.. context.Hand],
            PlayerPosition = context.PlayerPosition,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            WonTricks = wonTricks,
            OpponentsWonTricks = opponentsWonTricks,
            TrumpSuit = context.Deal.Trump!.Value,
            LeadPlayer = context.Trick.LeadPosition,
            LeadSuit = context.Trick.LeadSuit,
            PlayedCards = context.Trick.CardsPlayed.ToDictionary(
                pc => pc.PlayerPosition,
                pc => pc.Card),
            WinningTrickPlayer = context.Trick.CardsPlayed.Count > 0 && context.Trick.LeadSuit.HasValue
                ? context.TrickWinnerCalculator.CalculateWinner(context.Trick, context.Deal.Trump!.Value)
                : null,
            TrickNumber = context.Trick.TrickNumber,
            ValidCardsToPlay = [.. context.ValidCards],
            CallingPlayer = context.Deal.CallingPlayer!.Value,
            CallingPlayerGoingAlone = context.Deal.CallingPlayerIsGoingAlone,
            KnownPlayerSuitVoids = [.. context.Deal.KnownPlayerSuitVoids],
            Dealer = context.Deal.DealerPosition!.Value,
            DealerPickedUpCard = context.Deal.UpCard,
            CardsAccountedFor = [.. accountedForCards],
            ChosenCard = context.CardDecisionContext.ChosenCard,
            DecisionPredictedPoints = context.CardDecisionContext.DecisionPredictedPoints,
        };

        context.Trick.PlayCardDecisions.Add(record);
    }

    public void RecordDiscardDecision(DiscardCardRecordingContext context)
    {
        var (teamScore, opponentScore) = contextBuilder.GetScores(context.Deal, context.PlayerPosition);

        var record = new DiscardCardDecisionRecord
        {
            CardsInHand = [.. context.Hand],
            PlayerPosition = context.PlayerPosition,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            TrumpSuit = context.Deal.Trump!.Value,
            CallingPlayer = context.Deal.CallingPlayer!.Value,
            CallingPlayerGoingAlone = context.Deal.CallingPlayerIsGoingAlone,
            ValidCardsToDiscard = [.. context.Hand],
            ChosenCard = context.CardDecisionContext.ChosenCard,
            DecisionPredictedPoints = context.CardDecisionContext.DecisionPredictedPoints,
        };

        context.Deal.DiscardCardDecisions.Add(record);
    }
}
