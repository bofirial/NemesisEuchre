using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
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
    ITrumpSelectionValidator validator) : IDealerDiscardHandler
{
    public async Task HandleDealerDiscardAsync(Deal deal)
    {
        var dealerPosition = deal.DealerPosition!.Value;
        var dealer = deal.Players[dealerPosition];

        AddUpcardToDealerHand(dealer, deal.UpCard!);

        var relativeHand = ConvertToRelativeCards(dealer.CurrentHand, deal.Trump!.Value);
        var cardToDiscard = await GetDealerDiscardDecisionAsync(deal, dealerPosition, dealer, relativeHand);

        validator.ValidateDiscard(cardToDiscard, relativeHand);

        dealer.CurrentHand.Remove(cardToDiscard.Card);
    }

    private static void AddUpcardToDealerHand(DealPlayer dealer, Card upCard)
    {
        dealer.CurrentHand.Add(upCard);
    }

    private static RelativeCard[] ConvertToRelativeCards(List<Card> cards, Suit trump)
    {
        return [.. cards.Select(c => c.ToRelative(trump))];
    }

    private Task<RelativeCard> GetDealerDiscardDecisionAsync(
        Deal deal,
        PlayerPosition dealerPosition,
        DealPlayer dealer,
        RelativeCard[] relativeHand)
    {
        var dealerActor = playerActorResolver.GetPlayerActor(dealer);
        var (teamScore, opponentScore) = contextBuilder.GetScores(deal, dealerPosition);

        return dealerActor.DiscardCardAsync(
            [.. relativeHand],
            deal.ToRelative(dealerPosition),
            teamScore,
            opponentScore,
            [.. relativeHand]);
    }
}
