using FluentAssertions;

using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.DataAccess.Tests.Mappers;

public class TrickToEntityMapperTests
{
    [Fact]
    public void Map_ShouldMapAllProperties()
    {
        var mapper = new TrickToEntityMapper();
        var trick = new Trick
        {
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

        var gamePlayers = new Dictionary<PlayerPosition, Player>
        {
            { PlayerPosition.North, new Player { Position = PlayerPosition.North, ActorType = ActorType.Chaos } },
            { PlayerPosition.East, new Player { Position = PlayerPosition.East, ActorType = ActorType.Chaos } },
            { PlayerPosition.South, new Player { Position = PlayerPosition.South, ActorType = ActorType.Chaos } },
            { PlayerPosition.West, new Player { Position = PlayerPosition.West, ActorType = ActorType.Chaos } },
        };

        var entity = mapper.Map(trick, 1, gamePlayers, false, false, Team.Team1);

        entity.TrickNumber.Should().Be(1);
        entity.LeadPosition.Should().Be(PlayerPosition.North);
        entity.LeadSuit.Should().Be(Suit.Hearts);
        entity.WinningPosition.Should().Be(PlayerPosition.South);
        entity.WinningTeam.Should().Be(Team.Team1);
        entity.CardsPlayedJson.Should().NotBeNullOrEmpty();
    }
}
