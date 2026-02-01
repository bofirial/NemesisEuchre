using System.Text.Json;

using Bogus;

using FluentAssertions;

using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.MachineLearning.Tests.FeatureEngineering;

public class PlayCardFeatureEngineerTests
{
    private readonly PlayCardFeatureEngineer _engineer;
    private readonly Faker _faker;

    public PlayCardFeatureEngineerTests()
    {
        _engineer = new PlayCardFeatureEngineer();
        _faker = new Faker();
    }

    [Fact]
    public void Transform_WithValidEntity_MapsAllHandCards()
    {
        var cards = CreateRelativeCards(5);
        var entity = CreatePlayCardDecisionEntity(cards);

        var result = _engineer.Transform(entity);

        result.Card1Rank.Should().Be((float)cards[0].Rank);
        result.Card1Suit.Should().Be((float)cards[0].Suit);
        result.Card2Rank.Should().Be((float)cards[1].Rank);
        result.Card2Suit.Should().Be((float)cards[1].Suit);
        result.Card3Rank.Should().Be((float)cards[2].Rank);
        result.Card3Suit.Should().Be((float)cards[2].Suit);
        result.Card4Rank.Should().Be((float)cards[3].Rank);
        result.Card4Suit.Should().Be((float)cards[3].Suit);
        result.Card5Rank.Should().Be((float)cards[4].Rank);
        result.Card5Suit.Should().Be((float)cards[4].Suit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Transform_WithPlayedCards_MapsInConsistentOrder(int playedCardCount)
    {
        var playedCards = CreatePlayedCardsDict(playedCardCount);
        var entity = CreatePlayCardDecisionEntity(playedCards: playedCards);

        var result = _engineer.Transform(entity);

        result.CardsPlayedInTrick.Should().Be(playedCardCount);

        var orderedPlayedCards = playedCards
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => kvp.Value)
            .ToArray();

        if (playedCardCount > 0)
        {
            result.PlayedCard1Rank.Should().Be((float)orderedPlayedCards[0].Rank);
            result.PlayedCard1Suit.Should().Be((float)orderedPlayedCards[0].Suit);
        }

        if (playedCardCount > 1)
        {
            result.PlayedCard2Rank.Should().Be((float)orderedPlayedCards[1].Rank);
            result.PlayedCard2Suit.Should().Be((float)orderedPlayedCards[1].Suit);
        }

        if (playedCardCount > 2)
        {
            result.PlayedCard3Rank.Should().Be((float)orderedPlayedCards[2].Rank);
            result.PlayedCard3Suit.Should().Be((float)orderedPlayedCards[2].Suit);
        }
    }

    [Fact]
    public void Transform_WithNoPlayedCards_UsesZeroSentinels()
    {
        var entity = CreatePlayCardDecisionEntity(playedCards: []);

        var result = _engineer.Transform(entity);

        result.CardsPlayedInTrick.Should().Be(0);
        result.PlayedCard1Rank.Should().Be(0.0f);
        result.PlayedCard1Suit.Should().Be(0.0f);
        result.PlayedCard2Rank.Should().Be(0.0f);
        result.PlayedCard2Suit.Should().Be(0.0f);
        result.PlayedCard3Rank.Should().Be(0.0f);
        result.PlayedCard3Suit.Should().Be(0.0f);
    }

    [Fact]
    public void Transform_WithNullLeadSuit_UsesNegativeOneSentinel()
    {
        var cards = CreateRelativeCards(5);
        var entity = new PlayCardDecisionEntity
        {
            CardsInHandJson = JsonSerializer.Serialize(cards, JsonSerializationOptions.Default),
            PlayedCardsJson = JsonSerializer.Serialize(CreatePlayedCardsDict(1), JsonSerializationOptions.Default),
            ValidCardsToPlayJson = JsonSerializer.Serialize(cards, JsonSerializationOptions.Default),
            ChosenCardJson = JsonSerializer.Serialize(cards[0], JsonSerializationOptions.Default),
            LeadPlayer = _faker.PickRandom<RelativePlayerPosition>(),
            LeadSuit = null,
            WinningTrickPlayer = _faker.PickRandom<RelativePlayerPosition>(),
            TeamScore = (short)_faker.Random.Int(0, 9),
            OpponentScore = (short)_faker.Random.Int(0, 9),
            CallingPlayer = _faker.PickRandom<RelativePlayerPosition>(),
            CallingPlayerGoingAlone = _faker.Random.Bool(),
            RelativeDealPoints = (short)_faker.Random.Int(-2, 4),
            Trick = new TrickEntity { TrickNumber = (byte)_faker.Random.Int(1, 5) },
        };

        var result = _engineer.Transform(entity);

        result.LeadSuit.Should().Be(-1.0f);
    }

    [Fact]
    public void Transform_WithNullWinningTrickPlayer_UsesNegativeOneSentinel()
    {
        var cards = CreateRelativeCards(5);
        var entity = new PlayCardDecisionEntity
        {
            CardsInHandJson = JsonSerializer.Serialize(cards, JsonSerializationOptions.Default),
            PlayedCardsJson = JsonSerializer.Serialize(CreatePlayedCardsDict(1), JsonSerializationOptions.Default),
            ValidCardsToPlayJson = JsonSerializer.Serialize(cards, JsonSerializationOptions.Default),
            ChosenCardJson = JsonSerializer.Serialize(cards[0], JsonSerializationOptions.Default),
            LeadPlayer = _faker.PickRandom<RelativePlayerPosition>(),
            LeadSuit = _faker.PickRandom<RelativeSuit>(),
            WinningTrickPlayer = null,
            TeamScore = (short)_faker.Random.Int(0, 9),
            OpponentScore = (short)_faker.Random.Int(0, 9),
            CallingPlayer = _faker.PickRandom<RelativePlayerPosition>(),
            CallingPlayerGoingAlone = _faker.Random.Bool(),
            RelativeDealPoints = (short)_faker.Random.Int(-2, 4),
            Trick = new TrickEntity { TrickNumber = (byte)_faker.Random.Int(1, 5) },
        };

        var result = _engineer.Transform(entity);

        result.WinningTrickPlayer.Should().Be(-1.0f);
    }

    [Fact]
    public void Transform_WithLeadSuit_MapsCorrectly()
    {
        var entity = CreatePlayCardDecisionEntity(leadSuit: RelativeSuit.Trump);

        var result = _engineer.Transform(entity);

        result.LeadSuit.Should().Be((float)RelativeSuit.Trump);
    }

    [Fact]
    public void Transform_WithWinningTrickPlayer_MapsCorrectly()
    {
        var entity = CreatePlayCardDecisionEntity(winningTrickPlayer: RelativePlayerPosition.Partner);

        var result = _engineer.Transform(entity);

        result.WinningTrickPlayer.Should().Be((float)RelativePlayerPosition.Partner);
    }

    [Fact]
    public void Transform_WithValidCards_SetsCorrectValidityFlags()
    {
        var cards = CreateRelativeCards(5);
        var validCards = new[] { cards[0], cards[2], cards[4] };
        var entity = CreatePlayCardDecisionEntity(cards, validCards: validCards);

        var result = _engineer.Transform(entity);

        result.Card1IsValid.Should().Be(1.0f);
        result.Card2IsValid.Should().Be(0.0f);
        result.Card3IsValid.Should().Be(1.0f);
        result.Card4IsValid.Should().Be(0.0f);
        result.Card5IsValid.Should().Be(1.0f);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Transform_WithChosenCard_SetsCorrectDecisionFlag(int chosenCardIndex)
    {
        var cards = CreateRelativeCards(5);
        var chosenCard = cards[chosenCardIndex];
        var entity = CreatePlayCardDecisionEntity(cards, chosenCard: chosenCard);

        var result = _engineer.Transform(entity);

        result.Card1Chosen.Should().Be(chosenCardIndex == 0 ? 1.0f : 0.0f);
        result.Card2Chosen.Should().Be(chosenCardIndex == 1 ? 1.0f : 0.0f);
        result.Card3Chosen.Should().Be(chosenCardIndex == 2 ? 1.0f : 0.0f);
        result.Card4Chosen.Should().Be(chosenCardIndex == 3 ? 1.0f : 0.0f);
        result.Card5Chosen.Should().Be(chosenCardIndex == 4 ? 1.0f : 0.0f);
    }

    [Fact]
    public void Transform_WithChosenCardNotInHand_ThrowsInvalidOperationException()
    {
        var cards = CreateRelativeCards(5);
        var chosenCard = new RelativeCard
        {
            Rank = Rank.Ace,
            Suit = RelativeSuit.Trump,
        };

        while (cards.Any(c => c.Rank == chosenCard.Rank && c.Suit == chosenCard.Suit))
        {
            chosenCard.Rank = _faker.PickRandom<Rank>();
            chosenCard.Suit = _faker.PickRandom<RelativeSuit>();
        }

        var entity = CreatePlayCardDecisionEntity(cards, chosenCard: chosenCard);

        var act = () => _engineer.Transform(entity);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found in hand*");
    }

    [Fact]
    public void Transform_WithValidEntity_MapsExpectedDealPoints()
    {
        var cards = CreateRelativeCards(5);
        var entity = new PlayCardDecisionEntity
        {
            CardsInHandJson = JsonSerializer.Serialize(cards, JsonSerializationOptions.Default),
            PlayedCardsJson = JsonSerializer.Serialize(CreatePlayedCardsDict(1), JsonSerializationOptions.Default),
            ValidCardsToPlayJson = JsonSerializer.Serialize(cards, JsonSerializationOptions.Default),
            ChosenCardJson = JsonSerializer.Serialize(cards[0], JsonSerializationOptions.Default),
            LeadPlayer = _faker.PickRandom<RelativePlayerPosition>(),
            LeadSuit = _faker.PickRandom<RelativeSuit>(),
            WinningTrickPlayer = _faker.PickRandom<RelativePlayerPosition>(),
            TeamScore = (short)_faker.Random.Int(0, 9),
            OpponentScore = (short)_faker.Random.Int(0, 9),
            CallingPlayer = _faker.PickRandom<RelativePlayerPosition>(),
            CallingPlayerGoingAlone = _faker.Random.Bool(),
            RelativeDealPoints = 2,
            Trick = new TrickEntity { TrickNumber = (byte)_faker.Random.Int(1, 5) },
        };

        var result = _engineer.Transform(entity);

        result.ExpectedDealPoints.Should().Be(2);
    }

    [Fact]
    public void Transform_WithValidEntity_MapsContextFields()
    {
        var entity = CreatePlayCardDecisionEntity(
            leadPlayer: RelativePlayerPosition.LeftHandOpponent,
            teamScore: 4,
            opponentScore: 6,
            trickNumber: 3,
            callingPlayer: RelativePlayerPosition.Partner,
            callingPlayerGoingAlone: true);

        var result = _engineer.Transform(entity);

        result.LeadPlayer.Should().Be((float)RelativePlayerPosition.LeftHandOpponent);
        result.TeamScore.Should().Be(4);
        result.OpponentScore.Should().Be(6);
        result.TrickNumber.Should().Be(3);
        result.CallingPlayerPosition.Should().Be((float)RelativePlayerPosition.Partner);
        result.CallingPlayerGoingAlone.Should().Be(1.0f);
    }

    [Fact]
    public void Transform_WithCallingPlayerNotGoingAlone_MapsGoingAloneToZero()
    {
        var entity = CreatePlayCardDecisionEntity(callingPlayerGoingAlone: false);

        var result = _engineer.Transform(entity);

        result.CallingPlayerGoingAlone.Should().Be(0.0f);
    }

    private RelativeCard CreateRelativeCard(Rank? rank = null, RelativeSuit? suit = null)
    {
        return new RelativeCard
        {
            Rank = rank ?? _faker.PickRandom<Rank>(),
            Suit = suit ?? _faker.PickRandom<RelativeSuit>(),
        };
    }

    private RelativeCard[] CreateRelativeCards(int count)
    {
        var cards = new List<RelativeCard>();
        for (int i = 0; i < count; i++)
        {
            RelativeCard card;
            do
            {
                card = CreateRelativeCard();
            }
            while (cards.Any(c => c.Rank == card.Rank && c.Suit == card.Suit));

            cards.Add(card);
        }

        return [.. cards];
    }

    private Dictionary<RelativePlayerPosition, RelativeCard> CreatePlayedCardsDict(int count)
    {
        var dict = new Dictionary<RelativePlayerPosition, RelativeCard>();
        var positions = new[]
        {
            RelativePlayerPosition.Self,
            RelativePlayerPosition.LeftHandOpponent,
            RelativePlayerPosition.Partner,
        };

        for (int i = 0; i < count && i < positions.Length; i++)
        {
            dict[positions[i]] = CreateRelativeCard();
        }

        return dict;
    }

    private PlayCardDecisionEntity CreatePlayCardDecisionEntity(
        RelativeCard[]? cards = null,
        Dictionary<RelativePlayerPosition, RelativeCard>? playedCards = null,
        RelativeCard[]? validCards = null,
        RelativeCard? chosenCard = null,
        RelativePlayerPosition? leadPlayer = null,
        RelativeSuit? leadSuit = default,
        RelativePlayerPosition? winningTrickPlayer = default,
        short? teamScore = null,
        short? opponentScore = null,
        byte? trickNumber = null,
        RelativePlayerPosition? callingPlayer = null,
        bool? callingPlayerGoingAlone = null)
    {
        cards ??= CreateRelativeCards(5);
        playedCards ??= CreatePlayedCardsDict(1);
        validCards ??= cards;
        chosenCard ??= cards[0];

        return new PlayCardDecisionEntity
        {
            CardsInHandJson = JsonSerializer.Serialize(cards, JsonSerializationOptions.Default),
            PlayedCardsJson = JsonSerializer.Serialize(playedCards, JsonSerializationOptions.Default),
            ValidCardsToPlayJson = JsonSerializer.Serialize(validCards, JsonSerializationOptions.Default),
            ChosenCardJson = JsonSerializer.Serialize(chosenCard, JsonSerializationOptions.Default),
            LeadPlayer = leadPlayer ?? _faker.PickRandom<RelativePlayerPosition>(),
            LeadSuit = leadSuit ?? (_faker.Random.Bool() ? _faker.PickRandom<RelativeSuit>() : null),
            WinningTrickPlayer = winningTrickPlayer,
            TeamScore = teamScore ?? (short)_faker.Random.Int(0, 9),
            OpponentScore = opponentScore ?? (short)_faker.Random.Int(0, 9),
            CallingPlayer = callingPlayer ?? _faker.PickRandom<RelativePlayerPosition>(),
            CallingPlayerGoingAlone = callingPlayerGoingAlone ?? _faker.Random.Bool(),
            RelativeDealPoints = (short)_faker.Random.Int(-2, 4),
            Trick = new TrickEntity
            {
                TrickNumber = trickNumber ?? (byte)_faker.Random.Int(1, 5),
            },
        };
    }
}
