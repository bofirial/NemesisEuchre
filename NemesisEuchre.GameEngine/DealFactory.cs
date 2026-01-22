using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public class DealFactory(ICardShuffler cardShuffler) : IDealFactory
{
    public Task<Deal> CreateDealAsync(Game game, Deal? previousDeal = null)
    {
        ValidateInputs(game, previousDeal);

        var dealerPosition = CalculateDealerPosition(previousDeal);

        var deck = PrepareShuffledDeck();

        var players = DistributeCardsToPlayers(deck, dealerPosition, game.Players);

        var deal = BuildDeal(dealerPosition, deck, players);

        return Task.FromResult(deal);
    }

    private static void ValidateInputs(Game game, Deal? previousDeal)
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
    }

    private static PlayerPosition CalculateDealerPosition(Deal? previousDeal)
    {
        return previousDeal?.DealerPosition!.Value.GetNextPosition() ?? PlayerPosition.North;
    }

    private static Dictionary<PlayerPosition, DealPlayer> DistributeCardsToPlayers(
        Card[] deck,
        PlayerPosition dealerPosition,
        Dictionary<PlayerPosition, Player> gamePlayers)
    {
        var dealPlayers = new Dictionary<PlayerPosition, DealPlayer>();
        var currentPosition = dealerPosition.GetNextPosition();

        for (int i = 0; i < 4; i++)
        {
            var cardsForPlayer = new Card[5];
            for (int cardIndex = 0; cardIndex < 5; cardIndex++)
            {
                cardsForPlayer[cardIndex] = deck[(i * 5) + cardIndex];
            }

            dealPlayers[currentPosition] = new DealPlayer
            {
                Position = currentPosition,
                StartingHand = cardsForPlayer,
                CurrentHand = [.. cardsForPlayer],
                BotType = gamePlayers[currentPosition].BotType,
            };

            currentPosition = currentPosition.GetNextPosition();
        }

        return dealPlayers;
    }

    private static Deal BuildDeal(
        PlayerPosition dealerPosition,
        Card[] deck,
        Dictionary<PlayerPosition, DealPlayer> players)
    {
        return new Deal
        {
            DealStatus = DealStatus.NotStarted,
            DealerPosition = dealerPosition,
            Deck = [.. deck[21..24]],
            UpCard = deck[20],
            Players = players,
        };
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

    private Card[] PrepareShuffledDeck()
    {
        var deck = CreateEuchreDeck();

        cardShuffler.Shuffle(deck);

        return deck;
    }
}
