using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public class TrickWinnerCalculator : ITrickWinnerCalculator
{
    public PlayerPosition CalculateWinner(Trick trick, Suit trump)
    {
        ArgumentNullException.ThrowIfNull(trick);

        if (trick.CardsPlayed.Count == 0)
        {
            throw new InvalidOperationException("Cannot calculate winner of a trick with no cards played");
        }

        if (!trick.LeadSuit.HasValue)
        {
            throw new InvalidOperationException("Cannot calculate winner of a trick with no lead suit");
        }

        var winningCard = trick.CardsPlayed[0];
        var winningValue = GetCardValue(winningCard.Card, trick.LeadSuit.Value, trump);

        for (int i = 1; i < trick.CardsPlayed.Count; i++)
        {
            var currentCard = trick.CardsPlayed[i];
            var currentValue = GetCardValue(currentCard.Card, trick.LeadSuit.Value, trump);

            if (currentValue > winningValue)
            {
                winningCard = currentCard;
                winningValue = currentValue;
            }
        }

        return winningCard.PlayerPosition;
    }

    private static int GetCardValue(Card card, Suit leadSuit, Suit trump)
    {
        if (card.IsTrump(trump))
        {
            return 100 + card.GetTrumpValue(trump);
        }
        else if (card.GetEffectiveSuit(trump) == leadSuit)
        {
            return (int)card.Rank;
        }
        else
        {
            return 0;
        }
    }
}
