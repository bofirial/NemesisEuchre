using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests;

public class PlayCardDecisionRecordTests
{
    [Fact]
    public void PlayCardDecisionRecord_DefaultInitialization_SetsCollectionsToEmpty()
    {
        var record = new PlayCardDecisionRecord();

        record.Hand.Should().NotBeNull();
        record.Hand.Should().BeEmpty();
        record.ValidCardsToPlay.Should().NotBeNull();
        record.ValidCardsToPlay.Should().BeEmpty();
        record.CurrentTrick.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Hand_CanStoreVariableCardCounts(int cardCount)
    {
        var hand = new Card[cardCount];
        for (int i = 0; i < cardCount; i++)
        {
            hand[i] = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        }

        var record = new PlayCardDecisionRecord
        {
            Hand = hand,
        };

        record.Hand.Should().HaveCount(cardCount);
        record.Hand.Should().AllBeOfType<Card>();
    }

    [Fact]
    public void ValidCardsToPlay_CanStoreAllHandCardsWhenLeading()
    {
        Card[] hand =
        [
            new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
            new Card { Suit = Suit.Hearts, Rank = Rank.King },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new Card { Suit = Suit.Spades, Rank = Rank.Jack },
            new Card { Suit = Suit.Clubs, Rank = Rank.Ten },
        ];

        var record = new PlayCardDecisionRecord
        {
            Hand = hand,
            ValidCardsToPlay = hand,
        };

        record.ValidCardsToPlay.Should().HaveCount(5);
        record.ValidCardsToPlay.Should().BeEquivalentTo(hand);
    }

    [Fact]
    public void ValidCardsToPlay_CanStoreSubsetWhenFollowingSuit()
    {
        Card[] hand =
        [
            new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
            new Card { Suit = Suit.Hearts, Rank = Rank.King },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new Card { Suit = Suit.Spades, Rank = Rank.Jack },
        ];

        Card[] validCards =
        [
            new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
            new Card { Suit = Suit.Hearts, Rank = Rank.King },
        ];

        var record = new PlayCardDecisionRecord
        {
            Hand = hand,
            ValidCardsToPlay = validCards,
        };

        record.ValidCardsToPlay.Should().HaveCount(2);
        record.ValidCardsToPlay.Should().BeEquivalentTo(validCards);
    }

    [Fact]
    public void ValidCardsToPlay_CanStoreAllWhenCannotFollowSuit()
    {
        Card[] hand =
        [
            new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
            new Card { Suit = Suit.Spades, Rank = Rank.Jack },
            new Card { Suit = Suit.Clubs, Rank = Rank.Ten },
        ];

        var record = new PlayCardDecisionRecord
        {
            Hand = hand,
            ValidCardsToPlay = hand,
        };

        record.ValidCardsToPlay.Should().HaveCount(3);
        record.ValidCardsToPlay.Should().BeEquivalentTo(hand);
    }

    [Fact]
    public void CurrentTrick_CanBeEmptyWhenLeading()
    {
        var record = new PlayCardDecisionRecord
        {
            CurrentTrick = new Trick
            {
                LeadPosition = PlayerPosition.North,
            },
        };

        record.CurrentTrick.CardsPlayed.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CurrentTrick_CanStoreCardsPlayedBeforeDecision(int cardsPlayed)
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.East,
        };

        for (int i = 0; i < cardsPlayed; i++)
        {
            trick.CardsPlayed.Add(new PlayedCard
            {
                Card = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
                PlayerPosition = PlayerPosition.East,
            });
        }

        var record = new PlayCardDecisionRecord
        {
            CurrentTrick = trick,
        };

        record.CurrentTrick.CardsPlayed.Should().HaveCount(cardsPlayed);
        record.CurrentTrick.CardsPlayed.Should().AllBeOfType<PlayedCard>();
    }

    [Theory]
    [InlineData(PlayerPosition.North)]
    [InlineData(PlayerPosition.East)]
    [InlineData(PlayerPosition.South)]
    [InlineData(PlayerPosition.West)]
    public void LeadPosition_AcceptsAllPlayerPositions(PlayerPosition position)
    {
        var record = new PlayCardDecisionRecord
        {
            LeadPosition = position,
        };

        record.LeadPosition.Should().Be(position);
    }

    [Fact]
    public void AllProperties_CanBeSetAndRetrieved()
    {
        Card[] hand =
        [
            new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
            new Card { Suit = Suit.Hearts, Rank = Rank.King },
            new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
        ];

        Card[] validCardsToPlay =
        [
            new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
            new Card { Suit = Suit.Hearts, Rank = Rank.King },
        ];

        var chosenCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };

        var currentTrick = new Trick
        {
            LeadPosition = PlayerPosition.East,
            LeadSuit = Suit.Spades,
        };
        currentTrick.CardsPlayed.Add(new PlayedCard
        {
            Card = new Card { Suit = Suit.Spades, Rank = Rank.Queen },
            PlayerPosition = PlayerPosition.East,
        });

        var record = new PlayCardDecisionRecord
        {
            Hand = hand,
            DecidingPlayerPosition = PlayerPosition.North,
            CurrentTrick = currentTrick,
            TeamScore = 7,
            OpponentScore = 4,
            ValidCardsToPlay = validCardsToPlay,
            ChosenCard = chosenCard,
            LeadPosition = PlayerPosition.East,
        };

        record.Hand.Should().BeEquivalentTo(hand);
        record.DecidingPlayerPosition.Should().Be(PlayerPosition.North);
        record.CurrentTrick.Should().BeEquivalentTo(currentTrick);
        record.TeamScore.Should().Be(7);
        record.OpponentScore.Should().Be(4);
        record.ValidCardsToPlay.Should().BeEquivalentTo(validCardsToPlay);
        record.ChosenCard.Should().BeEquivalentTo(chosenCard);
        record.LeadPosition.Should().Be(PlayerPosition.East);
    }

    [Fact]
    public void Deal_PlayCardDecisions_InitializesToEmptyList()
    {
        var deal = new Deal();

        deal.PlayCardDecisions.Should().NotBeNull();
        deal.PlayCardDecisions.Should().BeEmpty();
        deal.PlayCardDecisions.Should().BeOfType<List<PlayCardDecisionRecord>>();
    }

    [Fact]
    public void Deal_PlayCardDecisions_CanAddMultipleRecords()
    {
        var deal = new Deal();

        for (int i = 0; i < 20; i++)
        {
            var record = new PlayCardDecisionRecord
            {
                DecidingPlayerPosition = PlayerPosition.North,
                TeamScore = 5,
                OpponentScore = 3,
                ChosenCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
                LeadPosition = PlayerPosition.North,
            };

            deal.PlayCardDecisions.Add(record);
        }

        deal.PlayCardDecisions.Should().HaveCount(20);
    }

    [Fact]
    public void Deal_PlayCardDecisions_CanAddFewerRecordsForGoingAlone()
    {
        var deal = new Deal();

        for (int i = 0; i < 15; i++)
        {
            var record = new PlayCardDecisionRecord
            {
                DecidingPlayerPosition = PlayerPosition.South,
                TeamScore = 2,
                OpponentScore = 1,
                ChosenCard = new Card { Suit = Suit.Spades, Rank = Rank.King },
                LeadPosition = PlayerPosition.East,
            };

            deal.PlayCardDecisions.Add(record);
        }

        deal.PlayCardDecisions.Should().HaveCount(15);
    }

    [Fact]
    public void Deal_PlayCardDecisions_RecordsMaintainIndependence()
    {
        var deal = new Deal();

        var record1 = new PlayCardDecisionRecord
        {
            DecidingPlayerPosition = PlayerPosition.North,
            TeamScore = 0,
            LeadPosition = PlayerPosition.North,
        };

        var record2 = new PlayCardDecisionRecord
        {
            DecidingPlayerPosition = PlayerPosition.South,
            TeamScore = 5,
            LeadPosition = PlayerPosition.East,
        };

        deal.PlayCardDecisions.Add(record1);
        deal.PlayCardDecisions.Add(record2);

        deal.PlayCardDecisions[0].TeamScore = 10;

        deal.PlayCardDecisions[0].TeamScore.Should().Be(10);
        deal.PlayCardDecisions[1].TeamScore.Should().Be(5);
    }
}
