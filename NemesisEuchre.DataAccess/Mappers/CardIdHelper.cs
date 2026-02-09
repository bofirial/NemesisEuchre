using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.DataAccess.Mappers;

public static class CardIdHelper
{
    public static int ToCardId(Card card)
    {
        return ((int)card.Suit * 100) + (int)card.Rank;
    }

    public static int ToRelativeCardId(RelativeCard card)
    {
        return ((int)card.Suit * 100) + (int)card.Rank;
    }

    public static Card ToCard(int cardId)
    {
        var suit = (Suit)(cardId / 100);
        var rank = (Rank)(cardId % 100);
        return new Card(suit, rank);
    }

    public static RelativeCard ToRelativeCard(int relativeCardId)
    {
        var suit = (RelativeSuit)(relativeCardId / 100);
        var rank = (Rank)(relativeCardId % 100);
        return new RelativeCard(rank, suit);
    }
}
