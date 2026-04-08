using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Utilities;

namespace NemesisEuchre.MachineLearning.Bots.Simulation;

public interface IHiddenCardDistributor
{
    Dictionary<PlayerPosition, List<Card>> DistributeHiddenCards(
        Card[] accountedForCards,
        PlayerPosition simulatingPlayer,
        int cardsPerOtherPlayer,
        Suit trump,
        IReadOnlyList<PlayerSuitVoid> knownVoids,
        PlayerPosition? sittingOutPlayer,
        IRandomNumberGenerator random);
}

public class HiddenCardDistributor : IHiddenCardDistributor
{
    private static readonly Card[] FullDeck = CreateEuchreDeck();

    public Dictionary<PlayerPosition, List<Card>> DistributeHiddenCards(
        Card[] accountedForCards,
        PlayerPosition simulatingPlayer,
        int cardsPerOtherPlayer,
        Suit trump,
        IReadOnlyList<PlayerSuitVoid> knownVoids,
        PlayerPosition? sittingOutPlayer,
        IRandomNumberGenerator random)
    {
        var accountedSet = new HashSet<Card>(accountedForCards);
        var unaccountedCards = FullDeck.Where(c => !accountedSet.Contains(c)).ToList();

        var playersToFill = Enum.GetValues<PlayerPosition>()
            .Where(p => p != simulatingPlayer && p != sittingOutPlayer)
            .ToArray();

        var voidLookup = BuildVoidLookup(knownVoids);

        return DistributeWithConstraints(unaccountedCards, playersToFill, cardsPerOtherPlayer, voidLookup, trump, random);
    }

    private static Dictionary<PlayerPosition, HashSet<Suit>> BuildVoidLookup(
        IReadOnlyList<PlayerSuitVoid> knownVoids)
    {
        var lookup = new Dictionary<PlayerPosition, HashSet<Suit>>();

        foreach (var v in knownVoids)
        {
            if (!lookup.TryGetValue(v.PlayerPosition, out var suits))
            {
                suits = [];
                lookup[v.PlayerPosition] = suits;
            }

            suits.Add(v.Suit);
        }

        return lookup;
    }

    private static Dictionary<PlayerPosition, List<Card>> DistributeWithConstraints(
        List<Card> unaccountedCards,
        PlayerPosition[] players,
        int cardsPerPlayer,
        Dictionary<PlayerPosition, HashSet<Suit>> voidLookup,
        Suit trump,
        IRandomNumberGenerator random)
    {
        var hands = new Dictionary<PlayerPosition, List<Card>>();
        foreach (var player in players)
        {
            hands[player] = new List<Card>(cardsPerPlayer);
        }

        Shuffle(unaccountedCards, random);

        var remaining = new List<Card>(unaccountedCards);

        foreach (var player in players)
        {
            voidLookup.TryGetValue(player, out var voidSuits);

            var hand = hands[player];
            for (int i = remaining.Count - 1; i >= 0 && hand.Count < cardsPerPlayer; i--)
            {
                var card = remaining[i];
                var effectiveSuit = card.GetEffectiveSuit(trump);

                if (voidSuits?.Contains(effectiveSuit) == true)
                {
                    continue;
                }

                hand.Add(card);
                remaining.RemoveAt(i);
            }
        }

        // If any player is short due to void constraints, fill with remaining cards
        // (constraints may be imperfect in random distributions)
        foreach (var player in players)
        {
            var hand = hands[player];
            while (hand.Count < cardsPerPlayer && remaining.Count > 0)
            {
                hand.Add(remaining[^1]);
                remaining.RemoveAt(remaining.Count - 1);
            }
        }

        return hands;
    }

    private static void Shuffle(List<Card> list, IRandomNumberGenerator random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.NextInt(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static Card[] CreateEuchreDeck()
    {
        var deck = new Card[24];
        var index = 0;

        foreach (var suit in Enum.GetValues<Suit>())
        {
            deck[index++] = new Card(suit, Rank.Nine);
            deck[index++] = new Card(suit, Rank.Ten);
            deck[index++] = new Card(suit, Rank.Jack);
            deck[index++] = new Card(suit, Rank.Queen);
            deck[index++] = new Card(suit, Rank.King);
            deck[index++] = new Card(suit, Rank.Ace);
        }

        return deck;
    }
}
