using FluentAssertions;

using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.DataAccess.Models;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Tests.Mappers;

public class TrickToEntityMapperTests
{
    [Fact]
    public void Map_ShouldMapTrickEntityProperties()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateSampleTrick();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 3, gamePlayers, new GameOutcomeContext(false, false), dealWinningTeam: Team.Team1, dealResult: DealResult.WonStandardBid);

        entity.TrickNumber.Should().Be(3);
        entity.LeadPlayerPositionId.Should().Be((int)PlayerPosition.North);
        entity.LeadSuitId.Should().Be((int)Suit.Hearts);
        entity.WinningPlayerPositionId.Should().Be((int)PlayerPosition.South);
        entity.WinningTeamId.Should().Be((int)Team.Team1);
        entity.TrickCardsPlayed.Should().HaveCount(2);
    }

    [Fact]
    public void Map_ShouldCreatePlayCardDecisionEntitiesCollection()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, new GameOutcomeContext(false, false), dealWinningTeam: Team.Team1, dealResult: DealResult.WonStandardBid);

        entity.PlayCardDecisions.Should().NotBeNull();
        entity.PlayCardDecisions.Should().HaveCount(4);
    }

    [Fact]
    public void Map_ShouldMapPlayCardDecisionProperties()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, new GameOutcomeContext(true, false), dealWinningTeam: Team.Team1, dealResult: DealResult.WonStandardBid);

        var decision = entity.PlayCardDecisions[0];
        decision.CardsInHand.Should().NotBeEmpty();
        decision.TeamScore.Should().Be(2);
        decision.OpponentScore.Should().Be(0);
        decision.ValidCards.Should().NotBeEmpty();
        decision.ChosenRelativeCardId.Should().BeGreaterThan(0);
        decision.ActorTypeId.Should().Be((int)ActorType.Chaos);
    }

    [Fact]
    public void Map_ShouldConvertAbsolutePositionsToRelativePositions()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, new GameOutcomeContext(false, false), dealWinningTeam: Team.Team1, dealResult: DealResult.WonStandardBid);

        var northDecision = entity.PlayCardDecisions[0];
        northDecision.LeadRelativePlayerPositionId.Should().Be((int)RelativePlayerPosition.Self);

        entity.PlayCardDecisions.Should().HaveCountGreaterThan(1);
        entity.PlayCardDecisions.Should().Contain(d => d.LeadRelativePlayerPositionId != (int)RelativePlayerPosition.Self);
    }

    [Fact]
    public void Map_ShouldConvertAbsoluteSuitsToRelativeSuits()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, new GameOutcomeContext(false, false), dealWinningTeam: Team.Team1, dealResult: DealResult.WonStandardBid);

        var decision = entity.PlayCardDecisions[0];
        decision.LeadRelativeSuitId.Should().Be((int)RelativeSuit.Trump);
    }

    [Fact]
    public void Map_ShouldConvertCardsToRelativeCards()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, new GameOutcomeContext(false, false), dealWinningTeam: Team.Team1, dealResult: DealResult.WonStandardBid);

        var decision = entity.PlayCardDecisions[0];
        decision.CardsInHand.Should().NotBeEmpty();
        decision.CardsInHand.Should().AllSatisfy(c => c.RelativeCardId.Should().BeGreaterThan(0));

        decision.ValidCards.Should().NotBeEmpty();
        decision.ValidCards.Should().AllSatisfy(c => c.RelativeCardId.Should().BeGreaterThan(0));

        decision.ChosenRelativeCardId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Map_ShouldConvertPlayedCardsDictionary()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, new GameOutcomeContext(false, false), dealWinningTeam: Team.Team1, dealResult: DealResult.WonStandardBid);

        var decision = entity.PlayCardDecisions[0];
        decision.PlayedCards.Should().NotBeEmpty();
        decision.PlayedCards.Should().AllSatisfy(p =>
        {
            p.RelativePlayerPositionId.Should().BeGreaterThanOrEqualTo(0);
            p.RelativeCardId.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public void Map_ShouldSetOutcomeFlagsCorrectly_WhenTeam1Wins()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, new GameOutcomeContext(true, false), dealWinningTeam: Team.Team1, dealResult: DealResult.WonStandardBid);

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

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, new GameOutcomeContext(false, true), dealWinningTeam: Team.Team2, dealResult: DealResult.WonStandardBid);

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
            { PlayerPosition.North, new Player { Position = PlayerPosition.North, Actor = new Actor(ActorType.Chaos, null) } },
            { PlayerPosition.East, new Player { Position = PlayerPosition.East, Actor = new Actor(ActorType.Chad, null) } },
            { PlayerPosition.South, new Player { Position = PlayerPosition.South, Actor = new Actor(ActorType.Beta, null) } },
            { PlayerPosition.West, new Player { Position = PlayerPosition.West, Actor = new Actor(ActorType.Chaos, null) } },
        };

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, new GameOutcomeContext(false, false), dealWinningTeam: Team.Team1, dealResult: DealResult.WonStandardBid);

        var decisions = entity.PlayCardDecisions.ToList();
        foreach (var decision in decisions)
        {
            decision.ActorTypeId.Should().NotBeNull();
        }

        var northDecision = decisions[0];
        var firstPosition = trick.PlayCardDecisions[0].PlayerPosition;
        northDecision.ActorTypeId.Should().Be((int?)gamePlayers[firstPosition].Actor.ActorType);
    }

    [Fact]
    public void Map_ShouldSerializeDecisionPredictedPoints()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, new GameOutcomeContext(false, false), dealWinningTeam: Team.Team1, dealResult: DealResult.WonStandardBid);

        var decision = entity.PlayCardDecisions[0];
        decision.PredictedPoints.Should().HaveCount(2);
        decision.PredictedPoints.Should().Contain(p => Math.Abs(p.PredictedPoints - 1.5f) < 0.001f);
        decision.PredictedPoints.Should().Contain(p => Math.Abs(p.PredictedPoints - 0.8f) < 0.001f);
    }

    [Fact]
    public void Map_WithEmptyDecisionPredictedPoints_SetsCollectionToEmpty()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateTrickWithDecisions();
        foreach (var d in trick.PlayCardDecisions)
        {
            d.DecisionPredictedPoints = [];
        }

        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, new GameOutcomeContext(false, false), dealWinningTeam: Team.Team1, dealResult: DealResult.WonStandardBid);

        entity.PlayCardDecisions.Should().AllSatisfy(d => d.PredictedPoints.Should().BeEmpty());
    }

    [Fact]
    public void Map_ShouldMapTrickCardsPlayed()
    {
        var mapper = new TrickToEntityMapper();
        var trick = CreateSampleTrick();
        var gamePlayers = CreateSamplePlayers();

        var entity = mapper.Map(trick, trickNumber: 1, gamePlayers, new GameOutcomeContext(false, false), dealWinningTeam: Team.Team1, dealResult: DealResult.WonStandardBid);

        entity.TrickCardsPlayed.Should().HaveCount(2);
        entity.TrickCardsPlayed.Should().Contain(c =>
            c.PlayerPositionId == (int)PlayerPosition.North &&
            c.CardId == CardIdHelper.ToCardId(new Card(Suit.Hearts, Rank.Ace)));
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

        trick.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.Ace), PlayerPosition.North));

        trick.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.King), PlayerPosition.East));

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
            trick.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.Nine), position));

            trick.PlayCardDecisions.Add(new PlayCardDecisionRecord
            {
                CardsInHand = [new Card(Suit.Hearts, Rank.Nine), new Card(Suit.Clubs, Rank.Ten)],
                PlayerPosition = position,
                TeamScore = 2,
                OpponentScore = 0,
                WonTricks = 0,
                OpponentsWonTricks = 0,
                TrumpSuit = Suit.Hearts,
                LeadPlayer = PlayerPosition.North,
                LeadSuit = Suit.Hearts,
                PlayedCards = new Dictionary<PlayerPosition, Card>
                {
                    { PlayerPosition.North, new Card(Suit.Hearts, Rank.Nine) },
                },
                WinningTrickPlayer = PlayerPosition.North,
                ValidCardsToPlay = [new Card(Suit.Hearts, Rank.Nine)],
                ChosenCard = new Card(Suit.Hearts, Rank.Nine),
                DecisionPredictedPoints = new Dictionary<Card, float>
                {
                    { new Card(Suit.Hearts, Rank.Nine), 1.5f },
                    { new Card(Suit.Clubs, Rank.Ten), 0.8f },
                },
            });
        }

        return trick;
    }

    private static Dictionary<PlayerPosition, Player> CreateSamplePlayers()
    {
        return new Dictionary<PlayerPosition, Player>
        {
            { PlayerPosition.North, new Player { Position = PlayerPosition.North, Actor = new Actor(ActorType.Chaos, null) } },
            { PlayerPosition.East, new Player { Position = PlayerPosition.East, Actor = new Actor(ActorType.Chaos, null) } },
            { PlayerPosition.South, new Player { Position = PlayerPosition.South, Actor = new Actor(ActorType.Chaos, null) } },
            { PlayerPosition.West, new Player { Position = PlayerPosition.West, Actor = new Actor(ActorType.Chaos, null) } },
        };
    }
}
