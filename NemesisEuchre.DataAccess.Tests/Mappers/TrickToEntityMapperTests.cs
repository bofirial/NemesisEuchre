using FluentAssertions;

using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

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

        var entity = mapper.Map(trick, 1);

        entity.TrickNumber.Should().Be(1);
        entity.LeadPosition.Should().Be(PlayerPosition.North);
        entity.LeadSuit.Should().Be(Suit.Hearts);
        entity.WinningPosition.Should().Be(PlayerPosition.South);
        entity.WinningTeam.Should().Be(Team.Team1);
        entity.CardsPlayedJson.Should().NotBeNullOrEmpty();
    }
}
