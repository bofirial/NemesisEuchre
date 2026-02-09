using FluentAssertions;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Tests.Mappers;

public class EntityToTrickMapperTests
{
    private readonly EntityToTrickMapper _mapper = new();

    [Fact]
    public void Map_WithBasicTrickEntity_MapsCoreFields()
    {
        var entity = CreateTestTrickEntity();

        var trick = _mapper.Map(entity, Suit.Hearts, PlayerPosition.North, includeDecisions: false);

        trick.TrickNumber.Should().Be(1);
        trick.LeadPosition.Should().Be(PlayerPosition.East);
        trick.LeadSuit.Should().Be(Suit.Hearts);
        trick.WinningPosition.Should().Be(PlayerPosition.South);
        trick.WinningTeam.Should().Be(Team.Team1);
    }

    [Fact]
    public void Map_WithCardsPlayed_OrdersByPlayOrder()
    {
        var entity = CreateTestTrickEntity();
        entity.TrickCardsPlayed =
        [
            new TrickCardPlayed { PlayerPositionId = (int)PlayerPosition.South, CardId = CardIdHelper.ToCardId(new Card(Suit.Hearts, Rank.King)), PlayOrder = 1 },
            new TrickCardPlayed { PlayerPositionId = (int)PlayerPosition.East, CardId = CardIdHelper.ToCardId(new Card(Suit.Hearts, Rank.Nine)), PlayOrder = 0 },
            new TrickCardPlayed { PlayerPositionId = (int)PlayerPosition.West, CardId = CardIdHelper.ToCardId(new Card(Suit.Spades, Rank.Ace)), PlayOrder = 2 },
        ];

        var trick = _mapper.Map(entity, Suit.Hearts, PlayerPosition.North, includeDecisions: false);

        trick.CardsPlayed.Should().HaveCount(3);
        trick.CardsPlayed[0].PlayerPosition.Should().Be(PlayerPosition.East);
        trick.CardsPlayed[1].PlayerPosition.Should().Be(PlayerPosition.South);
        trick.CardsPlayed[2].PlayerPosition.Should().Be(PlayerPosition.West);
    }

    [Fact]
    public void Map_WithoutDecisions_LeavesPlayCardDecisionsEmpty()
    {
        var entity = CreateTestTrickEntity();
        entity.PlayCardDecisions = [CreateTestPlayCardDecisionEntity(PlayerPosition.North)];

        var trick = _mapper.Map(entity, Suit.Hearts, PlayerPosition.North, includeDecisions: false);

        trick.PlayCardDecisions.Should().BeEmpty();
    }

    [Fact]
    public void Map_WithDecisions_MapsPlayCardDecisions()
    {
        var entity = CreateTestTrickEntity();
        entity.PlayCardDecisions = [CreateTestPlayCardDecisionEntity(PlayerPosition.East)];

        var trick = _mapper.Map(entity, Suit.Hearts, PlayerPosition.North, includeDecisions: true);

        trick.PlayCardDecisions.Should().HaveCount(1);
        var decision = trick.PlayCardDecisions[0];
        decision.TrumpSuit.Should().Be(Suit.Hearts);
        decision.TrickNumber.Should().Be(1);
    }

    [Fact]
    public void Map_PlayCardDecision_DerivesCorrectPlayerPosition()
    {
        var entity = CreateTestTrickEntity();
        var decisionEntity = CreateTestPlayCardDecisionEntity(PlayerPosition.East);

        entity.PlayCardDecisions = [decisionEntity];

        var trick = _mapper.Map(entity, Suit.Hearts, PlayerPosition.North, includeDecisions: true);

        trick.PlayCardDecisions[0].PlayerPosition.Should().Be(PlayerPosition.East);
    }

    [Fact]
    public void Map_WithNullLeadSuit_MapsToNull()
    {
        var entity = CreateTestTrickEntity();
        entity.LeadSuitId = null;

        var trick = _mapper.Map(entity, Suit.Hearts, PlayerPosition.North, includeDecisions: false);

        trick.LeadSuit.Should().BeNull();
    }

    [Fact]
    public void Map_WithNullWinningPosition_MapsToNull()
    {
        var entity = CreateTestTrickEntity();
        entity.WinningPlayerPositionId = null;
        entity.WinningTeamId = null;

        var trick = _mapper.Map(entity, Suit.Hearts, PlayerPosition.North, includeDecisions: false);

        trick.WinningPosition.Should().BeNull();
        trick.WinningTeam.Should().BeNull();
    }

    private static TrickEntity CreateTestTrickEntity()
    {
        return new TrickEntity
        {
            TrickId = 1,
            DealId = 1,
            TrickNumber = 1,
            LeadPlayerPositionId = (int)PlayerPosition.East,
            LeadSuitId = (int)Suit.Hearts,
            WinningPlayerPositionId = (int)PlayerPosition.South,
            WinningTeamId = (int)Team.Team1,
            TrickCardsPlayed = [],
            PlayCardDecisions = [],
        };
    }

    private static PlayCardDecisionEntity CreateTestPlayCardDecisionEntity(PlayerPosition self)
    {
        const PlayerPosition dealer = PlayerPosition.North;
        var dealerRelative = ((int)dealer - (int)self + 4) % 4;

        const PlayerPosition caller = PlayerPosition.East;
        var callerRelative = ((int)caller - (int)self + 4) % 4;

        const PlayerPosition lead = PlayerPosition.East;
        var leadRelative = ((int)lead - (int)self + 4) % 4;

        const Suit trump = Suit.Hearts;
        var chosenCard = new Card(Suit.Hearts, Rank.Ace);
        var relativeChosenCard = chosenCard.ToRelative(trump);

        return new PlayCardDecisionEntity
        {
            PlayCardDecisionId = 1,
            DealId = 1,
            TrickId = 1,
            TeamScore = 3,
            OpponentScore = 2,
            TrickNumber = 1,
            CallingPlayerGoingAlone = false,
            DealerRelativePlayerPositionId = dealerRelative,
            CallingRelativePlayerPositionId = callerRelative,
            LeadRelativePlayerPositionId = leadRelative,
            ChosenRelativeCardId = CardIdHelper.ToRelativeCardId(relativeChosenCard),
            CardsInHand = [],
            PlayedCards = [],
            ValidCards = [],
            KnownVoids = [],
            CardsAccountedFor = [],
            PredictedPoints = [],
        };
    }
}
