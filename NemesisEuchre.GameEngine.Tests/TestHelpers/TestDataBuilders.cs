using Bogus;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Tests.TestHelpers;

public static class TestDataBuilders
{
    private static readonly Faker Faker = new();

    public static Game CreateGame(short team1Score = 0, short team2Score = 0)
    {
        var game = new Game
        {
            Team1Score = team1Score,
            Team2Score = team2Score,
        };
        game.Players.Add(PlayerPosition.North, new Player { Position = PlayerPosition.North });
        game.Players.Add(PlayerPosition.East, new Player { Position = PlayerPosition.East });
        game.Players.Add(PlayerPosition.South, new Player { Position = PlayerPosition.South });
        game.Players.Add(PlayerPosition.West, new Player { Position = PlayerPosition.West });
        return game;
    }

    public static Deal CreateDeal(
        PlayerPosition callingPlayer = PlayerPosition.North,
        DealResult dealResult = DealResult.WonStandardBid,
        Team winningTeam = Team.Team1,
        bool isGoingAlone = false)
    {
        return new Deal
        {
            CallingPlayer = callingPlayer,
            DealResult = dealResult,
            WinningTeam = winningTeam,
            CallingPlayerIsGoingAlone = isGoingAlone,
        };
    }

    public static Trick CreateTrick(
        short trickNumber = 1,
        PlayerPosition leadPosition = PlayerPosition.North,
        PlayerPosition winningPosition = PlayerPosition.North)
    {
        return new Trick
        {
            TrickNumber = trickNumber,
            LeadPosition = leadPosition,
            WinningPosition = winningPosition,
        };
    }

    public static Dictionary<PlayerPosition, DealPlayer> CreateDealPlayers()
    {
        return new Dictionary<PlayerPosition, DealPlayer>
        {
            { PlayerPosition.North, new DealPlayer { Position = PlayerPosition.North, StartingHand = [] } },
            { PlayerPosition.East, new DealPlayer { Position = PlayerPosition.East, StartingHand = [] } },
            { PlayerPosition.South, new DealPlayer { Position = PlayerPosition.South, StartingHand = [] } },
            { PlayerPosition.West, new DealPlayer { Position = PlayerPosition.West, StartingHand = [] } },
        };
    }

    public static Card[] GenerateCards(int count)
    {
        var cards = new Card[count];
        for (int i = 0; i < count; i++)
        {
            cards[i] = GenerateCard();
        }

        return cards;
    }

    public static Card GenerateCard()
    {
        return new Card(Faker.PickRandom<Suit>(), Faker.PickRandom<Rank>());
    }

    public static RelativeCard[] GenerateRelativeCards(int count)
    {
        var cards = new RelativeCard[count];
        for (int i = 0; i < count; i++)
        {
            cards[i] = new RelativeCard(Faker.PickRandom<Rank>(), Faker.PickRandom<RelativeSuit>())
            {
                Card = GenerateCard(),
            };
        }

        return cards;
    }

    public static Deal CreateMinimalDeal(Suit trump)
    {
        var nonTrumpSuit = trump == Suit.Clubs ? Suit.Diamonds : Suit.Clubs;

        var trick1 = CreateTrick(1);
        trick1.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.King), PlayerPosition.North));
        trick1.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Queen), PlayerPosition.East));
        trick1.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Ten), PlayerPosition.South));
        trick1.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Nine), PlayerPosition.West));

        var trick2 = CreateTrick(2);
        trick2.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.King), PlayerPosition.North));
        trick2.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.Queen), PlayerPosition.East));
        trick2.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.Ten), PlayerPosition.South));
        trick2.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.Nine), PlayerPosition.West));

        var trick3 = CreateTrick(3);
        trick3.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Ace), PlayerPosition.North));
        trick3.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.King), PlayerPosition.East));
        trick3.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Queen), PlayerPosition.South));
        trick3.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Ten), PlayerPosition.West));

        var trick4 = CreateTrick(4);
        trick4.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.Ace), PlayerPosition.North));
        trick4.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.King), PlayerPosition.East));
        trick4.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.Queen), PlayerPosition.South));
        trick4.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.Ten), PlayerPosition.West));

        var trick5 = CreateTrick(5);
        trick5.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Ace), PlayerPosition.North));
        trick5.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.Ace), PlayerPosition.East));
        trick5.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.King), PlayerPosition.South));
        trick5.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Queen), PlayerPosition.West));

        return new Deal
        {
            Trump = trump,
            DealerPosition = PlayerPosition.North,
            CallingPlayer = PlayerPosition.East,
            ChosenDecision = CallTrumpDecision.CallHearts,
            UpCard = new Card(trump, Rank.Nine),
            WinningTeam = Team.Team1,
            Players = CreateDealPlayers(),
            CompletedTricks = [trick1, trick2, trick3, trick4, trick5],
        };
    }
}
