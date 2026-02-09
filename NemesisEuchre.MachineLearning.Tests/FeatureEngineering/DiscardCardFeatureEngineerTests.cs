using Bogus;

using FluentAssertions;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;
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
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Transform_WithChosenCard_SetsCorrectDecisionFlag(int chosenCardIndex)
    {
        var cards = CreateRelativeCards(6);
        var chosenCard = cards[chosenCardIndex];
        var entity = CreateDiscardCardDecisionEntity(cards, chosenCard);

        var result = _engineer.Transform(entity);

        result.Card1Chosen.Should().Be(chosenCardIndex == 0 ? 1.0f : 0.0f);
        result.Card2Chosen.Should().Be(chosenCardIndex == 1 ? 1.0f : 0.0f);
        result.Card3Chosen.Should().Be(chosenCardIndex == 2 ? 1.0f : 0.0f);
        result.Card4Chosen.Should().Be(chosenCardIndex == 3 ? 1.0f : 0.0f);
        result.Card5Chosen.Should().Be(chosenCardIndex == 4 ? 1.0f : 0.0f);
        result.Card6Chosen.Should().Be(chosenCardIndex == 5 ? 1.0f : 0.0f);
    }

    [Fact]
    public void Transform_WithChosenCardNotInHand_ThrowsInvalidOperationException()
    {
        var cards = CreateRelativeCards(6);
        var chosenCard = new RelativeCard(Rank.Ace, RelativeSuit.Trump);

        while (cards.Any(c => c == chosenCard))
        {
            chosenCard = new RelativeCard(_faker.PickRandom<Rank>(), _faker.PickRandom<RelativeSuit>());
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

    [Fact]
    public void Transform_WithValidEntity_MapsExpectedDealPoints()
    {
        var cards = CreateRelativeCards(6);
        var entity = new DiscardCardDecisionEntity
        {
            CardsInHand = [.. cards.Select((c, i) => new DiscardCardDecisionCardsInHand { RelativeCardId = CardIdHelper.ToRelativeCardId(c), SortOrder = i })],
            CallingRelativePlayerPositionId = (int)_faker.PickRandom<RelativePlayerPosition>(),
            CallingPlayerGoingAlone = _faker.Random.Bool(),
            TeamScore = (short)_faker.Random.Int(0, 9),
            OpponentScore = (short)_faker.Random.Int(0, 9),
            ChosenRelativeCardId = CardIdHelper.ToRelativeCardId(cards[0]),
            RelativeDealPoints = -1,
        };

        var result = _engineer.Transform(entity);

        result.ExpectedDealPoints.Should().Be(-1);
    }

    private RelativeCard CreateRelativeCard(Rank? rank = null, RelativeSuit? suit = null)
    {
        return new RelativeCard(rank ?? _faker.PickRandom<Rank>(), suit ?? _faker.PickRandom<RelativeSuit>());
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
            while (cards.Any(c => c == card));

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
            CardsInHand = [.. cards.Select((c, i) => new DiscardCardDecisionCardsInHand { RelativeCardId = CardIdHelper.ToRelativeCardId(c), SortOrder = i })],
            CallingRelativePlayerPositionId = (int)(callingPlayer ?? _faker.PickRandom<RelativePlayerPosition>()),
            CallingPlayerGoingAlone = callingPlayerGoingAlone ?? _faker.Random.Bool(),
            TeamScore = teamScore ?? (short)_faker.Random.Int(0, 9),
            OpponentScore = opponentScore ?? (short)_faker.Random.Int(0, 9),
            ChosenRelativeCardId = CardIdHelper.ToRelativeCardId(chosenCard),
            RelativeDealPoints = (short)_faker.Random.Int(-2, 4),
        };
    }
}
