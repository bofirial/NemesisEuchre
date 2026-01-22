using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public class DealFactory(ICardShuffler cardShuffler) : IDealFactory
{
    private readonly ICardShuffler _cardShuffler = cardShuffler;

    public Task<Deal> CreateDealAsync(Game game, Deal? previousDeal = null)
    {
        ArgumentNullException.ThrowIfNull(game);

        if (game.Players.Count != 4)
        {
            throw new ArgumentException("Game must have exactly 4 players.", nameof(game));
        }

        if (previousDeal?.DealerPosition == null && previousDeal != null)
        {
            throw new InvalidOperationException("Previous deal must have a dealer position.");
        }

        var dealerPosition = previousDeal?.DealerPosition!.Value.GetNextPosition() ?? PlayerPosition.North;

        var deck = CreateEuchreDeck();
        _cardShuffler.Shuffle(deck);

        foreach (var player in game.Players.Values)
        {
            player.Hand.Clear();
        }

        var currentPosition = dealerPosition.GetNextPosition();
        for (int i = 0; i < 4; i++)
        {
            var player = game.Players[currentPosition];
            for (int cardIndex = 0; cardIndex < 5; cardIndex++)
            {
                player.Hand.Add(deck[(i * 5) + cardIndex]);
            }

            currentPosition = currentPosition.GetNextPosition();
        }

        var deal = new Deal
        {
            DealStatus = DealStatus.NotStarted,
            DealerPosition = dealerPosition,
            Deck = deck[21..24],
            UpCard = deck[20],
        };

        return Task.FromResult(deal);
    }

    private static Card[] CreateEuchreDeck()
    {
        var deck = new Card[24];
        var index = 0;

        foreach (var suit in Enum.GetValues<Suit>())
        {
            deck[index++] = new Card { Suit = suit, Rank = Rank.Nine };
            deck[index++] = new Card { Suit = suit, Rank = Rank.Ten };
            deck[index++] = new Card { Suit = suit, Rank = Rank.Jack };
            deck[index++] = new Card { Suit = suit, Rank = Rank.Queen };
            deck[index++] = new Card { Suit = suit, Rank = Rank.King };
            deck[index++] = new Card { Suit = suit, Rank = Rank.Ace };
        }

        return deck;
    }
}
