using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.DataAccess.Tests.Integration;

public class GamePersistenceIntegrationTests
{
    [Fact]
    public async Task EndToEnd_SaveAndRetrieveCompleteGame()
    {
        var options = new DbContextOptionsBuilder<NemesisEuchreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var game = CreateCompletedGame();

        int gameId;
        await using (var context = new NemesisEuchreDbContext(options))
        {
            var trickMapper = new TrickToEntityMapper();
            var dealMapper = new DealToEntityMapper(trickMapper);
            var gameMapper = new GameToEntityMapper(dealMapper);
            var repository = new GameRepository(context, gameMapper);

            gameId = await repository.SaveCompletedGameAsync(game);
        }

        await using (var context = new NemesisEuchreDbContext(options))
        {
            var savedGame = await context.Games!
                .Include(g => g.Deals)
                    .ThenInclude(d => d.Tricks)
                .Include(g => g.Deals)
                    .ThenInclude(d => d.CallTrumpDecisions)
                .Include(g => g.Deals)
                    .ThenInclude(d => d.DiscardCardDecisions)
                .Include(g => g.Deals)
                    .ThenInclude(d => d.PlayCardDecisions)
                .FirstOrDefaultAsync(g => g.GameId == gameId);

            savedGame.Should().NotBeNull();
            savedGame!.GameStatus.Should().Be(GameStatus.Complete);
            savedGame.Team1Score.Should().Be(10);
            savedGame.WinningTeam.Should().Be(Team.Team1);
            savedGame.Deals.Should().HaveCount(1);

            var deal = savedGame.Deals.First();
            deal.DealNumber.Should().Be(1);
            deal.Trump.Should().Be(Suit.Hearts);
            deal.Tricks.Should().HaveCount(5);
            deal.CallTrumpDecisions.Should().HaveCountGreaterThan(0);
            deal.PlayCardDecisions.Should().HaveCount(20);

            var playDecision = deal.PlayCardDecisions.First();
            playDecision.ActorType.Should().Be(ActorType.Chaos);
            playDecision.DidTeamWinGame.Should().NotBeNull();
        }
    }

    private static Game CreateCompletedGame()
    {
        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 0,
            WinningTeam = Team.Team1,
        };

        game.Players[PlayerPosition.North] = new Player { Position = PlayerPosition.North, ActorType = ActorType.Chaos };
        game.Players[PlayerPosition.South] = new Player { Position = PlayerPosition.South, ActorType = ActorType.Chaos };
        game.Players[PlayerPosition.East] = new Player { Position = PlayerPosition.East, ActorType = ActorType.Chaos };
        game.Players[PlayerPosition.West] = new Player { Position = PlayerPosition.West, ActorType = ActorType.Chaos };

        var deal = new Deal
        {
            DealStatus = DealStatus.Complete,
            DealerPosition = PlayerPosition.North,
            Trump = Suit.Hearts,
            CallingPlayer = PlayerPosition.South,
            CallingPlayerIsGoingAlone = false,
            DealResult = DealResult.WonStandardBid,
            WinningTeam = Team.Team1,
            Team1Score = 2,
            Team2Score = 0,
        };

        deal.Deck.AddRange(CreateFullDeck());
        deal.UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Jack };

        foreach (var position in Enum.GetValues<PlayerPosition>())
        {
            deal.Players[position] = new DealPlayer
            {
                Position = position,
                ActorType = ActorType.Chaos,
            };
        }

        deal.CallTrumpDecisions.Add(new CallTrumpDecisionRecord
        {
            Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Nine }],
            UpCard = deal.UpCard,
            DealerPosition = PlayerPosition.North,
            DecidingPlayerPosition = PlayerPosition.South,
            TeamScore = 0,
            OpponentScore = 0,
            ValidDecisions = [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp],
            ChosenDecision = CallTrumpDecision.OrderItUp,
            DecisionOrder = 1,
        });

        for (int i = 0; i < 5; i++)
        {
            var trick = new Trick
            {
                LeadPosition = PlayerPosition.North,
                LeadSuit = Suit.Hearts,
                WinningPosition = PlayerPosition.South,
                WinningTeam = Team.Team1,
            };

            foreach (var position in Enum.GetValues<PlayerPosition>())
            {
                trick.CardsPlayed.Add(new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
                    PlayerPosition = position,
                });

                deal.PlayCardDecisions.Add(new PlayCardDecisionRecord
                {
                    Hand = [new Card { Suit = Suit.Hearts, Rank = Rank.Nine }],
                    DecidingPlayerPosition = position,
                    CurrentTrick = trick,
                    TeamScore = 0,
                    OpponentScore = 0,
                    ValidCardsToPlay = [new Card { Suit = Suit.Hearts, Rank = Rank.Nine }],
                    ChosenCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
                    LeadPosition = PlayerPosition.North,
                });
            }

            deal.CompletedTricks.Add(trick);
        }

        game.CompletedDeals.Add(deal);

        return game;
    }

    private static List<Card> CreateFullDeck()
    {
        var deck = new List<Card>();
        foreach (var suit in Enum.GetValues<Suit>())
        {
            foreach (var rank in new[] { Rank.Nine, Rank.Ten, Rank.Jack, Rank.Queen, Rank.King, Rank.Ace })
            {
                deck.Add(new Card { Suit = suit, Rank = rank });
            }
        }

        return deck;
    }
}
