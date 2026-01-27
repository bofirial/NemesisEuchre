using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Services;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.GameEngine.Handlers;

public interface IDealerDiscardHandler
{
    Task HandleDealerDiscardAsync(Deal deal);
}

public class DealerDiscardHandler(
    IPlayerActorResolver playerActorResolver,
    IPlayerContextBuilder contextBuilder,
    ITrumpSelectionValidator validator,
    IDecisionRecorder decisionRecorder) : IDealerDiscardHandler
{
    public async Task HandleDealerDiscardAsync(Deal deal)
    {
        var dealerPosition = deal.DealerPosition!.Value;
        var dealer = deal.Players[dealerPosition];

        AddUpcardToDealerHand(dealer, deal.UpCard!);

        var hand = dealer.CurrentHand.ToArray();
        var cardToDiscard = await GetDealerDiscardDecisionAsync(deal, dealerPosition, dealer, hand);

        decisionRecorder.RecordDiscardDecision(deal, dealerPosition, hand, cardToDiscard);

        validator.ValidateDiscard(cardToDiscard, hand);

        dealer.CurrentHand.Remove(cardToDiscard);
    }

    private static void AddUpcardToDealerHand(DealPlayer dealer, Card upCard)
    {
        dealer.CurrentHand.Add(upCard);
    }

    private Task<Card> GetDealerDiscardDecisionAsync(
        Deal deal,
        PlayerPosition dealerPosition,
        DealPlayer dealer,
        Card[] hand)
    {
        var dealerActor = playerActorResolver.GetPlayerActor(dealer);
        var (teamScore, opponentScore) = contextBuilder.GetScores(deal, dealerPosition);

        return dealerActor.DiscardCardAsync(
            [.. hand],
            dealerPosition,
            teamScore,
            opponentScore,
            deal.Trump!.Value,
            deal.CallingPlayer!.Value,
            deal.CallingPlayerIsGoingAlone,
            [.. hand]);
    }
}
