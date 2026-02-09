using System.Text.Json;

using Bogus;

using FluentAssertions;

using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.DataAccess.Entities;
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
        _engineer = new CallTrumpFeatureEngineer();
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
        result.DecisionOrder.Should().Be(2);
    }

    [Theory]
    [InlineData(new[] { CallTrumpDecision.Pass, CallTrumpDecision.CallSpades })]
    [InlineData(new[] { CallTrumpDecision.OrderItUp, CallTrumpDecision.OrderItUpAndGoAlone })]
    [InlineData(new[] { CallTrumpDecision.Pass, CallTrumpDecision.CallSpades, CallTrumpDecision.CallHearts, CallTrumpDecision.CallClubs, CallTrumpDecision.CallDiamonds })]
    public void Transform_WithValidDecisions_SetsCorrectValidityFlags(CallTrumpDecision[] validDecisions)
    {
        var chosenDecision = validDecisions[0];
        var entity = CreateCallTrumpDecisionEntity(
            validDecisions: validDecisions,
            chosenDecision: chosenDecision);

        var result = _engineer.Transform(entity);

        var validityFlags = new[]
        {
            result.Decision0IsValid,
            result.Decision1IsValid,
            result.Decision2IsValid,
            result.Decision3IsValid,
            result.Decision4IsValid,
            result.Decision5IsValid,
            result.Decision6IsValid,
            result.Decision7IsValid,
            result.Decision8IsValid,
            result.Decision9IsValid,
            result.Decision10IsValid,
        };

        for (int i = 0; i < 11; i++)
        {
            var expectedFlag = validDecisions.Contains((CallTrumpDecision)i) ? 1.0f : 0.0f;
            validityFlags[i].Should().Be(expectedFlag, $"Decision{i}IsValid should be {expectedFlag}");
        }
    }

    [Theory]
    [InlineData(CallTrumpDecision.Pass)]
    [InlineData(CallTrumpDecision.CallSpades)]
    [InlineData(CallTrumpDecision.CallHearts)]
    [InlineData(CallTrumpDecision.CallClubs)]
    [InlineData(CallTrumpDecision.CallDiamonds)]
    [InlineData(CallTrumpDecision.CallSpadesAndGoAlone)]
    [InlineData(CallTrumpDecision.OrderItUp)]
    [InlineData(CallTrumpDecision.OrderItUpAndGoAlone)]
    public void Transform_WithChosenDecision_SetsCorrectDecisionFlag(CallTrumpDecision chosenDecision)
    {
        var entity = CreateCallTrumpDecisionEntity(
            validDecisions: [chosenDecision],
            chosenDecision: chosenDecision);

        var result = _engineer.Transform(entity);

        result.Decision0Chosen.Should().Be(chosenDecision == CallTrumpDecision.Pass ? 1.0f : 0.0f);
        result.Decision1Chosen.Should().Be(chosenDecision == CallTrumpDecision.CallSpades ? 1.0f : 0.0f);
        result.Decision2Chosen.Should().Be(chosenDecision == CallTrumpDecision.CallHearts ? 1.0f : 0.0f);
        result.Decision3Chosen.Should().Be(chosenDecision == CallTrumpDecision.CallClubs ? 1.0f : 0.0f);
        result.Decision4Chosen.Should().Be(chosenDecision == CallTrumpDecision.CallDiamonds ? 1.0f : 0.0f);
        result.Decision5Chosen.Should().Be(chosenDecision == CallTrumpDecision.CallSpadesAndGoAlone ? 1.0f : 0.0f);
        result.Decision6Chosen.Should().Be(chosenDecision == CallTrumpDecision.CallHeartsAndGoAlone ? 1.0f : 0.0f);
        result.Decision7Chosen.Should().Be(chosenDecision == CallTrumpDecision.CallClubsAndGoAlone ? 1.0f : 0.0f);
        result.Decision8Chosen.Should().Be(chosenDecision == CallTrumpDecision.CallDiamondsAndGoAlone ? 1.0f : 0.0f);
        result.Decision9Chosen.Should().Be(chosenDecision == CallTrumpDecision.OrderItUp ? 1.0f : 0.0f);
        result.Decision10Chosen.Should().Be(chosenDecision == CallTrumpDecision.OrderItUpAndGoAlone ? 1.0f : 0.0f);
    }

    [Fact]
    public void Transform_WithChosenDecisionNotInValidSet_ThrowsInvalidOperationException()
    {
        var entity = CreateCallTrumpDecisionEntity(
            validDecisions: [CallTrumpDecision.Pass, CallTrumpDecision.CallSpades],
            chosenDecision: CallTrumpDecision.OrderItUp);

        var act = () => _engineer.Transform(entity);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not in the valid decisions array*");
    }

    [Fact]
    public void Transform_WithValidEntity_MapsExpectedDealPoints()
    {
        var cards = CreateCards(5);
        var upCard = CreateCard();
        var entity = new CallTrumpDecisionEntity
        {
            CardsInHandJson = JsonSerializer.Serialize(cards, JsonSerializationOptions.Default),
            UpCardJson = JsonSerializer.Serialize(upCard, JsonSerializationOptions.Default),
            DealerPosition = _faker.PickRandom<RelativePlayerPosition>(),
            TeamScore = (short)_faker.Random.Int(0, 9),
            OpponentScore = (short)_faker.Random.Int(0, 9),
            DecisionOrder = (byte)_faker.Random.Int(0, 7),
            ValidDecisionsJson = JsonSerializer.Serialize(new[] { CallTrumpDecision.Pass }, JsonSerializationOptions.Default),
            ChosenDecisionJson = JsonSerializer.Serialize(CallTrumpDecision.Pass, JsonSerializationOptions.Default),
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
            CardsInHandJson = JsonSerializer.Serialize(cards, JsonSerializationOptions.Default),
            UpCardJson = JsonSerializer.Serialize(upCard, JsonSerializationOptions.Default),
            DealerPosition = dealerPosition ?? _faker.PickRandom<RelativePlayerPosition>(),
            TeamScore = teamScore ?? (short)_faker.Random.Int(0, 9),
            OpponentScore = opponentScore ?? (short)_faker.Random.Int(0, 9),
            DecisionOrder = decisionOrder ?? (byte)_faker.Random.Int(0, 7),
            ValidDecisionsJson = JsonSerializer.Serialize(validDecisions, JsonSerializationOptions.Default),
            ChosenDecisionJson = JsonSerializer.Serialize(chosenDecision, JsonSerializationOptions.Default),
            RelativeDealPoints = (short)_faker.Random.Int(-2, 4),
        };
    }
}
