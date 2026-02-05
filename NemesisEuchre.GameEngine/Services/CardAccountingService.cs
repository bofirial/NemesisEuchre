using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Services;

public interface ICardAccountingService
{
    List<Card> GetAccountedForCards(
        Deal deal,
        Trick currentTrick,
        PlayerPosition currentPlayerPosition,
        Card[] currentPlayerHand);
}

public class CardAccountingService : ICardAccountingService
{
    public List<Card> GetAccountedForCards(
        Deal deal,
        Trick currentTrick,
        PlayerPosition currentPlayerPosition,
        Card[] currentPlayerHand)
    {
        var accountedForCards = new List<Card>();

        foreach (var completedTrick in deal.CompletedTricks)
        {
            accountedForCards.AddRange(completedTrick.CardsPlayed.Select(pc => pc.Card));
        }

        accountedForCards.AddRange(currentTrick.CardsPlayed.Select(pc => pc.Card));

        accountedForCards.AddRange(currentPlayerHand);

        var isRound1 = deal.ChosenDecision is CallTrumpDecision.OrderItUp or
                       CallTrumpDecision.OrderItUpAndGoAlone;

        if (!isRound1 && deal.UpCard != null)
        {
            accountedForCards.Add(deal.UpCard);
        }

        if (currentPlayerPosition == deal.DealerPosition && deal.DiscardedCard != null)
        {
            accountedForCards.Add(deal.DiscardedCard);
        }

        return accountedForCards;
    }
}
