using FluentAssertions;

using Moq;

using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.DataAccess.Tests.Mappers;

public class DealToEntityMapperTests
{
    private readonly Mock<ITrickToEntityMapper> _mockTrickMapper;
    private readonly DealToEntityMapper _mapper;

    public DealToEntityMapperTests()
    {
        _mockTrickMapper = new Mock<ITrickToEntityMapper>();
        _mapper = new DealToEntityMapper(_mockTrickMapper.Object);
    }

    [Fact]
    public void Map_WithEmptyKnownPlayerSuitVoids_SetsJsonToNull()
    {
        var deal = CreateTestDeal();
        deal.KnownPlayerSuitVoids = [];

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, false, false);

        entity.KnownPlayerSuitVoidsJson.Should().BeNull();
    }

    [Fact]
    public void Map_WithKnownPlayerSuitVoids_SerializesCorrectly()
    {
        var deal = CreateTestDeal();
        deal.KnownPlayerSuitVoids =
        [
            (PlayerPosition.North, Suit.Spades),
            (PlayerPosition.East, Suit.Hearts),
        ];

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, false, false);

        entity.KnownPlayerSuitVoidsJson.Should().NotBeNullOrEmpty();

        var json = entity.KnownPlayerSuitVoidsJson!;
        json.Should().Contain("North");
        json.Should().Contain("Spades");
        json.Should().Contain("East");
        json.Should().Contain("Hearts");
    }

    [Fact]
    public void Map_KnownPlayerSuitVoidsRoundtrip_PreservesData()
    {
        var deal = CreateTestDeal();
        deal.KnownPlayerSuitVoids =
        [
            (PlayerPosition.South, Suit.Clubs),
            (PlayerPosition.West, Suit.Diamonds),
            (PlayerPosition.North, Suit.Hearts),
        ];

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, false, false);

        var json = entity.KnownPlayerSuitVoidsJson!;
        json.Should().Contain("South");
        json.Should().Contain("Clubs");
        json.Should().Contain("West");
        json.Should().Contain("Diamonds");
        json.Should().Contain("North");
        json.Should().Contain("Hearts");
    }

    private static Deal CreateTestDeal()
    {
        return new Deal
        {
            DealNumber = 1,
            DealStatus = DealStatus.Complete,
            DealerPosition = PlayerPosition.North,
            Deck = [],
            UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
            Trump = Suit.Hearts,
            CallingPlayer = PlayerPosition.North,
            CallingPlayerIsGoingAlone = false,
            ChosenDecision = CallTrumpDecision.OrderItUp,
            DealResult = DealResult.WonStandardBid,
            WinningTeam = Team.Team1,
            Team1Score = 1,
            Team2Score = 0,
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                [PlayerPosition.North] = new DealPlayer
                {
                    Position = PlayerPosition.North,
                    ActorType = ActorType.Chaos,
                    StartingHand = [],
                    CurrentHand = [],
                },
                [PlayerPosition.East] = new DealPlayer
                {
                    Position = PlayerPosition.East,
                    ActorType = ActorType.Chaos,
                    StartingHand = [],
                    CurrentHand = [],
                },
                [PlayerPosition.South] = new DealPlayer
                {
                    Position = PlayerPosition.South,
                    ActorType = ActorType.Chaos,
                    StartingHand = [],
                    CurrentHand = [],
                },
                [PlayerPosition.West] = new DealPlayer
                {
                    Position = PlayerPosition.West,
                    ActorType = ActorType.Chaos,
                    StartingHand = [],
                    CurrentHand = [],
                },
            },
        };
    }

    private static Dictionary<PlayerPosition, Player> CreateTestGamePlayers()
    {
        return new Dictionary<PlayerPosition, Player>
        {
            [PlayerPosition.North] = new Player
            {
                Position = PlayerPosition.North,
                ActorType = ActorType.Chaos,
            },
            [PlayerPosition.East] = new Player
            {
                Position = PlayerPosition.East,
                ActorType = ActorType.Chaos,
            },
            [PlayerPosition.South] = new Player
            {
                Position = PlayerPosition.South,
                ActorType = ActorType.Chaos,
            },
            [PlayerPosition.West] = new Player
            {
                Position = PlayerPosition.West,
                ActorType = ActorType.Chaos,
            },
        };
    }
}
