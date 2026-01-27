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

        record.CardsInHand.Should().NotBeNull();
        record.CardsInHand.Should().BeEmpty();
        record.ValidCardsToPlay.Should().NotBeNull();
        record.ValidCardsToPlay.Should().BeEmpty();
        record.PlayedCards.Should().NotBeNull();
        record.PlayedCards.Should().BeEmpty();
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
            CardsInHand = hand,
        };

        record.CardsInHand.Should().HaveCount(cardCount);
        record.CardsInHand.Should().AllBeOfType<Card>();
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
            CardsInHand = hand,
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
            CardsInHand = hand,
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
            CardsInHand = hand,
            ValidCardsToPlay = hand,
        };

        record.ValidCardsToPlay.Should().HaveCount(3);
        record.ValidCardsToPlay.Should().BeEquivalentTo(hand);
    }

    [Theory]
    [InlineData(PlayerPosition.North)]
    [InlineData(PlayerPosition.East)]
    [InlineData(PlayerPosition.South)]
    [InlineData(PlayerPosition.West)]
    public void LeadPlayer_AcceptsAllPlayerPositions(PlayerPosition position)
    {
        var record = new PlayCardDecisionRecord
        {
            LeadPlayer = position,
        };

        record.LeadPlayer.Should().Be(position);
    }

    [Fact]
    public void LeadSuit_CanBeNullWhenLeading()
    {
        var record = new PlayCardDecisionRecord
        {
            LeadSuit = null,
        };

        record.LeadSuit.Should().BeNull();
    }

    [Fact]
    public void LeadSuit_CanBeSetWhenFollowing()
    {
        var record = new PlayCardDecisionRecord
        {
            LeadSuit = Suit.Hearts,
        };

        record.LeadSuit.Should().Be(Suit.Hearts);
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

        var playedCards = new Dictionary<PlayerPosition, Card>
        {
            { PlayerPosition.East, new Card { Suit = Suit.Spades, Rank = Rank.Queen } },
        };

        var record = new PlayCardDecisionRecord
        {
            CardsInHand = hand,
            PlayerPosition = PlayerPosition.North,
            TeamScore = 7,
            OpponentScore = 4,
            TrumpSuit = Suit.Clubs,
            LeadPlayer = PlayerPosition.East,
            LeadSuit = Suit.Spades,
            PlayedCards = playedCards,
            WinningTrickPlayer = PlayerPosition.East,
            ValidCardsToPlay = validCardsToPlay,
            ChosenCard = chosenCard,
        };

        record.CardsInHand.Should().BeEquivalentTo(hand);
        record.PlayerPosition.Should().Be(PlayerPosition.North);
        record.TeamScore.Should().Be(7);
        record.OpponentScore.Should().Be(4);
        record.TrumpSuit.Should().Be(Suit.Clubs);
        record.LeadPlayer.Should().Be(PlayerPosition.East);
        record.LeadSuit.Should().Be(Suit.Spades);
        record.PlayedCards.Should().BeEquivalentTo(playedCards);
        record.WinningTrickPlayer.Should().Be(PlayerPosition.East);
        record.ValidCardsToPlay.Should().BeEquivalentTo(validCardsToPlay);
        record.ChosenCard.Should().BeEquivalentTo(chosenCard);
    }

    [Fact]
    public void Trick_PlayCardDecisions_InitializesToEmptyList()
    {
        var trick = new Trick();

        trick.PlayCardDecisions.Should().NotBeNull();
        trick.PlayCardDecisions.Should().BeEmpty();
        trick.PlayCardDecisions.Should().BeOfType<List<PlayCardDecisionRecord>>();
    }

    [Fact]
    public void Trick_PlayCardDecisions_CanAddMultipleRecords()
    {
        var trick = new Trick();

        for (int i = 0; i < 4; i++)
        {
            var record = new PlayCardDecisionRecord
            {
                PlayerPosition = PlayerPosition.North,
                TeamScore = 5,
                OpponentScore = 3,
                TrumpSuit = Suit.Hearts,
                LeadPlayer = PlayerPosition.North,
                ChosenCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
            };

            trick.PlayCardDecisions.Add(record);
        }

        trick.PlayCardDecisions.Should().HaveCount(4);
    }

    [Fact]
    public void Trick_PlayCardDecisions_RecordsMaintainIndependence()
    {
        var trick = new Trick();

        var record1 = new PlayCardDecisionRecord
        {
            PlayerPosition = PlayerPosition.North,
            TeamScore = 0,
            TrumpSuit = Suit.Hearts,
            LeadPlayer = PlayerPosition.North,
        };

        var record2 = new PlayCardDecisionRecord
        {
            PlayerPosition = PlayerPosition.South,
            TeamScore = 5,
            TrumpSuit = Suit.Hearts,
            LeadPlayer = PlayerPosition.East,
        };

        trick.PlayCardDecisions.Add(record1);
        trick.PlayCardDecisions.Add(record2);

        trick.PlayCardDecisions[0].TeamScore = 10;

        trick.PlayCardDecisions[0].TeamScore.Should().Be(10);
        trick.PlayCardDecisions[1].TeamScore.Should().Be(5);
    }
}
