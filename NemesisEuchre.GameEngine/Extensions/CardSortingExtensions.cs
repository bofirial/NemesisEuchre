using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Extensions;

public static class CardSortingExtensions
{
    public static Card[] SortByTrump(this IEnumerable<Card> cards, Suit? trump)
    {
        if (trump == null)
        {
            return [.. cards
                .OrderBy(c => (int)c.Suit)
                .ThenByDescending(c => (int)c.Rank)];
        }

        return [.. cards
            .OrderByDescending(c => c.IsTrump(trump.Value))
            .ThenByDescending(c => c.IsTrump(trump.Value)
                ? c.GetTrumpValue(trump.Value)
                : 0)
            .ThenBy(c => c.IsTrump(trump.Value) ? 0 : (int)c.Suit)
            .ThenByDescending(c => c.IsTrump(trump.Value) ? 0 : (int)c.Rank)];
    }
}
