using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
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

        var hand = dealer.CurrentHand.SortByTrump(deal.Trump);
        var cardToDiscardContext = await GetDealerDiscardDecisionAsync(deal, dealerPosition, dealer, hand);

        var recordingContext = new DiscardCardRecordingContext(
            Deal: deal,
            PlayerPosition: dealerPosition,
            Hand: hand,
            CardDecisionContext: cardToDiscardContext);
        decisionRecorder.RecordDiscardDecision(recordingContext);

        validator.ValidateDiscard(cardToDiscardContext.ChosenCard, hand);

        deal.DiscardedCard = cardToDiscardContext.ChosenCard;

        dealer.CurrentHand.Remove(cardToDiscardContext.ChosenCard);
    }

    private static void AddUpcardToDealerHand(DealPlayer dealer, Card upCard)
    {
        dealer.CurrentHand.Add(upCard);
    }

    private Task<CardDecisionContext> GetDealerDiscardDecisionAsync(
        Deal deal,
        PlayerPosition dealerPosition,
        DealPlayer dealer,
        Card[] hand)
    {
        var dealerActor = playerActorResolver.GetPlayerActor(dealer);
        var (teamScore, opponentScore) = contextBuilder.GetScores(deal, dealerPosition);

        var context = new DiscardCardContext
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

        return dealerActor.DiscardCardAsync(context);
    }
}
