using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests;

public class DiscardCardDecisionRecordTests
{
    [Fact]
    public void DiscardCardDecisionRecord_DefaultInitialization_SetsCollectionsToEmpty()
    {
        var record = new DiscardCardDecisionRecord() { ChosenCard = new(default, default) };

        record.CardsInHand.Should().NotBeNull();
        record.CardsInHand.Should().BeEmpty();
        record.ValidCardsToDiscard.Should().NotBeNull();
        record.ValidCardsToDiscard.Should().BeEmpty();
    }

    [Fact]
    public void Hand_CanStore6Cards()
    {
        var record = new DiscardCardDecisionRecord
        {
            CardsInHand =
            [
                new Card(Suit.Hearts, Rank.Ace),
                new Card(Suit.Hearts, Rank.King),
                new Card(Suit.Hearts, Rank.Queen),
                new Card(Suit.Hearts, Rank.Jack),
                new Card(Suit.Hearts, Rank.Ten),
                new Card(Suit.Hearts, Rank.Nine),
            ],
            ChosenCard = new Card(Suit.Hearts, Rank.Nine),
        };

        record.CardsInHand.Should().HaveCount(6);
        record.CardsInHand.Should().AllBeOfType<Card>();
    }

    [Fact]
    public void ValidCardsToDiscard_CanStore6Cards()
    {
        var record = new DiscardCardDecisionRecord
        {
            ValidCardsToDiscard =
            [
                new Card(Suit.Spades, Rank.Ace),
                new Card(Suit.Spades, Rank.King),
                new Card(Suit.Spades, Rank.Queen),
                new Card(Suit.Spades, Rank.Jack),
                new Card(Suit.Spades, Rank.Ten),
                new Card(Suit.Spades, Rank.Nine),
            ],
            ChosenCard = new Card(Suit.Hearts, Rank.Nine),
        };

        record.ValidCardsToDiscard.Should().HaveCount(6);
        record.ValidCardsToDiscard.Should().AllBeOfType<Card>();
    }

    [Fact]
    public void AllProperties_CanBeSetAndRetrieved()
    {
        Card[] hand =
        [
            new Card(Suit.Hearts, Rank.Ace),
            new Card(Suit.Hearts, Rank.King),
            new Card(Suit.Hearts, Rank.Queen),
            new Card(Suit.Hearts, Rank.Jack),
            new Card(Suit.Hearts, Rank.Ten),
            new Card(Suit.Hearts, Rank.Nine),
        ];

        Card[] validCardsToDiscard =
        [
            new Card(Suit.Spades, Rank.Ace),
            new Card(Suit.Spades, Rank.King),
            new Card(Suit.Spades, Rank.Queen),
            new Card(Suit.Spades, Rank.Jack),
            new Card(Suit.Spades, Rank.Ten),
            new Card(Suit.Spades, Rank.Nine),
        ];

        var chosenCard = new Card(Suit.Diamonds, Rank.Ten);

        var record = new DiscardCardDecisionRecord
        {
            CardsInHand = hand,
            PlayerPosition = PlayerPosition.North,
            TeamScore = 5,
            OpponentScore = 3,
            TrumpSuit = Suit.Hearts,
            CallingPlayer = PlayerPosition.East,
            CallingPlayerGoingAlone = true,
            ValidCardsToDiscard = validCardsToDiscard,
            ChosenCard = chosenCard,
        };

        record.CardsInHand.Should().BeEquivalentTo(hand);
        record.PlayerPosition.Should().Be(PlayerPosition.North);
        record.TeamScore.Should().Be(5);
        record.OpponentScore.Should().Be(3);
        record.TrumpSuit.Should().Be(Suit.Hearts);
        record.CallingPlayer.Should().Be(PlayerPosition.East);
        record.CallingPlayerGoingAlone.Should().BeTrue();
        record.ValidCardsToDiscard.Should().BeEquivalentTo(validCardsToDiscard);
        record.ChosenCard.Should().BeEquivalentTo(chosenCard);
    }

    [Fact]
    public void Deal_DiscardCardDecisions_InitializesToEmptyList()
    {
        var deal = new Deal();

        deal.DiscardCardDecisions.Should().NotBeNull();
        deal.DiscardCardDecisions.Should().BeEmpty();
        deal.DiscardCardDecisions.Should().BeOfType<List<DiscardCardDecisionRecord>>();
    }

    [Fact]
    public void Deal_DiscardCardDecisions_CanAddRecord()
    {
        var deal = new Deal();

        var record = new DiscardCardDecisionRecord
        {
            PlayerPosition = PlayerPosition.South,
            TeamScore = 2,
            OpponentScore = 4,
            ChosenCard = new Card(Suit.Clubs, Rank.Nine),
        };

        deal.DiscardCardDecisions.Add(record);

        deal.DiscardCardDecisions.Should().HaveCount(1);
        deal.DiscardCardDecisions[0].PlayerPosition.Should().Be(PlayerPosition.South);
        deal.DiscardCardDecisions[0].TeamScore.Should().Be(2);
        deal.DiscardCardDecisions[0].OpponentScore.Should().Be(4);
    }

    [Fact]
    public void Deal_DiscardCardDecisions_RecordMaintainsIndependence()
    {
        var deal = new Deal();

        var record1 = new DiscardCardDecisionRecord
        {
            PlayerPosition = PlayerPosition.North,
            TeamScore = 0,
            ChosenCard = new Card(Suit.Hearts, Rank.Nine),
        };

        var record2 = new DiscardCardDecisionRecord
        {
            PlayerPosition = PlayerPosition.South,
            TeamScore = 5,
            ChosenCard = new Card(Suit.Hearts, Rank.Nine),
        };

        deal.DiscardCardDecisions.Add(record1);
        deal.DiscardCardDecisions.Add(record2);

        deal.DiscardCardDecisions[0].TeamScore = 10;

        deal.DiscardCardDecisions[0].TeamScore.Should().Be(10);
        deal.DiscardCardDecisions[1].TeamScore.Should().Be(5);
    }
}
