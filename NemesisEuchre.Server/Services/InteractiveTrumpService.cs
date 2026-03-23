using Microsoft.Extensions.Options;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Mappers;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Options;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Services;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.Server.Services;

public interface IInteractiveTrumpService
{
    (PlayerPosition position, CallTrumpDecision[] validDecisions)? GetCurrentTrumpDecider(Deal deal);

    (PlayerPosition position, Card[] validCards)? GetCurrentDiscardDecider(Deal deal);

    Task ProcessBotTrumpDecisionsAsync(Deal deal, CancellationToken ct = default);

    Task ApplyHumanTrumpDecisionAsync(
        Deal deal,
        PlayerPosition position,
        CallTrumpDecision decision,
        CancellationToken ct = default);

    Task ApplyHumanDealerDiscardAsync(
        Deal deal,
        PlayerPosition position,
        Card card,
        CancellationToken ct = default);
}

public class InteractiveTrumpService(
    IOptions<GameOptions> gameOptions,
    IPlayerActorResolver actorResolver,
    ICallTrumpDecisionMapper decisionMapper,
    IPlayerContextBuilder contextBuilder,
    ITrumpSelectionValidator validator,
    IDecisionRecorder decisionRecorder) : IInteractiveTrumpService
{
    public (PlayerPosition position, CallTrumpDecision[] validDecisions)? GetCurrentTrumpDecider(Deal deal)
    {
        if (deal.DealStatus is not DealStatus.SelectingTrumpPhase1 and not DealStatus.SelectingTrumpPhase2)
        {
            return null;
        }

        var index = deal.CallTrumpDecisions.Count;
        var phase1Done = index >= 4;

        if (phase1Done && deal.DealStatus == DealStatus.SelectingTrumpPhase1)
        {
            return null;
        }

        var position = GetPositionForDecisionIndex(deal, index);

        CallTrumpDecision[] validDecisions;
        if (!phase1Done)
        {
            validDecisions = decisionMapper.GetValidRound1Decisions();
        }
        else
        {
            var isDealer = position == deal.DealerPosition!.Value;
            validDecisions = decisionMapper.GetValidRound2Decisions(
                deal.UpCard!.Suit,
                isDealer,
                gameOptions.Value.StickTheDealer);
        }

        return (position, validDecisions);
    }

    public (PlayerPosition position, Card[] validCards)? GetCurrentDiscardDecider(Deal deal)
    {
        if (deal.DealStatus != DealStatus.DealerDiscarding)
        {
            return null;
        }

        var position = deal.DealerPosition!.Value;
        var hand = deal.Players[position].CurrentHand;
        return (position, [.. hand]);
    }

    public async Task ProcessBotTrumpDecisionsAsync(Deal deal, CancellationToken ct = default)
    {
        while (true)
        {
            if (deal.DealStatus == DealStatus.DealerDiscarding)
            {
                var dealerPosition = deal.DealerPosition!.Value;
                var dealerDealPlayer = deal.Players[dealerPosition];
                if (dealerDealPlayer.Actor.ActorType != ActorType.User)
                {
                    await ExecuteBotDealerDiscardAsync(deal, dealerPosition, dealerDealPlayer);
                    deal.DealStatus = DealStatus.Playing;
                }

                return;
            }

            var decider = GetCurrentTrumpDecider(deal);
            if (decider is null)
            {
                return;
            }

            var dealPlayer = deal.Players[decider.Value.position];
            if (dealPlayer.Actor.ActorType == ActorType.User)
            {
                return;
            }

            var actor = actorResolver.GetPlayerActor(dealPlayer);
            var context = BuildCallTrumpContext(deal, decider.Value.position, decider.Value.validDecisions);
            var result = await actor.CallTrumpAsync(context);
            ApplyDecisionToDeal(deal, decider.Value.position, result, decider.Value.validDecisions);
        }
    }

    public Task ApplyHumanTrumpDecisionAsync(
        Deal deal,
        PlayerPosition position,
        CallTrumpDecision decision,
        CancellationToken ct = default)
    {
        var decider = GetCurrentTrumpDecider(deal);
        if (decider?.position != position)
        {
            throw new InvalidOperationException("Not your turn");
        }

        validator.ValidateDecision(decision, decider.Value.validDecisions);

        var result = new CallTrumpDecisionContext { ChosenCallTrumpDecision = decision };
        ApplyDecisionToDeal(deal, position, result, decider.Value.validDecisions);

        return ProcessBotTrumpDecisionsAsync(deal, ct);
    }

    public Task ApplyHumanDealerDiscardAsync(
        Deal deal,
        PlayerPosition position,
        Card card,
        CancellationToken ct = default)
    {
        if (deal.DealStatus != DealStatus.DealerDiscarding)
        {
            throw new InvalidOperationException("Not in dealer discard phase");
        }

        if (position != deal.DealerPosition)
        {
            throw new InvalidOperationException("Not your turn to discard");
        }

        var dealer = deal.Players[position];
        validator.ValidateDiscard(card, [.. dealer.CurrentHand]);

        var hand = dealer.CurrentHand.SortByTrump(deal.Trump);
        var cardDecision = new CardDecisionContext { ChosenCard = card };
        var recordingContext = new DiscardCardRecordingContext(deal, position, hand, cardDecision);
        decisionRecorder.RecordDiscardDecision(recordingContext);

        deal.DiscardedCard = card;
        dealer.CurrentHand.Remove(card);
        deal.DealStatus = DealStatus.Playing;

        return Task.CompletedTask;
    }

    private static PlayerPosition GetPositionForDecisionIndex(Deal deal, int index)
    {
        var position = deal.DealerPosition!.Value.GetNextPosition();
        var advances = index % 4;
        for (var i = 0; i < advances; i++)
        {
            position = position.GetNextPosition();
        }

        return position;
    }

    private CallTrumpContext BuildCallTrumpContext(
        Deal deal,
        PlayerPosition position,
        CallTrumpDecision[] validDecisions)
    {
        var player = deal.Players[position];
        var (teamScore, opponentScore) = contextBuilder.GetScores(deal, position);

        return new CallTrumpContext
        {
            CardsInHand = [.. player.CurrentHand],
            PlayerPosition = position,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            DealerPosition = deal.DealerPosition!.Value,
            UpCard = deal.UpCard!,
            ValidCallTrumpDecisions = [.. validDecisions],
            DecisionNumber = (byte)(deal.CallTrumpDecisions.Count + 1),
        };
    }

    private void ApplyDecisionToDeal(
        Deal deal,
        PlayerPosition position,
        CallTrumpDecisionContext result,
        CallTrumpDecision[] validDecisions)
    {
        var decision = result.ChosenCallTrumpDecision;

        var recordingContext = new CallTrumpRecordingContext(deal, position, validDecisions, result);
        var order = (byte)deal.CallTrumpDecisions.Count;
        decisionRecorder.RecordCallTrumpDecision(recordingContext, ref order);

        if (decision != CallTrumpDecision.Pass)
        {
            var isPhase1 = deal.DealStatus == DealStatus.SelectingTrumpPhase1;

            deal.Trump = isPhase1
                ? deal.UpCard!.Suit
                : decisionMapper.ConvertDecisionToSuit(decision);
            deal.CallingPlayer = position;
            deal.CallingPlayerIsGoingAlone = decisionMapper.IsGoingAloneDecision(decision);
            deal.ChosenDecision = decision;

            if (isPhase1)
            {
                deal.Players[deal.DealerPosition!.Value].CurrentHand.Add(deal.UpCard!);
                deal.DealStatus = DealStatus.DealerDiscarding;
            }
            else
            {
                deal.DealStatus = DealStatus.Playing;
            }
        }
        else if (deal.CallTrumpDecisions.Count == 4)
        {
            deal.DealStatus = DealStatus.SelectingTrumpPhase2;
        }
        else if (deal.CallTrumpDecisions.Count == 8)
        {
            deal.DealStatus = DealStatus.Complete;
        }
    }

    private async Task ExecuteBotDealerDiscardAsync(
        Deal deal,
        PlayerPosition dealerPosition,
        DealPlayer dealer)
    {
        var dealerActor = actorResolver.GetPlayerActor(dealer);
        var hand = dealer.CurrentHand.SortByTrump(deal.Trump);
        var (teamScore, opponentScore) = contextBuilder.GetScores(deal, dealerPosition);

        var discardContext = new DiscardCardContext
        {
            CardsInHand = [.. hand],
            PlayerPosition = dealerPosition,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            TrumpSuit = deal.Trump!.Value,
            CallingPlayer = deal.CallingPlayer!.Value,
            CallingPlayerGoingAlone = deal.CallingPlayerIsGoingAlone,
            ValidCardsToDiscard = [.. hand],
        };

        var cardDecision = await dealerActor.DiscardCardAsync(discardContext);
        validator.ValidateDiscard(cardDecision.ChosenCard, hand);

        var recordingContext = new DiscardCardRecordingContext(deal, dealerPosition, hand, cardDecision);
        decisionRecorder.RecordDiscardDecision(recordingContext);

        deal.DiscardedCard = cardDecision.ChosenCard;
        dealer.CurrentHand.Remove(cardDecision.ChosenCard);
    }
}
