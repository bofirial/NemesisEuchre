using System.Text.Json;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation.Constants;
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

        await using (var context = new NemesisEuchreDbContext(options))
        {
            var mockLogger = new Mock<ILogger<GameRepository>>();
            var trickMapper = new TrickToEntityMapper();
            var dealMapper = new DealToEntityMapper(trickMapper);
            var gameMapper = new GameToEntityMapper(dealMapper);
            var mockOptions = new Mock<IOptions<PersistenceOptions>>();
            mockOptions.Setup(x => x.Value).Returns(new PersistenceOptions());
            var repository = new GameRepository(context, mockLogger.Object, gameMapper, mockOptions.Object);

            await repository.SaveCompletedGameAsync(game, TestContext.Current.CancellationToken);
        }

        await using (var context = new NemesisEuchreDbContext(options))
        {
            var savedGame = await context.Games!
                .Include(g => g.Deals)
                    .ThenInclude(d => d.Tricks)
                        .ThenInclude(t => t.PlayCardDecisions)
                .Include(g => g.Deals)
                    .ThenInclude(d => d.CallTrumpDecisions)
                .Include(g => g.Deals)
                    .ThenInclude(d => d.DiscardCardDecisions)
                .Include(g => g.Deals)
                    .ThenInclude(d => d.PlayCardDecisions)
                .FirstOrDefaultAsync(TestContext.Current.CancellationToken);

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

            deal.Tricks.Should().AllSatisfy(trick =>
            {
                trick.TrickNumber.Should().BeInRange(1, 5);
                trick.PlayCardDecisions.Should().HaveCount(4);
            });

            var firstTrick = deal.Tricks.First();
            firstTrick.TrickNumber.Should().Be(1);
            var playDecision = firstTrick.PlayCardDecisions[0];
            playDecision.ActorType.Should().Be(ActorType.Chaos);
            playDecision.DidTeamWinGame.Should().NotBeNull();
            playDecision.LeadPlayer.Should().BeDefined();
            playDecision.DealId.Should().Be(deal.DealId);
            playDecision.TrickId.Should().Be(firstTrick.TrickId);

            var cardsInHand = JsonSerializer.Deserialize<List<RelativeCard>>(playDecision.CardsInHandJson, JsonSerializationOptions.Default);
            cardsInHand.Should().NotBeNull();
            cardsInHand.Should().AllBeOfType<RelativeCard>();

            var playedCards = JsonSerializer.Deserialize<Dictionary<RelativePlayerPosition, RelativeCard>>(playDecision.PlayedCardsJson, JsonSerializationOptions.Default);
            playedCards.Should().NotBeNull();
            playedCards!.Keys.Should().AllBeOfType<RelativePlayerPosition>();
            playedCards.Values.Should().AllBeOfType<RelativeCard>();
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
            DealNumber = 1,
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
        deal.UpCard = new Card(Suit.Hearts, Rank.Jack);

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
            CardsInHand = [new Card(Suit.Hearts, Rank.Nine)],
            UpCard = deal.UpCard,
            DealerPosition = PlayerPosition.North,
            PlayerPosition = PlayerPosition.South,
            TeamScore = 0,
            OpponentScore = 0,
            ValidCallTrumpDecisions = [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp],
            ChosenDecision = CallTrumpDecision.OrderItUp,
            DecisionOrder = 1,
        });

        for (int i = 0; i < 5; i++)
        {
            var trick = new Trick
            {
                TrickNumber = (short)(i + 1),
                LeadPosition = PlayerPosition.North,
                LeadSuit = Suit.Hearts,
                WinningPosition = PlayerPosition.South,
                WinningTeam = Team.Team1,
            };

            foreach (var position in Enum.GetValues<PlayerPosition>())
            {
                trick.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.Nine), position));

                trick.PlayCardDecisions.Add(new PlayCardDecisionRecord
                {
                    CardsInHand = [new Card(Suit.Hearts, Rank.Nine)],
                    PlayerPosition = position,
                    TeamScore = 0,
                    OpponentScore = 0,
                    TrumpSuit = Suit.Hearts,
                    LeadPlayer = PlayerPosition.North,
                    LeadSuit = Suit.Hearts,
                    PlayedCards = [],
                    WinningTrickPlayer = null,
                    ValidCardsToPlay = [new Card(Suit.Hearts, Rank.Nine)],
                    ChosenCard = new Card(Suit.Hearts, Rank.Nine),
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
                deck.Add(new Card(suit, rank));
            }
        }

        return deck;
    }
}
