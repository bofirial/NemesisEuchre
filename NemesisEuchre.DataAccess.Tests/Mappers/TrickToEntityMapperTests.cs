using System.Text.Json;

using FluentAssertions;

using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.DataAccess.Tests.Mappers;

public class TrickToEntityMapperTests
{
    [Fact]
    public void Map_ShouldMapTrickEntityProperties()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateSampleTrick();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 3, gamePlayers, didTeam1WinGame: false, didTeam2WinGame: false, dealWinningTeam: Team.Team1);

        entity.TrickNumber.Should().Be(3);
        entity.LeadPosition.Should().Be(PlayerPosition.North);
        entity.LeadSuit.Should().Be(Suit.Hearts);
        entity.WinningPosition.Should().Be(PlayerPosition.South);
        entity.WinningTeam.Should().Be(Team.Team1);
        entity.CardsPlayedJson.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Map_ShouldCreatePlayCardDecisionEntitiesCollection()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, didTeam1WinGame: false, didTeam2WinGame: false, dealWinningTeam: Team.Team1);

        entity.PlayCardDecisions.Should().NotBeNull();
        entity.PlayCardDecisions.Should().HaveCount(4);
    }

    [Fact]
    public void Map_ShouldMapPlayCardDecisionProperties()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, didTeam1WinGame: true, didTeam2WinGame: false, dealWinningTeam: Team.Team1);

        var decision = entity.PlayCardDecisions[0];
        decision.CardsInHandJson.Should().NotBeNullOrEmpty();
        decision.TeamScore.Should().Be(2);
        decision.OpponentScore.Should().Be(0);
        decision.ValidCardsToPlayJson.Should().NotBeNullOrEmpty();
        decision.ChosenCardJson.Should().NotBeNullOrEmpty();
        decision.ActorType.Should().Be(ActorType.Chaos);
    }

    [Fact]
    public void Map_ShouldConvertAbsolutePositionsToRelativePositions()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, didTeam1WinGame: false, didTeam2WinGame: false, dealWinningTeam: Team.Team1);

        var northDecision = entity.PlayCardDecisions[0];
        northDecision.LeadPlayer.Should().Be(RelativePlayerPosition.Self);

        entity.PlayCardDecisions.Should().HaveCountGreaterThan(1);
        entity.PlayCardDecisions.Should().Contain(d => d.LeadPlayer != RelativePlayerPosition.Self);
    }

    [Fact]
    public void Map_ShouldConvertAbsoluteSuitsToRelativeSuits()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, didTeam1WinGame: false, didTeam2WinGame: false, dealWinningTeam: Team.Team1);

        var decision = entity.PlayCardDecisions[0];
        decision.LeadSuit.Should().Be(RelativeSuit.Trump);
    }

    [Fact]
    public void Map_ShouldConvertCardsToRelativeCards()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, didTeam1WinGame: false, didTeam2WinGame: false, dealWinningTeam: Team.Team1);

        var decision = entity.PlayCardDecisions[0];
        var cardsInHand = JsonSerializer.Deserialize<List<RelativeCard>>(decision.CardsInHandJson, JsonSerializationOptions.Default);
        cardsInHand.Should().NotBeNull();
        cardsInHand.Should().AllBeOfType<RelativeCard>();

        var validCards = JsonSerializer.Deserialize<List<RelativeCard>>(decision.ValidCardsToPlayJson, JsonSerializationOptions.Default);
        validCards.Should().NotBeNull();
        validCards.Should().AllBeOfType<RelativeCard>();

        var chosenCard = JsonSerializer.Deserialize<RelativeCard>(decision.ChosenCardJson, JsonSerializationOptions.Default);
        chosenCard.Should().NotBeNull();
        chosenCard.Suit.Should().BeDefined();
    }

    [Fact]
    public void Map_ShouldConvertPlayedCardsDictionary()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, didTeam1WinGame: false, didTeam2WinGame: false, dealWinningTeam: Team.Team1);

        var decision = entity.PlayCardDecisions[0];
        var playedCards = JsonSerializer.Deserialize<Dictionary<RelativePlayerPosition, RelativeCard>>(decision.PlayedCardsJson, JsonSerializationOptions.Default);

        playedCards.Should().NotBeNull();
        playedCards.Should().NotBeEmpty();
        playedCards!.Keys.Should().AllBeOfType<RelativePlayerPosition>();
        playedCards.Values.Should().AllBeOfType<RelativeCard>();
    }

    [Fact]
    public void Map_ShouldSetOutcomeFlagsCorrectly_WhenTeam1Wins()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, didTeam1WinGame: true, didTeam2WinGame: false, dealWinningTeam: Team.Team1);

        entity.PlayCardDecisions.Should().HaveCount(4);

        foreach (var decision in entity.PlayCardDecisions)
        {
            decision.DidTeamWinTrick.Should().NotBeNull();
            decision.DidTeamWinDeal.Should().NotBeNull();
            decision.DidTeamWinGame.Should().NotBeNull();
        }

        entity.PlayCardDecisions.Should().Contain(d => d.DidTeamWinTrick == true);
    }

    [Fact]
    public void Map_ShouldSetOutcomeFlagsCorrectly_WhenTeam2Wins()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        trick.WinningTeam = Team.Team2;
        trick.WinningPosition = PlayerPosition.East;
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, didTeam1WinGame: false, didTeam2WinGame: true, dealWinningTeam: Team.Team2);

        entity.PlayCardDecisions.Should().HaveCount(4);

        foreach (var decision in entity.PlayCardDecisions)
        {
            decision.DidTeamWinTrick.Should().NotBeNull();
            decision.DidTeamWinDeal.Should().NotBeNull();
            decision.DidTeamWinGame.Should().NotBeNull();
        }

        entity.PlayCardDecisions.Should().Contain(d => d.DidTeamWinTrick == true);
    }

    [Fact]
    public void Map_ShouldMapActorTypeFromGamePlayers()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = new Dictionary<PlayerPosition, Player>
        {
            { PlayerPosition.North, new Player { Position = PlayerPosition.North, ActorType = ActorType.Chaos } },
            { PlayerPosition.East, new Player { Position = PlayerPosition.East, ActorType = ActorType.Chad } },
            { PlayerPosition.South, new Player { Position = PlayerPosition.South, ActorType = ActorType.Beta } },
            { PlayerPosition.West, new Player { Position = PlayerPosition.West, ActorType = ActorType.Chaos } },
        };

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, didTeam1WinGame: false, didTeam2WinGame: false, dealWinningTeam: Team.Team1);

        var decisions = entity.PlayCardDecisions.ToList();
        foreach (var decision in decisions)
        {
            decision.ActorType.Should().NotBeNull();
        }

        var northDecision = decisions[0];
        var firstPosition = trick.PlayCardDecisions[0].PlayerPosition;
        northDecision.ActorType.Should().Be(gamePlayers[firstPosition].ActorType);
    }

    private static Trick CreateSampleTrick()
    {
        var trick = new Trick
        {
            TrickNumber = 1,
            LeadPosition = PlayerPosition.North,
            LeadSuit = Suit.Hearts,
            WinningPosition = PlayerPosition.South,
            WinningTeam = Team.Team1,
        };

        trick.CardsPlayed.Add(new PlayedCard
        {
            Card = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
            PlayerPosition = PlayerPosition.North,
        });

        trick.CardsPlayed.Add(new PlayedCard
        {
            Card = new Card { Suit = Suit.Hearts, Rank = Rank.King },
            PlayerPosition = PlayerPosition.East,
        });

        return trick;
    }

    private static Trick CreateTrickWithDecisions()
    {
        var trick = new Trick
        {
            TrickNumber = 1,
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

            trick.PlayCardDecisions.Add(new PlayCardDecisionRecord
            {
                CardsInHand = [new Card { Suit = Suit.Hearts, Rank = Rank.Nine }, new Card { Suit = Suit.Clubs, Rank = Rank.Ten }],
                PlayerPosition = position,
                TeamScore = 2,
                OpponentScore = 0,
                TrumpSuit = Suit.Hearts,
                LeadPlayer = PlayerPosition.North,
                LeadSuit = Suit.Hearts,
                PlayedCards = new Dictionary<PlayerPosition, Card>
                {
                    { PlayerPosition.North, new Card { Suit = Suit.Hearts, Rank = Rank.Nine } },
                },
                WinningTrickPlayer = PlayerPosition.North,
                ValidCardsToPlay = [new Card { Suit = Suit.Hearts, Rank = Rank.Nine }],
                ChosenCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
            });
        }

        return trick;
    }

    private static Dictionary<PlayerPosition, Player> CreateSamplePlayers()
    {
        return new Dictionary<PlayerPosition, Player>
        {
            { PlayerPosition.North, new Player { Position = PlayerPosition.North, ActorType = ActorType.Chaos } },
            { PlayerPosition.East, new Player { Position = PlayerPosition.East, ActorType = ActorType.Chaos } },
            { PlayerPosition.South, new Player { Position = PlayerPosition.South, ActorType = ActorType.Chaos } },
            { PlayerPosition.West, new Player { Position = PlayerPosition.West, ActorType = ActorType.Chaos } },
        };
    }
}
