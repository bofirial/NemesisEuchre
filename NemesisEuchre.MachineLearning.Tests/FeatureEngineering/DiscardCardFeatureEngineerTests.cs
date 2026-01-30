using System.Text.Json;

using Bogus;

using FluentAssertions;

using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.MachineLearning.Tests.FeatureEngineering;

public class DiscardCardFeatureEngineerTests
{
    private readonly DiscardCardFeatureEngineer _engineer;
    private readonly Faker _faker;

    public DiscardCardFeatureEngineerTests()
    {
        _engineer = new DiscardCardFeatureEngineer();
        _faker = new Faker();
    }

    [Fact]
    public void Transform_WithValidEntity_MapsAll6Cards()
    {
        var cards = CreateRelativeCards(6);
        var entity = CreateDiscardCardDecisionEntity(cards);

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
        result.Card6Rank.Should().Be((float)cards[5].Rank);
        result.Card6Suit.Should().Be((float)cards[5].Suit);
    }

    [Fact]
    public void Transform_WithValidEntity_MapsCallingPlayerContext()
    {
        var entity = CreateDiscardCardDecisionEntity(
            callingPlayer: RelativePlayerPosition.LeftHandOpponent,
            callingPlayerGoingAlone: true,
            teamScore: 7,
            opponentScore: 2);

        var result = _engineer.Transform(entity);

        result.CallingPlayerPosition.Should().Be((float)RelativePlayerPosition.LeftHandOpponent);
        result.CallingPlayerGoingAlone.Should().Be(1.0f);
        result.TeamScore.Should().Be(7);
        result.OpponentScore.Should().Be(2);
    }

    [Fact]
    public void Transform_WithCallingPlayerNotGoingAlone_MapsGoingAloneToZero()
    {
        var entity = CreateDiscardCardDecisionEntity(callingPlayerGoingAlone: false);

        var result = _engineer.Transform(entity);

        result.CallingPlayerGoingAlone.Should().Be(0.0f);
    }

    [Theory]
    [InlineData(0, 0u)]
    [InlineData(1, 1u)]
    [InlineData(2, 2u)]
    [InlineData(3, 3u)]
    [InlineData(4, 4u)]
    [InlineData(5, 5u)]
    public void Transform_WithChosenCard_MapsToCorrectIndex(int chosenCardIndex, uint expectedIndex)
    {
        var cards = CreateRelativeCards(6);
        var chosenCard = cards[chosenCardIndex];
        var entity = CreateDiscardCardDecisionEntity(cards, chosenCard);

        var result = _engineer.Transform(entity);

        result.ChosenCardIndex.Should().Be(expectedIndex);
    }

    [Fact]
    public void Transform_WithChosenCardNotInHand_ThrowsInvalidOperationException()
    {
        var cards = CreateRelativeCards(6);
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

        var entity = CreateDiscardCardDecisionEntity(cards, chosenCard);

        var act = () => _engineer.Transform(entity);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found in hand*");
    }

    [Fact]
    public void Transform_WithFewerThan6Cards_ThrowsInvalidOperationException()
    {
        var cards = CreateRelativeCards(5);
        var entity = CreateDiscardCardDecisionEntity(cards);

        var act = () => _engineer.Transform(entity);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Expected 6 cards*");
    }

    [Fact]
    public void Transform_WithMoreThan6Cards_ThrowsInvalidOperationException()
    {
        var cards = CreateRelativeCards(7);
        var entity = CreateDiscardCardDecisionEntity(cards);

        var act = () => _engineer.Transform(entity);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Expected 6 cards*");
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

    private DiscardCardDecisionEntity CreateDiscardCardDecisionEntity(
        RelativeCard[]? cards = null,
        RelativeCard? chosenCard = null,
        RelativePlayerPosition? callingPlayer = null,
        bool? callingPlayerGoingAlone = null,
        short? teamScore = null,
        short? opponentScore = null)
    {
        cards ??= CreateRelativeCards(6);
        chosenCard ??= cards[0];

        return new DiscardCardDecisionEntity
        {
            CardsInHandJson = JsonSerializer.Serialize(cards, JsonSerializationOptions.Default),
            CallingPlayer = callingPlayer ?? _faker.PickRandom<RelativePlayerPosition>(),
            CallingPlayerGoingAlone = callingPlayerGoingAlone ?? _faker.Random.Bool(),
            TeamScore = teamScore ?? (short)_faker.Random.Int(0, 9),
            OpponentScore = opponentScore ?? (short)_faker.Random.Int(0, 9),
            ChosenCardJson = JsonSerializer.Serialize(chosenCard, JsonSerializationOptions.Default),
        };
    }
}
