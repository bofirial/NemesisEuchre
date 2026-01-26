using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public interface ITrickWinnerCalculator
{
    PlayerPosition CalculateWinner(Trick trick, Suit trump);
}

public class TrickWinnerCalculator : ITrickWinnerCalculator
{
    /// <summary>
    /// Value added to trump cards to ensure they always beat non-trump cards.
    /// This offset places trump cards in a higher value range than any lead suit card.
    /// </summary>
    private const int TrumpValueOffset = 100;

    /// <summary>
    /// Value assigned to off-suit cards (cards that are neither trump nor match the lead suit).
    /// These cards cannot win the trick.
    /// </summary>
    private const int OffSuitValue = 0;

    public PlayerPosition CalculateWinner(Trick trick, Suit trump)
    {
        ValidateTrick(trick);

        var leadSuit = trick.LeadSuit!.Value;
        var winningCard = trick.CardsPlayed
            .MaxBy(playedCard => GetCardValue(playedCard.Card, leadSuit, trump));

        return winningCard!.PlayerPosition;
    }

    private static void ValidateTrick(Trick trick)
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
    }

    private static int GetCardValue(Card card, Suit leadSuit, Suit trump)
    {
        if (card.IsTrump(trump))
        {
            return TrumpValueOffset + card.GetTrumpValue(trump);
        }

        if (card.GetEffectiveSuit(trump) == leadSuit)
        {
            return (int)card.Rank;
        }

        return OffSuitValue;
    }
}
