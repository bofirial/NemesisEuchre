using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.MachineLearning.Tests.FeatureEngineering;

public class DiscardCardFeatureBuilderTests
{
    [Fact]
    public void BuildFeatures_Maps6CardHand()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.Ace, RelativeSuit.Trump),
            new(Rank.King, RelativeSuit.Trump),
            new(Rank.Queen, RelativeSuit.NonTrumpSameColor),
            new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor1),
            new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor2),
            new(Rank.Jack, RelativeSuit.NonTrumpSameColor),
        };

        var result = DiscardCardFeatureBuilder.BuildFeatures(
            cards,
            callingPlayer: RelativePlayerPosition.Self,
            callingPlayerGoingAlone: false,
            teamScore: 0,
            opponentScore: 0,
            chosenCard: cards[5]);

        result.Card1Rank.Should().Be((float)Rank.Ace);
        result.Card1Suit.Should().Be((float)RelativeSuit.Trump);
        result.Card2Rank.Should().Be((float)Rank.King);
        result.Card2Suit.Should().Be((float)RelativeSuit.Trump);
        result.Card3Rank.Should().Be((float)Rank.Queen);
        result.Card3Suit.Should().Be((float)RelativeSuit.NonTrumpSameColor);
        result.Card4Rank.Should().Be((float)Rank.Ten);
        result.Card4Suit.Should().Be((float)RelativeSuit.NonTrumpOppositeColor1);
        result.Card5Rank.Should().Be((float)Rank.Nine);
        result.Card5Suit.Should().Be((float)RelativeSuit.NonTrumpOppositeColor2);
        result.Card6Rank.Should().Be((float)Rank.Jack);
        result.Card6Suit.Should().Be((float)RelativeSuit.NonTrumpSameColor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void BuildFeatures_SetsChosenCardProperties(int chosenIndex)
    {
        var cards = CreateDefaultHand();

        var result = DiscardCardFeatureBuilder.BuildFeatures(
            cards,
            callingPlayer: RelativePlayerPosition.Self,
            callingPlayerGoingAlone: false,
            teamScore: 0,
            opponentScore: 0,
            chosenCard: cards[chosenIndex]);

        result.ChosenCardRank.Should().Be((float)cards[chosenIndex].Rank);
        result.ChosenCardRelativeSuit.Should().Be((float)cards[chosenIndex].Suit);
    }

    [Fact]
    public void BuildFeatures_MapsCallingPlayerContext()
    {
        var cards = CreateDefaultHand();

        var result = DiscardCardFeatureBuilder.BuildFeatures(
            cards,
            callingPlayer: RelativePlayerPosition.Partner,
            callingPlayerGoingAlone: true,
            teamScore: 8,
            opponentScore: 6,
            chosenCard: cards[0]);

        result.CallingPlayerPosition.Should().Be((float)RelativePlayerPosition.Partner);
        result.CallingPlayerGoingAlone.Should().Be(1.0f);
        result.TeamScore.Should().Be(8);
        result.OpponentScore.Should().Be(6);
    }

    [Fact]
    public void BuildFeatures_WithCallingPlayerNotGoingAlone_MapsGoingAloneToZero()
    {
        var cards = CreateDefaultHand();

        var result = DiscardCardFeatureBuilder.BuildFeatures(
            cards,
            callingPlayer: RelativePlayerPosition.Self,
            callingPlayerGoingAlone: false,
            teamScore: 0,
            opponentScore: 0,
            chosenCard: cards[0]);

        result.CallingPlayerGoingAlone.Should().Be(0.0f);
    }

    private static RelativeCard[] CreateDefaultHand()
    {
        return
        [
            new(Rank.Ace, RelativeSuit.Trump),
            new(Rank.King, RelativeSuit.Trump),
            new(Rank.Queen, RelativeSuit.NonTrumpSameColor),
            new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor1),
            new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor2),
            new(Rank.Jack, RelativeSuit.NonTrumpSameColor),
        ];
    }
}
