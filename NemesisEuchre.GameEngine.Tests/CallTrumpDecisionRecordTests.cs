using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Tests;

public class CallTrumpDecisionRecordTests
{
    [Fact]
    public void CallTrumpDecisionRecord_DefaultInitialization_SetsCollectionsToEmpty()
    {
        var record = new CallTrumpDecisionRecord();

        record.Hand.Should().NotBeNull();
        record.Hand.Should().BeEmpty();
        record.ValidDecisions.Should().NotBeNull();
        record.ValidDecisions.Should().BeEmpty();
    }

    [Fact]
    public void Hand_CanStore5Cards()
    {
        var record = new CallTrumpDecisionRecord
        {
            Hand =
            [
                new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
                new Card { Suit = Suit.Hearts, Rank = Rank.King },
                new Card { Suit = Suit.Hearts, Rank = Rank.Queen },
                new Card { Suit = Suit.Hearts, Rank = Rank.Jack },
                new Card { Suit = Suit.Hearts, Rank = Rank.Ten },
            ],
        };

        record.Hand.Should().HaveCount(5);
        record.Hand.Should().AllBeOfType<Card>();
    }

    [Fact]
    public void ValidDecisions_CanStoreRound1Decisions()
    {
        var record = new CallTrumpDecisionRecord
        {
            ValidDecisions =
            [
                CallTrumpDecision.Pass,
                CallTrumpDecision.OrderItUp,
                CallTrumpDecision.OrderItUpAndGoAlone,
            ],
        };

        record.ValidDecisions.Should().HaveCount(3);
        record.ValidDecisions.Should().Contain(CallTrumpDecision.Pass);
        record.ValidDecisions.Should().Contain(CallTrumpDecision.OrderItUp);
        record.ValidDecisions.Should().Contain(CallTrumpDecision.OrderItUpAndGoAlone);
    }

    [Fact]
    public void ValidDecisions_CanStoreRound2Decisions()
    {
        var record = new CallTrumpDecisionRecord
        {
            ValidDecisions =
            [
                CallTrumpDecision.Pass,
                CallTrumpDecision.CallClubs,
                CallTrumpDecision.CallDiamonds,
                CallTrumpDecision.CallHearts,
                CallTrumpDecision.CallSpades,
                CallTrumpDecision.CallClubsAndGoAlone,
                CallTrumpDecision.CallDiamondsAndGoAlone,
                CallTrumpDecision.CallHeartsAndGoAlone,
                CallTrumpDecision.CallSpadesAndGoAlone,
            ],
        };

        record.ValidDecisions.Should().HaveCount(9);
        record.ValidDecisions.Should().Contain(CallTrumpDecision.Pass);
        record.ValidDecisions.Should().Contain(CallTrumpDecision.CallClubs);
        record.ValidDecisions.Should().Contain(CallTrumpDecision.CallDiamonds);
        record.ValidDecisions.Should().Contain(CallTrumpDecision.CallHearts);
        record.ValidDecisions.Should().Contain(CallTrumpDecision.CallSpades);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void DecisionOrder_AcceptsValidRange(byte order)
    {
        var record = new CallTrumpDecisionRecord
        {
            DecisionOrder = order,
        };

        record.DecisionOrder.Should().Be(order);
    }

    [Fact]
    public void AllProperties_CanBeSetAndRetrieved()
    {
        Card[] hand =
        [
            new Card { Suit = Suit.Spades, Rank = Rank.Ace },
            new Card { Suit = Suit.Spades, Rank = Rank.King },
            new Card { Suit = Suit.Spades, Rank = Rank.Queen },
            new Card { Suit = Suit.Spades, Rank = Rank.Jack },
            new Card { Suit = Suit.Spades, Rank = Rank.Nine },
        ];

        var upCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };

        CallTrumpDecision[] validDecisions =
        [
            CallTrumpDecision.Pass,
            CallTrumpDecision.OrderItUp,
            CallTrumpDecision.OrderItUpAndGoAlone,
        ];

        var record = new CallTrumpDecisionRecord
        {
            Hand = hand,
            UpCard = upCard,
            DealerPosition = PlayerPosition.North,
            DecidingPlayerPosition = PlayerPosition.East,
            TeamScore = 5,
            OpponentScore = 3,
            ValidDecisions = validDecisions,
            ChosenDecision = CallTrumpDecision.OrderItUp,
            DecisionOrder = 2,
        };

        record.Hand.Should().BeEquivalentTo(hand);
        record.UpCard.Should().BeEquivalentTo(upCard);
        record.DealerPosition.Should().Be(PlayerPosition.North);
        record.DecidingPlayerPosition.Should().Be(PlayerPosition.East);
        record.TeamScore.Should().Be(5);
        record.OpponentScore.Should().Be(3);
        record.ValidDecisions.Should().BeEquivalentTo(validDecisions);
        record.ChosenDecision.Should().Be(CallTrumpDecision.OrderItUp);
        record.DecisionOrder.Should().Be(2);
    }

    [Fact]
    public void Deal_CallTrumpDecisions_InitializesToEmptyList()
    {
        var deal = new Deal();

        deal.CallTrumpDecisions.Should().NotBeNull();
        deal.CallTrumpDecisions.Should().BeEmpty();
        deal.CallTrumpDecisions.Should().BeOfType<List<CallTrumpDecisionRecord>>();
    }

    [Fact]
    public void Deal_CallTrumpDecisions_CanAddMultipleRecords()
    {
        var deal = new Deal();

        for (byte i = 1; i <= 8; i++)
        {
            var record = new CallTrumpDecisionRecord
            {
                DecisionOrder = i,
                DecidingPlayerPosition = (PlayerPosition)(i % 4),
                ChosenDecision = CallTrumpDecision.Pass,
            };
            deal.CallTrumpDecisions.Add(record);
        }

        deal.CallTrumpDecisions.Should().HaveCount(8);
        deal.CallTrumpDecisions[0].DecisionOrder.Should().Be(1);
        deal.CallTrumpDecisions[7].DecisionOrder.Should().Be(8);
    }

    [Fact]
    public void Deal_CallTrumpDecisions_RecordsMaintainIndependence()
    {
        var deal = new Deal();

        var record1 = new CallTrumpDecisionRecord
        {
            DecisionOrder = 1,
            ChosenDecision = CallTrumpDecision.Pass,
            TeamScore = 0,
        };

        var record2 = new CallTrumpDecisionRecord
        {
            DecisionOrder = 2,
            ChosenDecision = CallTrumpDecision.OrderItUp,
            TeamScore = 5,
        };

        deal.CallTrumpDecisions.Add(record1);
        deal.CallTrumpDecisions.Add(record2);

        deal.CallTrumpDecisions[0].TeamScore = 10;

        deal.CallTrumpDecisions[0].TeamScore.Should().Be(10);
        deal.CallTrumpDecisions[1].TeamScore.Should().Be(5);
    }
}
