using Bogus;

using FluentAssertions;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.MachineLearning.Tests.FeatureEngineering;

public class CallTrumpFeatureEngineerTests
{
    private readonly CallTrumpFeatureEngineer _engineer;
    private readonly Faker _faker;

    public CallTrumpFeatureEngineerTests()
    {
        var builder = new CallTrumpFeatureBuilder();
        _engineer = new CallTrumpFeatureEngineer(builder);
        _faker = new Faker();
    }

    [Fact]
    public void Transform_WithValidEntity_MapsAllHandCards()
    {
        var cards = CreateCards(5);
        var entity = CreateCallTrumpDecisionEntity(cards);

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

    [Fact]
    public void Transform_WithValidEntity_MapsUpCard()
    {
        var upCard = CreateCard();
        var entity = CreateCallTrumpDecisionEntity(upCard: upCard);

        var result = _engineer.Transform(entity);

        result.UpCardRank.Should().Be((float)upCard.Rank);
        result.UpCardSuit.Should().Be((float)upCard.Suit);
    }

    [Fact]
    public void Transform_WithValidEntity_MapsContextFields()
    {
        var entity = CreateCallTrumpDecisionEntity(
            dealerPosition: RelativePlayerPosition.Partner,
            teamScore: 5,
            opponentScore: 3,
            decisionOrder: 2);

        var result = _engineer.Transform(entity);

        result.DealerPosition.Should().Be((float)RelativePlayerPosition.Partner);
        result.TeamScore.Should().Be(5);
        result.OpponentScore.Should().Be(3);
        result.DecisionNumber.Should().Be(2);
    }

    [Theory]
    [InlineData(CallTrumpDecision.Pass, 0)]
    [InlineData(CallTrumpDecision.CallSpades, 1)]
    [InlineData(CallTrumpDecision.CallHearts, 2)]
    [InlineData(CallTrumpDecision.CallClubs, 3)]
    [InlineData(CallTrumpDecision.CallDiamonds, 4)]
    [InlineData(CallTrumpDecision.CallSpadesAndGoAlone, 5)]
    [InlineData(CallTrumpDecision.CallHeartsAndGoAlone, 6)]
    [InlineData(CallTrumpDecision.CallClubsAndGoAlone, 7)]
    [InlineData(CallTrumpDecision.CallDiamondsAndGoAlone, 8)]
    [InlineData(CallTrumpDecision.OrderItUp, 9)]
    [InlineData(CallTrumpDecision.OrderItUpAndGoAlone, 10)]
    public void Transform_WithChosenDecision_SetsChosenDecisionValue(CallTrumpDecision chosenDecision, int expectedValue)
    {
        var entity = CreateCallTrumpDecisionEntity(
            validDecisions: [chosenDecision],
            chosenDecision: chosenDecision);

        var result = _engineer.Transform(entity);

        result.ChosenDecision.Should().Be(expectedValue);
    }

    [Fact]
    public void Transform_WithValidEntity_MapsExpectedDealPoints()
    {
        var cards = CreateCards(5);
        var upCard = CreateCard();
        var entity = new CallTrumpDecisionEntity
        {
            CardsInHand = [.. cards.Select((c, i) => new CallTrumpDecisionCardsInHand { CardId = CardIdHelper.ToCardId(c), SortOrder = i })],
            UpCardId = CardIdHelper.ToCardId(upCard),
            DealerRelativePositionId = (int)_faker.PickRandom<RelativePlayerPosition>(),
            TeamScore = (short)_faker.Random.Int(0, 9),
            OpponentScore = (short)_faker.Random.Int(0, 9),
            DecisionOrder = (byte)_faker.Random.Int(0, 7),
            ValidDecisions = [new CallTrumpDecisionValidDecision { CallTrumpDecisionValueId = (int)CallTrumpDecision.Pass }],
            ChosenDecisionValueId = (int)CallTrumpDecision.Pass,
            RelativeDealPoints = 4,
        };

        var result = _engineer.Transform(entity);

        result.ExpectedDealPoints.Should().Be(4);
    }

    private Card CreateCard(Rank? rank = null, Suit? suit = null)
    {
        return new Card(suit ?? _faker.PickRandom<Suit>(), rank ?? _faker.PickRandom<Rank>());
    }

    private Card[] CreateCards(int count)
    {
        return [.. Enumerable.Range(0, count).Select(_ => CreateCard())];
    }

    private CallTrumpDecisionEntity CreateCallTrumpDecisionEntity(
        Card[]? cards = null,
        Card? upCard = null,
        RelativePlayerPosition? dealerPosition = null,
        short? teamScore = null,
        short? opponentScore = null,
        byte? decisionOrder = null,
        CallTrumpDecision[]? validDecisions = null,
        CallTrumpDecision? chosenDecision = null)
    {
        cards ??= CreateCards(5);
        upCard ??= CreateCard();
        validDecisions ??= [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp];
        chosenDecision ??= validDecisions[0];

        return new CallTrumpDecisionEntity
        {
            CardsInHand = [.. cards.Select((c, i) => new CallTrumpDecisionCardsInHand { CardId = CardIdHelper.ToCardId(c), SortOrder = i })],
            UpCardId = CardIdHelper.ToCardId(upCard),
            DealerRelativePositionId = (int)(dealerPosition ?? _faker.PickRandom<RelativePlayerPosition>()),
            TeamScore = teamScore ?? (short)_faker.Random.Int(0, 9),
            OpponentScore = opponentScore ?? (short)_faker.Random.Int(0, 9),
            DecisionOrder = decisionOrder ?? (byte)_faker.Random.Int(0, 7),
            ValidDecisions = [.. validDecisions.Select(d => new CallTrumpDecisionValidDecision { CallTrumpDecisionValueId = (int)d })],
            ChosenDecisionValueId = (int)chosenDecision,
            RelativeDealPoints = (short)_faker.Random.Int(-2, 4),
        };
    }
}
