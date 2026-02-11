using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Utilities;

namespace NemesisEuchre.GameEngine;

public interface IDealFactory
{
    Task<Deal> CreateDealAsync(Game game, Deal? previousDeal = null);
}

public class DealFactory(ICardShuffler cardShuffler, IRandomNumberGenerator randomNumberGenerator) : IDealFactory
{
    private const int CardsPerPlayer = 5;

    private const int PlayersPerGame = 4;

    private const int UpCardIndex = 20;

    private const int RemainingDeckStart = 21;

    public Task<Deal> CreateDealAsync(Game game, Deal? previousDeal = null)
    {
        ValidateInputs(game, previousDeal);

        var dealerPosition = CalculateDealerPosition(previousDeal);

        var deck = PrepareShuffledDeck();

        var players = DistributeCardsToPlayers(deck, dealerPosition, game.Players);

        var deal = BuildDeal(dealerPosition, deck, players);

        deal.Team1Score = game.Team1Score;
        deal.Team2Score = game.Team2Score;

        return Task.FromResult(deal);
    }

    private static void ValidateInputs(Game game, Deal? previousDeal)
    {
        ArgumentNullException.ThrowIfNull(game);

        if (game.Players.Count != PlayersPerGame)
        {
            throw new ArgumentException($"Game must have exactly {PlayersPerGame} players.", nameof(game));
        }

        if (previousDeal?.DealerPosition == null && previousDeal != null)
        {
            throw new InvalidOperationException("Previous deal must have a dealer position.");
        }
    }

    private static Dictionary<PlayerPosition, DealPlayer> DistributeCardsToPlayers(
        Card[] deck,
        PlayerPosition dealerPosition,
        Dictionary<PlayerPosition, Player> gamePlayers)
    {
        var dealPlayers = new Dictionary<PlayerPosition, DealPlayer>();
        var currentPosition = dealerPosition.GetNextPosition();

        for (int playerIndex = 0; playerIndex < PlayersPerGame; playerIndex++)
        {
            var sortedCards = DealCardsToPlayer(deck, playerIndex).SortByTrump(null);

            dealPlayers[currentPosition] = new DealPlayer
            {
                Position = currentPosition,
                StartingHand = sortedCards,
                CurrentHand = [.. sortedCards],
                Actor = gamePlayers[currentPosition].Actor,
            };

            currentPosition = currentPosition.GetNextPosition();
        }

        return dealPlayers;
    }

    private static Card[] DealCardsToPlayer(Card[] deck, int playerIndex)
    {
        var cardsForPlayer = new Card[CardsPerPlayer];
        for (int cardIndex = 0; cardIndex < CardsPerPlayer; cardIndex++)
        {
            cardsForPlayer[cardIndex] = deck[(playerIndex * CardsPerPlayer) + cardIndex];
        }

        return cardsForPlayer;
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
            Deck = [.. deck[RemainingDeckStart..]],
            UpCard = deck[UpCardIndex],
            Players = players,
        };
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

    private PlayerPosition CalculateDealerPosition(Deal? previousDeal)
    {
        if (previousDeal?.DealerPosition != null)
        {
            return previousDeal.DealerPosition.Value.GetNextPosition();
        }

        int randomPosition = randomNumberGenerator.NextInt(4);
        return (PlayerPosition)randomPosition;
    }

    private Card[] PrepareShuffledDeck()
    {
        var deck = CreateEuchreDeck();

        cardShuffler.Shuffle(deck);

        return deck;
    }
}
