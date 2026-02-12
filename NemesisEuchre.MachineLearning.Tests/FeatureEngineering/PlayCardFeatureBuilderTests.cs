using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.MachineLearning.Tests.FeatureEngineering;

public class PlayCardFeatureBuilderTests
{
    [Fact]
    public void BuildFeatures_WithFullHand_MapsAllCardRanksAndSuits()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.Ace, RelativeSuit.Trump),
            new(Rank.King, RelativeSuit.NonTrumpSameColor),
            new(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1),
            new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor2),
            new(Rank.Nine, RelativeSuit.Trump),
        };

        var result = PlayCardFeatureBuilder.BuildFeatures(
            cards,
            validCards: cards,
            playedCards: [],
            teamScore: 3,
            opponentScore: 5,
            leadPlayer: RelativePlayerPosition.Self,
            leadSuit: RelativeSuit.Trump,
            callingPlayer: RelativePlayerPosition.Self,
            callingPlayerGoingAlone: false,
            dealer: RelativePlayerPosition.Partner,
            dealerPickedUpCard: null,
            knownPlayerSuitVoids: [],
            cardsAccountedFor: [],
            winningTrickPlayer: null,
            trickNumber: 1,
            chosenCard: cards[0]);

        result.Card1Rank.Should().Be((float)Rank.Ace);
        result.Card1Suit.Should().Be((float)RelativeSuit.Trump);
        result.Card2Rank.Should().Be((float)Rank.King);
        result.Card2Suit.Should().Be((float)RelativeSuit.NonTrumpSameColor);
        result.Card3Rank.Should().Be((float)Rank.Queen);
        result.Card3Suit.Should().Be((float)RelativeSuit.NonTrumpOppositeColor1);
        result.Card4Rank.Should().Be((float)Rank.Ten);
        result.Card4Suit.Should().Be((float)RelativeSuit.NonTrumpOppositeColor2);
        result.Card5Rank.Should().Be((float)Rank.Nine);
        result.Card5Suit.Should().Be((float)RelativeSuit.Trump);
    }

    [Fact]
    public void BuildFeatures_WithFewerThan5Cards_UsesSentinelValues()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.Ace, RelativeSuit.Trump),
            new(Rank.King, RelativeSuit.NonTrumpSameColor),
            new(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1),
        };

        var result = PlayCardFeatureBuilder.BuildFeatures(
            cards,
            validCards: cards,
            playedCards: [],
            teamScore: 0,
            opponentScore: 0,
            leadPlayer: RelativePlayerPosition.Self,
            leadSuit: null,
            callingPlayer: RelativePlayerPosition.Self,
            callingPlayerGoingAlone: false,
            dealer: RelativePlayerPosition.Self,
            dealerPickedUpCard: null,
            knownPlayerSuitVoids: [],
            cardsAccountedFor: [],
            winningTrickPlayer: null,
            trickNumber: 1,
            chosenCard: cards[0]);

        result.Card1Rank.Should().Be((float)Rank.Ace);
        result.Card2Rank.Should().Be((float)Rank.King);
        result.Card3Rank.Should().Be((float)Rank.Queen);
        result.Card4Rank.Should().Be(-1.0f);
        result.Card4Suit.Should().Be(-1.0f);
        result.Card5Rank.Should().Be(-1.0f);
        result.Card5Suit.Should().Be(-1.0f);
    }

    [Fact]
    public void BuildFeatures_SetsValidityFlagsCorrectly()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.Ace, RelativeSuit.Trump),
            new(Rank.King, RelativeSuit.NonTrumpSameColor),
            new(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1),
            new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor2),
            new(Rank.Nine, RelativeSuit.Trump),
        };
        var validCards = new[] { cards[0], cards[2], cards[4] };

        var result = PlayCardFeatureBuilder.BuildFeatures(
            cards,
            validCards: validCards,
            playedCards: [],
            teamScore: 0,
            opponentScore: 0,
            leadPlayer: RelativePlayerPosition.Self,
            leadSuit: RelativeSuit.Trump,
            callingPlayer: RelativePlayerPosition.Self,
            callingPlayerGoingAlone: false,
            dealer: RelativePlayerPosition.Self,
            dealerPickedUpCard: null,
            knownPlayerSuitVoids: [],
            cardsAccountedFor: [],
            winningTrickPlayer: null,
            trickNumber: 1,
            chosenCard: cards[0]);

        result.Card1IsValid.Should().Be(1.0f);
        result.Card2IsValid.Should().Be(0.0f);
        result.Card3IsValid.Should().Be(1.0f);
        result.Card4IsValid.Should().Be(0.0f);
        result.Card5IsValid.Should().Be(1.0f);
    }

    [Fact]
    public void BuildFeatures_MapsPlayedCardsForEachPosition()
    {
        var cards = CreateDefaultHand();
        var lhoCard = new RelativeCard(Rank.Jack, RelativeSuit.NonTrumpSameColor);
        var partnerCard = new RelativeCard(Rank.Ten, RelativeSuit.NonTrumpOppositeColor1);
        var rhoCard = new RelativeCard(Rank.Nine, RelativeSuit.NonTrumpOppositeColor2);

        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>
        {
            [RelativePlayerPosition.LeftHandOpponent] = lhoCard,
            [RelativePlayerPosition.Partner] = partnerCard,
            [RelativePlayerPosition.RightHandOpponent] = rhoCard,
        };

        var result = PlayCardFeatureBuilder.BuildFeatures(
            cards,
            validCards: cards,
            playedCards: playedCards,
            teamScore: 0,
            opponentScore: 0,
            leadPlayer: RelativePlayerPosition.LeftHandOpponent,
            leadSuit: RelativeSuit.NonTrumpSameColor,
            callingPlayer: RelativePlayerPosition.Self,
            callingPlayerGoingAlone: false,
            dealer: RelativePlayerPosition.Self,
            dealerPickedUpCard: null,
            knownPlayerSuitVoids: [],
            cardsAccountedFor: [],
            winningTrickPlayer: RelativePlayerPosition.Partner,
            trickNumber: 2,
            chosenCard: cards[0]);

        result.LeftHandOpponentPlayedCardRank.Should().Be((float)Rank.Jack);
        result.LeftHandOpponentPlayedCardSuit.Should().Be((float)RelativeSuit.NonTrumpSameColor);
        result.PartnerPlayedCardRank.Should().Be((float)Rank.Ten);
        result.PartnerPlayedCardSuit.Should().Be((float)RelativeSuit.NonTrumpOppositeColor1);
        result.RightHandOpponentPlayedCardRank.Should().Be((float)Rank.Nine);
        result.RightHandOpponentPlayedCardSuit.Should().Be((float)RelativeSuit.NonTrumpOppositeColor2);
    }

    [Fact]
    public void BuildFeatures_WithNoPlayedCards_UsesSentinelsForAllPositions()
    {
        var cards = CreateDefaultHand();

        var result = PlayCardFeatureBuilder.BuildFeatures(
            cards,
            validCards: cards,
            playedCards: [],
            teamScore: 0,
            opponentScore: 0,
            leadPlayer: RelativePlayerPosition.Self,
            leadSuit: null,
            callingPlayer: RelativePlayerPosition.Self,
            callingPlayerGoingAlone: false,
            dealer: RelativePlayerPosition.Self,
            dealerPickedUpCard: null,
            knownPlayerSuitVoids: [],
            cardsAccountedFor: [],
            winningTrickPlayer: null,
            trickNumber: 1,
            chosenCard: cards[0]);

        result.LeftHandOpponentPlayedCardRank.Should().Be(-1.0f);
        result.LeftHandOpponentPlayedCardSuit.Should().Be(-1.0f);
        result.PartnerPlayedCardRank.Should().Be(-1.0f);
        result.PartnerPlayedCardSuit.Should().Be(-1.0f);
        result.RightHandOpponentPlayedCardRank.Should().Be(-1.0f);
        result.RightHandOpponentPlayedCardSuit.Should().Be(-1.0f);
    }

    [Fact]
    public void BuildFeatures_SetsCardsAccountedForFlags()
    {
        var cards = CreateDefaultHand();
        var cardsAccountedFor = new RelativeCard[]
        {
            new(Rank.RightBower, RelativeSuit.Trump),
            new(Rank.Ace, RelativeSuit.NonTrumpSameColor),
            new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor2),
        };

        var result = PlayCardFeatureBuilder.BuildFeatures(
            cards,
            validCards: cards,
            playedCards: [],
            teamScore: 0,
            opponentScore: 0,
            leadPlayer: RelativePlayerPosition.Self,
            leadSuit: null,
            callingPlayer: RelativePlayerPosition.Self,
            callingPlayerGoingAlone: false,
            dealer: RelativePlayerPosition.Self,
            dealerPickedUpCard: null,
            knownPlayerSuitVoids: [],
            cardsAccountedFor: cardsAccountedFor,
            winningTrickPlayer: null,
            trickNumber: 1,
            chosenCard: cards[0]);

        result.RightBowerOfTrumpHasBeenAccountedFor.Should().Be(1.0f);
        result.LeftBowerOfTrumpHasBeenAccountedFor.Should().Be(0.0f);
        result.AceOfTrumpHasBeenAccountedFor.Should().Be(0.0f);
        result.AceOfNonTrumpSameColorHasBeenAccountedFor.Should().Be(1.0f);
        result.NineOfNonTrumpOppositeColor2HasBeenAccountedFor.Should().Be(1.0f);
        result.KingOfNonTrumpOppositeColor2HasBeenAccountedFor.Should().Be(0.0f);
    }

    [Fact]
    public void BuildFeatures_SetsKnownPlayerSuitVoidFlags()
    {
        var cards = CreateDefaultHand();
        var voids = new RelativePlayerSuitVoid[]
        {
            new() { PlayerPosition = RelativePlayerPosition.LeftHandOpponent, Suit = RelativeSuit.Trump },
            new() { PlayerPosition = RelativePlayerPosition.Partner, Suit = RelativeSuit.NonTrumpSameColor },
            new() { PlayerPosition = RelativePlayerPosition.RightHandOpponent, Suit = RelativeSuit.NonTrumpOppositeColor1 },
        };

        var result = PlayCardFeatureBuilder.BuildFeatures(
            cards,
            validCards: cards,
            playedCards: [],
            teamScore: 0,
            opponentScore: 0,
            leadPlayer: RelativePlayerPosition.Self,
            leadSuit: null,
            callingPlayer: RelativePlayerPosition.Self,
            callingPlayerGoingAlone: false,
            dealer: RelativePlayerPosition.Self,
            dealerPickedUpCard: null,
            knownPlayerSuitVoids: voids,
            cardsAccountedFor: [],
            winningTrickPlayer: null,
            trickNumber: 1,
            chosenCard: cards[0]);

        result.LeftHandOpponentMayHaveTrump.Should().Be(0.0f);
        result.LeftHandOpponentMayHaveNonTrumpSameColor.Should().Be(1.0f);
        result.PartnerMayHaveTrump.Should().Be(1.0f);
        result.PartnerMayHaveNonTrumpSameColor.Should().Be(0.0f);
        result.RightHandOpponentMayHaveNonTrumpOppositeColor1.Should().Be(0.0f);
        result.RightHandOpponentMayHaveTrump.Should().Be(1.0f);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void BuildFeatures_SetsChosenCardIndex(int chosenIndex)
    {
        var cards = CreateDefaultHand();

        var result = PlayCardFeatureBuilder.BuildFeatures(
            cards,
            validCards: cards,
            playedCards: [],
            teamScore: 0,
            opponentScore: 0,
            leadPlayer: RelativePlayerPosition.Self,
            leadSuit: null,
            callingPlayer: RelativePlayerPosition.Self,
            callingPlayerGoingAlone: false,
            dealer: RelativePlayerPosition.Self,
            dealerPickedUpCard: null,
            knownPlayerSuitVoids: [],
            cardsAccountedFor: [],
            winningTrickPlayer: null,
            trickNumber: 1,
            chosenCard: cards[chosenIndex]);

        result.Card1Chosen.Should().Be(chosenIndex == 0 ? 1.0f : 0.0f);
        result.Card2Chosen.Should().Be(chosenIndex == 1 ? 1.0f : 0.0f);
        result.Card3Chosen.Should().Be(chosenIndex == 2 ? 1.0f : 0.0f);
        result.Card4Chosen.Should().Be(chosenIndex == 3 ? 1.0f : 0.0f);
        result.Card5Chosen.Should().Be(chosenIndex == 4 ? 1.0f : 0.0f);
    }

    [Fact]
    public void BuildFeatures_MapsContextFields()
    {
        var cards = CreateDefaultHand();
        var dealerPickedUpCard = new RelativeCard(Rank.King, RelativeSuit.Trump);

        var result = PlayCardFeatureBuilder.BuildFeatures(
            cards,
            validCards: cards,
            playedCards: [],
            teamScore: 4,
            opponentScore: 7,
            leadPlayer: RelativePlayerPosition.LeftHandOpponent,
            leadSuit: RelativeSuit.NonTrumpSameColor,
            callingPlayer: RelativePlayerPosition.Partner,
            callingPlayerGoingAlone: true,
            dealer: RelativePlayerPosition.RightHandOpponent,
            dealerPickedUpCard: dealerPickedUpCard,
            knownPlayerSuitVoids: [],
            cardsAccountedFor: [],
            winningTrickPlayer: RelativePlayerPosition.Partner,
            trickNumber: 3,
            chosenCard: cards[0]);

        result.TeamScore.Should().Be(4);
        result.OpponentScore.Should().Be(7);
        result.LeadPlayer.Should().Be((float)RelativePlayerPosition.LeftHandOpponent);
        result.LeadSuit.Should().Be((float)RelativeSuit.NonTrumpSameColor);
        result.CallingPlayerPosition.Should().Be((float)RelativePlayerPosition.Partner);
        result.CallingPlayerGoingAlone.Should().Be(1.0f);
        result.DealerPlayerPosition.Should().Be((float)RelativePlayerPosition.RightHandOpponent);
        result.DealerPickedUpCardRank.Should().Be((float)Rank.King);
        result.DealerPickedUpCardSuit.Should().Be((float)RelativeSuit.Trump);
        result.WinningTrickPlayer.Should().Be((float)RelativePlayerPosition.Partner);
        result.TrickNumber.Should().Be(3);
    }

    [Fact]
    public void BuildFeatures_WithNullOptionalFields_UsesSentinels()
    {
        var cards = CreateDefaultHand();

        var result = PlayCardFeatureBuilder.BuildFeatures(
            cards,
            validCards: cards,
            playedCards: [],
            teamScore: 0,
            opponentScore: 0,
            leadPlayer: RelativePlayerPosition.Self,
            leadSuit: null,
            callingPlayer: RelativePlayerPosition.Self,
            callingPlayerGoingAlone: false,
            dealer: RelativePlayerPosition.Self,
            dealerPickedUpCard: null,
            knownPlayerSuitVoids: [],
            cardsAccountedFor: [],
            winningTrickPlayer: null,
            trickNumber: 1,
            chosenCard: cards[0]);

        result.LeadSuit.Should().Be(-1.0f);
        result.WinningTrickPlayer.Should().Be(-1.0f);
        result.DealerPickedUpCardRank.Should().Be(-1.0f);
        result.DealerPickedUpCardSuit.Should().Be(-1.0f);
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
        ];
    }
}
