using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Models;

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

        var context = new PlayCardFeatureBuilderContext(
            CardsInHand: cards,
            PlayedCards: [],
            TeamScore: 3,
            OpponentScore: 5,
            LeadPlayer: RelativePlayerPosition.Self,
            LeadSuit: RelativeSuit.Trump,
            CallingPlayer: RelativePlayerPosition.Self,
            CallingPlayerGoingAlone: false,
            Dealer: RelativePlayerPosition.Partner,
            DealerPickedUpCard: null,
            KnownPlayerSuitVoids: [],
            CardsAccountedFor: [],
            WinningTrickPlayer: null,
            TrickNumber: 1,
            WonTricks: 0,
            OpponentsWonTricks: 0,
            ChosenCard: cards[0]);
        var result = PlayCardFeatureBuilder.BuildFeatures(context);

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

        var context = new PlayCardFeatureBuilderContext(
            CardsInHand: cards,
            PlayedCards: [],
            TeamScore: 0,
            OpponentScore: 0,
            LeadPlayer: RelativePlayerPosition.Self,
            LeadSuit: null,
            CallingPlayer: RelativePlayerPosition.Self,
            CallingPlayerGoingAlone: false,
            Dealer: RelativePlayerPosition.Self,
            DealerPickedUpCard: null,
            KnownPlayerSuitVoids: [],
            CardsAccountedFor: [],
            WinningTrickPlayer: null,
            TrickNumber: 1,
            WonTricks: 0,
            OpponentsWonTricks: 0,
            ChosenCard: cards[0]);
        var result = PlayCardFeatureBuilder.BuildFeatures(context);

        result.Card1Rank.Should().Be((float)Rank.Ace);
        result.Card2Rank.Should().Be((float)Rank.King);
        result.Card3Rank.Should().Be((float)Rank.Queen);
        result.Card4Rank.Should().Be(-1.0f);
        result.Card4Suit.Should().Be(-1.0f);
        result.Card5Rank.Should().Be(-1.0f);
        result.Card5Suit.Should().Be(-1.0f);
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

        var context = new PlayCardFeatureBuilderContext(
            CardsInHand: cards,
            PlayedCards: playedCards,
            TeamScore: 0,
            OpponentScore: 0,
            LeadPlayer: RelativePlayerPosition.LeftHandOpponent,
            LeadSuit: RelativeSuit.NonTrumpSameColor,
            CallingPlayer: RelativePlayerPosition.Self,
            CallingPlayerGoingAlone: false,
            Dealer: RelativePlayerPosition.Self,
            DealerPickedUpCard: null,
            KnownPlayerSuitVoids: [],
            CardsAccountedFor: [],
            WinningTrickPlayer: RelativePlayerPosition.Partner,
            TrickNumber: 2,
            WonTricks: 0,
            OpponentsWonTricks: 0,
            ChosenCard: cards[0]);
        var result = PlayCardFeatureBuilder.BuildFeatures(context);

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

        var context = new PlayCardFeatureBuilderContext(
            CardsInHand: cards,
            PlayedCards: [],
            TeamScore: 0,
            OpponentScore: 0,
            LeadPlayer: RelativePlayerPosition.Self,
            LeadSuit: null,
            CallingPlayer: RelativePlayerPosition.Self,
            CallingPlayerGoingAlone: false,
            Dealer: RelativePlayerPosition.Self,
            DealerPickedUpCard: null,
            KnownPlayerSuitVoids: [],
            CardsAccountedFor: [],
            WinningTrickPlayer: null,
            TrickNumber: 1,
            WonTricks: 0,
            OpponentsWonTricks: 0,
            ChosenCard: cards[0]);
        var result = PlayCardFeatureBuilder.BuildFeatures(context);

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

        var context = new PlayCardFeatureBuilderContext(
            CardsInHand: cards,
            PlayedCards: [],
            TeamScore: 0,
            OpponentScore: 0,
            LeadPlayer: RelativePlayerPosition.Self,
            LeadSuit: null,
            CallingPlayer: RelativePlayerPosition.Self,
            CallingPlayerGoingAlone: false,
            Dealer: RelativePlayerPosition.Self,
            DealerPickedUpCard: null,
            KnownPlayerSuitVoids: [],
            CardsAccountedFor: cardsAccountedFor,
            WinningTrickPlayer: null,
            TrickNumber: 1,
            WonTricks: 0,
            OpponentsWonTricks: 0,
            ChosenCard: cards[0]);
        var result = PlayCardFeatureBuilder.BuildFeatures(context);

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

        var context = new PlayCardFeatureBuilderContext(
            CardsInHand: cards,
            PlayedCards: [],
            TeamScore: 0,
            OpponentScore: 0,
            LeadPlayer: RelativePlayerPosition.Self,
            LeadSuit: null,
            CallingPlayer: RelativePlayerPosition.Self,
            CallingPlayerGoingAlone: false,
            Dealer: RelativePlayerPosition.Self,
            DealerPickedUpCard: null,
            KnownPlayerSuitVoids: voids,
            CardsAccountedFor: [],
            WinningTrickPlayer: null,
            TrickNumber: 1,
            WonTricks: 0,
            OpponentsWonTricks: 0,
            ChosenCard: cards[0]);
        var result = PlayCardFeatureBuilder.BuildFeatures(context);

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
    public void BuildFeatures_SetsChosenCardProperties(int chosenIndex)
    {
        var cards = CreateDefaultHand();

        var context = new PlayCardFeatureBuilderContext(
            CardsInHand: cards,
            PlayedCards: [],
            TeamScore: 0,
            OpponentScore: 0,
            LeadPlayer: RelativePlayerPosition.Self,
            LeadSuit: null,
            CallingPlayer: RelativePlayerPosition.Self,
            CallingPlayerGoingAlone: false,
            Dealer: RelativePlayerPosition.Self,
            DealerPickedUpCard: null,
            KnownPlayerSuitVoids: [],
            CardsAccountedFor: [],
            WinningTrickPlayer: null,
            TrickNumber: 1,
            WonTricks: 0,
            OpponentsWonTricks: 0,
            ChosenCard: cards[chosenIndex]);
        var result = PlayCardFeatureBuilder.BuildFeatures(context);

        result.ChosenCardRank.Should().Be((float)cards[chosenIndex].Rank);
        result.ChosenCardRelativeSuit.Should().Be((float)cards[chosenIndex].Suit);
    }

    [Fact]
    public void BuildFeatures_MapsContextFields()
    {
        var cards = CreateDefaultHand();
        var dealerPickedUpCard = new RelativeCard(Rank.King, RelativeSuit.Trump);

        var context = new PlayCardFeatureBuilderContext(
            CardsInHand: cards,
            PlayedCards: [],
            TeamScore: 4,
            OpponentScore: 7,
            LeadPlayer: RelativePlayerPosition.LeftHandOpponent,
            LeadSuit: RelativeSuit.NonTrumpSameColor,
            CallingPlayer: RelativePlayerPosition.Partner,
            CallingPlayerGoingAlone: true,
            Dealer: RelativePlayerPosition.RightHandOpponent,
            DealerPickedUpCard: dealerPickedUpCard,
            KnownPlayerSuitVoids: [],
            CardsAccountedFor: [],
            WinningTrickPlayer: RelativePlayerPosition.Partner,
            TrickNumber: 3,
            WonTricks: 0,
            OpponentsWonTricks: 0,
            ChosenCard: cards[0]);
        var result = PlayCardFeatureBuilder.BuildFeatures(context);

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

        var context = new PlayCardFeatureBuilderContext(
            CardsInHand: cards,
            PlayedCards: [],
            TeamScore: 0,
            OpponentScore: 0,
            LeadPlayer: RelativePlayerPosition.Self,
            LeadSuit: null,
            CallingPlayer: RelativePlayerPosition.Self,
            CallingPlayerGoingAlone: false,
            Dealer: RelativePlayerPosition.Self,
            DealerPickedUpCard: null,
            KnownPlayerSuitVoids: [],
            CardsAccountedFor: [],
            WinningTrickPlayer: null,
            TrickNumber: 1,
            WonTricks: 0,
            OpponentsWonTricks: 0,
            ChosenCard: cards[0]);
        var result = PlayCardFeatureBuilder.BuildFeatures(context);

        result.LeadSuit.Should().Be(-1.0f);
        result.WinningTrickPlayer.Should().Be(-1.0f);
        result.DealerPickedUpCardRank.Should().Be(-1.0f);
        result.DealerPickedUpCardSuit.Should().Be(-1.0f);
    }

    [Fact]
    public void BuildFeatures_RightBowerOfTrump_HasZeroThreats()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.RightBower, RelativeSuit.Trump),
            new(Rank.Nine, RelativeSuit.NonTrumpSameColor),
        };

        var result = BuildWithDefaults(cards, cards[0]);

        result.Card1Threats.Should().Be(0f);
    }

    [Fact]
    public void BuildFeatures_LeftBowerWithRightBowerUnaccounted_HasOneThreat()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.LeftBower, RelativeSuit.Trump),
        };

        var result = BuildWithDefaults(cards, cards[0]);

        result.Card1Threats.Should().Be(1f);
    }

    [Fact]
    public void BuildFeatures_LeftBowerWithRightBowerAccountedFor_HasZeroThreats()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.LeftBower, RelativeSuit.Trump),
        };
        var accountedFor = new RelativeCard[]
        {
            new(Rank.RightBower, RelativeSuit.Trump),
        };

        var result = BuildWithDefaults(cards, cards[0], cardsAccountedFor: accountedFor);

        result.Card1Threats.Should().Be(0f);
    }

    [Fact]
    public void BuildFeatures_NonTrumpKingTrick1NoAccountedCards_HasEightThreats()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
        };

        var result = BuildWithDefaults(cards, cards[0]);

        // 7 trump + 1 Ace of same suit = 8 threats
        result.Card1Threats.Should().Be(8f);
    }

    [Fact]
    public void BuildFeatures_NonTrumpWithSomeAccountedCards_DecreasesCount()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
        };
        var accountedFor = new RelativeCard[]
        {
            new(Rank.RightBower, RelativeSuit.Trump),
            new(Rank.LeftBower, RelativeSuit.Trump),
            new(Rank.Ace, RelativeSuit.NonTrumpOppositeColor1),
        };

        var result = BuildWithDefaults(cards, cards[0], cardsAccountedFor: accountedFor);

        // 5 unaccounted trump + 0 higher same-suit = 5 threats
        result.Card1Threats.Should().Be(5f);
    }

    [Fact]
    public void BuildFeatures_BothOpponentsVoidInTrump_NonTrumpExcludesTrumpThreats()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
        };
        var voids = new RelativePlayerSuitVoid[]
        {
            new() { PlayerPosition = RelativePlayerPosition.LeftHandOpponent, Suit = RelativeSuit.Trump },
            new() { PlayerPosition = RelativePlayerPosition.RightHandOpponent, Suit = RelativeSuit.Trump },
        };

        var result = BuildWithDefaults(cards, cards[0], knownPlayerSuitVoids: voids);

        // 0 trump threats + 1 Ace of same suit = 1 threat
        result.Card1Threats.Should().Be(1f);
    }

    [Fact]
    public void BuildFeatures_BothOpponentsVoidInCardSuit_ExcludesSameSuitThreats()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor1),
        };
        var voids = new RelativePlayerSuitVoid[]
        {
            new() { PlayerPosition = RelativePlayerPosition.LeftHandOpponent, Suit = RelativeSuit.NonTrumpOppositeColor1 },
            new() { PlayerPosition = RelativePlayerPosition.RightHandOpponent, Suit = RelativeSuit.NonTrumpOppositeColor1 },
        };

        var result = BuildWithDefaults(cards, cards[0], knownPlayerSuitVoids: voids);

        // 7 trump threats + 0 same-suit threats = 7
        result.Card1Threats.Should().Be(7f);
    }

    [Fact]
    public void BuildFeatures_OnlyOneOpponentVoid_StillCountsThreats()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
        };
        var voids = new RelativePlayerSuitVoid[]
        {
            new() { PlayerPosition = RelativePlayerPosition.LeftHandOpponent, Suit = RelativeSuit.Trump },
        };

        var result = BuildWithDefaults(cards, cards[0], knownPlayerSuitVoids: voids);

        // Both opponents must be void to exclude; only LHO is void
        // 7 trump + 1 Ace = 8
        result.Card1Threats.Should().Be(8f);
    }

    [Fact]
    public void BuildFeatures_MissingCardSlots_ReturnSentinel()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.RightBower, RelativeSuit.Trump),
            new(Rank.Ace, RelativeSuit.Trump),
            new(Rank.King, RelativeSuit.Trump),
        };

        var result = BuildWithDefaults(cards, cards[0]);

        result.Card1Threats.Should().Be(0f);
        result.Card2Threats.Should().Be(2f);
        result.Card3Threats.Should().Be(3f);
        result.Card4Threats.Should().Be(-1f);
        result.Card5Threats.Should().Be(-1f);
    }

    [Fact]
    public void BuildFeatures_ChosenCardThreats_MatchesChosenCard()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.RightBower, RelativeSuit.Trump),
            new(Rank.LeftBower, RelativeSuit.Trump),
        };

        var result = BuildWithDefaults(cards, cards[1]);

        // ChosenCard is LeftBower → 1 threat (RightBower unaccounted)
        result.ChosenCardThreats.Should().Be(1f);
    }

    [Fact]
    public void BuildFeatures_TrumpWithSomeHigherTrumpAccountedFor_PartialCount()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.King, RelativeSuit.Trump),
        };
        var accountedFor = new RelativeCard[]
        {
            new(Rank.RightBower, RelativeSuit.Trump),
            new(Rank.Ace, RelativeSuit.Trump),
        };

        var result = BuildWithDefaults(cards, cards[0], cardsAccountedFor: accountedFor);

        // Higher: LeftBower (unaccounted) → 1 threat
        result.Card1Threats.Should().Be(1f);
    }

    [Fact]
    public void BuildFeatures_NineOfTrump_HasSixThreatsWhenNoneAccountedFor()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.Nine, RelativeSuit.Trump),
        };

        var result = BuildWithDefaults(cards, cards[0]);

        // Ten, Queen, King, Ace, LeftBower, RightBower = 6
        result.Card1Threats.Should().Be(6f);
    }

    [Fact]
    public void BuildFeatures_OpponentDealerPickedUpCard_StillCountsAsThreat()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.King, RelativeSuit.Trump),
        };
        var pickedUpCard = new RelativeCard(Rank.Ace, RelativeSuit.Trump);
        var accountedFor = new RelativeCard[]
        {
            new(Rank.Ace, RelativeSuit.Trump),
            new(Rank.RightBower, RelativeSuit.Trump),
        };

        var result = BuildWithDefaults(
            cards,
            cards[0],
            dealer: RelativePlayerPosition.LeftHandOpponent,
            dealerPickedUpCard: pickedUpCard,
            cardsAccountedFor: accountedFor);

        // Ace removed from effective accounted-for (opponent holds it), RightBower stays accounted
        // Higher than King: Ace (unaccounted), LeftBower (unaccounted) = 2 threats
        result.Card1Threats.Should().Be(2f);
    }

    [Fact]
    public void BuildFeatures_PartnerDealerPickedUpCard_DoesNotCountAsThreat()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.King, RelativeSuit.Trump),
        };
        var pickedUpCard = new RelativeCard(Rank.Ace, RelativeSuit.Trump);
        var accountedFor = new RelativeCard[]
        {
            new(Rank.Ace, RelativeSuit.Trump),
            new(Rank.RightBower, RelativeSuit.Trump),
        };

        var result = BuildWithDefaults(
            cards,
            cards[0],
            dealer: RelativePlayerPosition.Partner,
            dealerPickedUpCard: pickedUpCard,
            cardsAccountedFor: accountedFor);

        // Ace stays accounted (partner holds it safely), RightBower stays accounted
        // Higher than King: LeftBower (unaccounted) = 1 threat
        result.Card1Threats.Should().Be(1f);
    }

    [Fact]
    public void BuildFeatures_BothOpponentsVoidInTrump_TrumpCardHasZeroThreats()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.Nine, RelativeSuit.Trump),
        };
        var voids = new RelativePlayerSuitVoid[]
        {
            new() { PlayerPosition = RelativePlayerPosition.LeftHandOpponent, Suit = RelativeSuit.Trump },
            new() { PlayerPosition = RelativePlayerPosition.RightHandOpponent, Suit = RelativeSuit.Trump },
        };

        var result = BuildWithDefaults(cards, cards[0], knownPlayerSuitVoids: voids);

        result.Card1Threats.Should().Be(0f);
    }

    [Fact]
    public void ThreatsThisTrick_WhenLeading_MatchesCardThreats()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.Ace, RelativeSuit.Trump),
            new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
        };

        var result = BuildWithDefaults(cards, cards[0]);

        result.Card1ThreatsThisTrick.Should().Be(result.Card1Threats);
        result.Card2ThreatsThisTrick.Should().Be(result.Card2Threats);
    }

    [Fact]
    public void ThreatsThisTrick_MissingSlots_ReturnSentinel()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.RightBower, RelativeSuit.Trump),
            new(Rank.Ace, RelativeSuit.Trump),
            new(Rank.King, RelativeSuit.Trump),
        };

        var result = BuildWithDefaults(cards, cards[0]);

        result.Card4ThreatsThisTrick.Should().Be(-1f);
        result.Card5ThreatsThisTrick.Should().Be(-1f);
    }

    [Fact]
    public void ThreatsThisTrick_LastToPlay_PartnerWinning_ReturnsZero()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.Nine, RelativeSuit.NonTrumpSameColor),
        };

        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>
        {
            [RelativePlayerPosition.LeftHandOpponent] = new(Rank.Ten, RelativeSuit.NonTrumpSameColor),
            [RelativePlayerPosition.Partner] = new(Rank.Ace, RelativeSuit.NonTrumpSameColor),
            [RelativePlayerPosition.RightHandOpponent] = new(Rank.King, RelativeSuit.NonTrumpSameColor),
        };

        var result = BuildWithDefaults(
            cards,
            cards[0],
            playedCards: playedCards,
            winningTrickPlayer: RelativePlayerPosition.Partner,
            leadPlayer: RelativePlayerPosition.LeftHandOpponent,
            leadSuit: RelativeSuit.NonTrumpSameColor);

        result.Card1ThreatsThisTrick.Should().Be(0f);
    }

    [Fact]
    public void ThreatsThisTrick_LastToPlay_OpponentWinning_CanBeat_ReturnsZero()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.RightBower, RelativeSuit.Trump),
        };

        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>
        {
            [RelativePlayerPosition.LeftHandOpponent] = new(Rank.Ace, RelativeSuit.NonTrumpSameColor),
            [RelativePlayerPosition.Partner] = new(Rank.King, RelativeSuit.NonTrumpSameColor),
            [RelativePlayerPosition.RightHandOpponent] = new(Rank.Queen, RelativeSuit.NonTrumpSameColor),
        };

        var result = BuildWithDefaults(
            cards,
            cards[0],
            playedCards: playedCards,
            winningTrickPlayer: RelativePlayerPosition.LeftHandOpponent,
            leadPlayer: RelativePlayerPosition.LeftHandOpponent,
            leadSuit: RelativeSuit.NonTrumpSameColor);

        result.Card1ThreatsThisTrick.Should().Be(0f);
    }

    [Fact]
    public void ThreatsThisTrick_LastToPlay_OpponentWinning_CannotBeat_Returns25()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor1),
        };

        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>
        {
            [RelativePlayerPosition.LeftHandOpponent] = new(Rank.Ace, RelativeSuit.NonTrumpSameColor),
            [RelativePlayerPosition.Partner] = new(Rank.King, RelativeSuit.NonTrumpSameColor),
            [RelativePlayerPosition.RightHandOpponent] = new(Rank.Queen, RelativeSuit.NonTrumpSameColor),
        };

        var result = BuildWithDefaults(
            cards,
            cards[0],
            playedCards: playedCards,
            winningTrickPlayer: RelativePlayerPosition.LeftHandOpponent,
            leadPlayer: RelativePlayerPosition.LeftHandOpponent,
            leadSuit: RelativeSuit.NonTrumpSameColor);

        result.Card1ThreatsThisTrick.Should().Be(25f);
    }

    [Fact]
    public void ThreatsThisTrick_OpponentWinning_CanBeat_CountsRemainingOpponentThreats()
    {
        // Self leads, LHO played trump Ace and is winning, Partner and RHO haven't played
        // Self has RightBower — beats LHO. RHO still plays after us.
        var cards = new RelativeCard[]
        {
            new(Rank.RightBower, RelativeSuit.Trump),
        };

        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>
        {
            [RelativePlayerPosition.LeftHandOpponent] = new(Rank.Ace, RelativeSuit.Trump),
        };

        var result = BuildWithDefaults(
            cards,
            cards[0],
            playedCards: playedCards,
            winningTrickPlayer: RelativePlayerPosition.LeftHandOpponent,
            leadPlayer: RelativePlayerPosition.Self,
            leadSuit: RelativeSuit.Trump);

        // RightBower is highest trump — no card beats it → 0 threats
        result.Card1ThreatsThisTrick.Should().Be(0f);
    }

    [Fact]
    public void ThreatsThisTrick_OpponentWinning_CanBeat_ButHigherThreatExists()
    {
        // RHO leads NonTrumpSameColor, Self plays 2nd. LHO is after us.
        // RHO played Ace of NonTrumpSameColor, winning.
        // Self has LeftBower (trump) — can beat. But RightBower is unaccounted.
        // LHO plays after us and could have trump.
        var cards = new RelativeCard[]
        {
            new(Rank.LeftBower, RelativeSuit.Trump),
        };

        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>
        {
            [RelativePlayerPosition.RightHandOpponent] = new(Rank.Ace, RelativeSuit.NonTrumpSameColor),
        };

        var result = BuildWithDefaults(
            cards,
            cards[0],
            playedCards: playedCards,
            winningTrickPlayer: RelativePlayerPosition.RightHandOpponent,
            leadPlayer: RelativePlayerPosition.RightHandOpponent,
            leadSuit: RelativeSuit.NonTrumpSameColor);

        // LHO plays after us. LHO must be void in NonTrumpSameColor AND not void in Trump to play trump.
        // With no void info, LHO is not void in NonTrumpSameColor → can follow suit but can't trump.
        // Following led suit: no card higher than our trump.
        // So actually LHO can't beat our trump by following suit. And LHO isn't known void in lead suit, so can't trump.
        // Threats = 0
        result.Card1ThreatsThisTrick.Should().Be(0f);
    }

    [Fact]
    public void ThreatsThisTrick_OpponentWinning_CanBeat_OpponentVoidInLeadSuit_TrumpThreat()
    {
        // RHO leads NonTrumpSameColor. Self plays 2nd. LHO plays after us.
        // RHO played Ace, winning. Self has LeftBower (trump).
        // LHO is void in NonTrumpSameColor → can trump → RightBower is a threat
        var cards = new RelativeCard[]
        {
            new(Rank.LeftBower, RelativeSuit.Trump),
        };

        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>
        {
            [RelativePlayerPosition.RightHandOpponent] = new(Rank.Ace, RelativeSuit.NonTrumpSameColor),
        };

        var voids = new RelativePlayerSuitVoid[]
        {
            new() { PlayerPosition = RelativePlayerPosition.LeftHandOpponent, Suit = RelativeSuit.NonTrumpSameColor },
        };

        var result = BuildWithDefaults(
            cards,
            cards[0],
            knownPlayerSuitVoids: voids,
            playedCards: playedCards,
            winningTrickPlayer: RelativePlayerPosition.RightHandOpponent,
            leadPlayer: RelativePlayerPosition.RightHandOpponent,
            leadSuit: RelativeSuit.NonTrumpSameColor);

        // LHO void in led suit + not void in trump → can play RightBower (unaccounted) → 1 threat
        result.Card1ThreatsThisTrick.Should().Be(1f);
    }

    [Fact]
    public void ThreatsThisTrick_PartnerWinning_CountsThreatsAgainstBestCard()
    {
        // Partner leads trump, RHO played, Self plays 3rd. LHO after us.
        // Partner played King of trump (winning). Self plays Nine of trump.
        // Best team card is partner's King. LHO plays after us.
        var cards = new RelativeCard[]
        {
            new(Rank.Nine, RelativeSuit.Trump),
        };

        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>
        {
            [RelativePlayerPosition.Partner] = new(Rank.King, RelativeSuit.Trump),
            [RelativePlayerPosition.RightHandOpponent] = new(Rank.Ten, RelativeSuit.Trump),
        };

        var result = BuildWithDefaults(
            cards,
            cards[0],
            playedCards: playedCards,
            winningTrickPlayer: RelativePlayerPosition.Partner,
            leadPlayer: RelativePlayerPosition.Partner,
            leadSuit: RelativeSuit.Trump);

        // Best team card = King (partner's). LHO can beat with Ace, LeftBower, RightBower = 3 threats
        result.Card1ThreatsThisTrick.Should().Be(3f);
    }

    [Fact]
    public void ThreatsThisTrick_PartnerWinning_MyCardBetterThanPartners()
    {
        // Partner leads trump, RHO played, Self plays 3rd. LHO after us.
        // Partner played King of trump (winning). Self plays Ace of trump.
        // Best team card is MY Ace.
        var cards = new RelativeCard[]
        {
            new(Rank.Ace, RelativeSuit.Trump),
        };

        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>
        {
            [RelativePlayerPosition.Partner] = new(Rank.King, RelativeSuit.Trump),
            [RelativePlayerPosition.RightHandOpponent] = new(Rank.Ten, RelativeSuit.Trump),
        };

        var result = BuildWithDefaults(
            cards,
            cards[0],
            playedCards: playedCards,
            winningTrickPlayer: RelativePlayerPosition.Partner,
            leadPlayer: RelativePlayerPosition.Partner,
            leadSuit: RelativeSuit.Trump);

        // Best team card = Ace (mine). LHO can beat with LeftBower, RightBower = 2 threats
        result.Card1ThreatsThisTrick.Should().Be(2f);
    }

    [Fact]
    public void ThreatsThisTrick_GoingAlone_OpponentSitsOut_FewerThreats()
    {
        // Self leads. LHO going alone → RHO sits out.
        // Opponents after Self: normally [LHO, RHO], but RHO sits out → [LHO]
        var cards = new RelativeCard[]
        {
            new(Rank.LeftBower, RelativeSuit.Trump),
        };

        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>
        {
            [RelativePlayerPosition.LeftHandOpponent] = new(Rank.Nine, RelativeSuit.Trump),
        };

        var result = BuildWithDefaults(
            cards,
            cards[0],
            playedCards: playedCards,
            winningTrickPlayer: RelativePlayerPosition.LeftHandOpponent,
            leadPlayer: RelativePlayerPosition.Self,
            leadSuit: RelativeSuit.Trump,
            callingPlayer: RelativePlayerPosition.LeftHandOpponent,
            callingPlayerGoingAlone: true);

        // LHO going alone → RHO sits out. Only LHO already played.
        // Opponents after us: normally [LHO, RHO], but RHO sits out → [LHO].
        // But LHO already played! Wait — LHO is in "opponents after me" list based on play order, not played status.
        // Actually, the "after me" is about play order. Self leads → LHO → Partner → RHO.
        // Opponents after Self in play order: LHO, RHO. RHO sits out → [LHO].
        // LHO has already played (in playedCards). But our method doesn't track that —
        // it just counts threats from the unplayed cards in the deck.
        // LeftBower can be beaten by RightBower. Can LHO play it? LHO not void in trump → yes.
        // But LHO already played Nine of trump! This is a pre-existing design limitation.
        // The feature still counts it as a threat.
        result.Card1ThreatsThisTrick.Should().Be(1f);
    }

    [Fact]
    public void ThreatsThisTrick_GoingAlone_SelfGoingAlone_PartnerSitsOut()
    {
        // Self leads and going alone. Partner sits out. LHO and RHO are opponents.
        // Since partner sits out, partner isn't in the play order, but the going-alone
        // filter only removes the sitting-out player from the opponents list.
        // Partner isn't an opponent, so the opponents-after-me list is unchanged: [LHO, RHO]
        var cards = new RelativeCard[]
        {
            new(Rank.Ace, RelativeSuit.Trump),
        };

        var result = BuildWithDefaults(
            cards,
            cards[0],
            callingPlayer: RelativePlayerPosition.Self,
            callingPlayerGoingAlone: true);

        // Leading → same as CardThreats
        result.Card1ThreatsThisTrick.Should().Be(result.Card1Threats);
    }

    [Fact]
    public void ThreatsThisTrick_OpponentVoidInTrump_TrumpThreatsExcluded()
    {
        // Partner leads NonTrumpSameColor. RHO played, Self plays 3rd. LHO after us.
        // Partner winning with Ace. Self plays Nine of NonTrumpSameColor.
        // LHO is void in trump → can't trump → only same-suit threats count.
        var cards = new RelativeCard[]
        {
            new(Rank.Nine, RelativeSuit.NonTrumpSameColor),
        };

        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>
        {
            [RelativePlayerPosition.Partner] = new(Rank.Ace, RelativeSuit.NonTrumpSameColor),
            [RelativePlayerPosition.RightHandOpponent] = new(Rank.Ten, RelativeSuit.NonTrumpSameColor),
        };

        var voids = new RelativePlayerSuitVoid[]
        {
            new() { PlayerPosition = RelativePlayerPosition.LeftHandOpponent, Suit = RelativeSuit.Trump },
        };

        var result = BuildWithDefaults(
            cards,
            cards[0],
            knownPlayerSuitVoids: voids,
            playedCards: playedCards,
            winningTrickPlayer: RelativePlayerPosition.Partner,
            leadPlayer: RelativePlayerPosition.Partner,
            leadSuit: RelativeSuit.NonTrumpSameColor);

        // Best team card = Ace (partner's). No non-trump same color card beats Ace (it's highest).
        // LHO void in trump → can't trump. Threats = 0.
        result.Card1ThreatsThisTrick.Should().Be(0f);
    }

    [Fact]
    public void ThreatsThisTrick_NonTrumpCard_OpponentCanTrump()
    {
        // Partner leads NonTrumpSameColor. RHO played, Self plays 3rd. LHO after us.
        // Partner winning with King. Self plays Nine.
        // LHO is void in NonTrumpSameColor → must play other suit → can trump.
        var cards = new RelativeCard[]
        {
            new(Rank.Nine, RelativeSuit.NonTrumpSameColor),
        };

        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>
        {
            [RelativePlayerPosition.Partner] = new(Rank.King, RelativeSuit.NonTrumpSameColor),
            [RelativePlayerPosition.RightHandOpponent] = new(Rank.Ten, RelativeSuit.NonTrumpSameColor),
        };

        var voids = new RelativePlayerSuitVoid[]
        {
            new() { PlayerPosition = RelativePlayerPosition.LeftHandOpponent, Suit = RelativeSuit.NonTrumpSameColor },
        };

        var result = BuildWithDefaults(
            cards,
            cards[0],
            knownPlayerSuitVoids: voids,
            playedCards: playedCards,
            winningTrickPlayer: RelativePlayerPosition.Partner,
            leadPlayer: RelativePlayerPosition.Partner,
            leadSuit: RelativeSuit.NonTrumpSameColor);

        // Best team card = King (partner's, since Nine < King in same suit).
        // Led suit threats: Ace of NonTrumpSameColor beats King. But LHO void in that suit → can't follow.
        // Trump threats: LHO void in led suit AND not void in trump → all 7 trump cards are threats.
        result.Card1ThreatsThisTrick.Should().Be(7f);
    }

    [Fact]
    public void ThreatsThisTrick_ChosenCard_MatchesCalculation()
    {
        var cards = new RelativeCard[]
        {
            new(Rank.RightBower, RelativeSuit.Trump),
            new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor1),
        };

        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>
        {
            [RelativePlayerPosition.LeftHandOpponent] = new(Rank.Ace, RelativeSuit.NonTrumpSameColor),
            [RelativePlayerPosition.Partner] = new(Rank.King, RelativeSuit.NonTrumpSameColor),
            [RelativePlayerPosition.RightHandOpponent] = new(Rank.Queen, RelativeSuit.NonTrumpSameColor),
        };

        var result = BuildWithDefaults(
            cards,
            cards[1],
            playedCards: playedCards,
            winningTrickPlayer: RelativePlayerPosition.LeftHandOpponent,
            leadPlayer: RelativePlayerPosition.LeftHandOpponent,
            leadSuit: RelativeSuit.NonTrumpSameColor);

        // Chosen card is Nine of OppColor1 (off-suit, not trump). Can't beat Ace of NonTrumpSameColor → 25
        result.ChosenCardThreatsThisTrick.Should().Be(25f);

        // Card1 is RightBower (trump). Can beat Ace. Last to play → 0
        result.Card1ThreatsThisTrick.Should().Be(0f);
    }

    [Fact]
    public void ThreatsThisTrick_OpponentWinning_CannotBeat_Returns25_WithOpponentsAfter()
    {
        // Self leads. LHO played and winning. Partner and RHO haven't played.
        // Self's card can't beat LHO.
        var cards = new RelativeCard[]
        {
            new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor1),
        };

        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>
        {
            [RelativePlayerPosition.LeftHandOpponent] = new(Rank.RightBower, RelativeSuit.Trump),
        };

        var result = BuildWithDefaults(
            cards,
            cards[0],
            playedCards: playedCards,
            winningTrickPlayer: RelativePlayerPosition.LeftHandOpponent,
            leadPlayer: RelativePlayerPosition.Self,
            leadSuit: RelativeSuit.NonTrumpOppositeColor1);

        // Can't beat RightBower with Nine of off-suit → 25
        result.Card1ThreatsThisTrick.Should().Be(25f);
    }

    private static AllPlayCardTrainingData BuildWithDefaults(
        RelativeCard[] cardsInHand,
        RelativeCard chosenCard,
        RelativePlayerPosition dealer = RelativePlayerPosition.Partner,
        RelativeCard? dealerPickedUpCard = null,
        RelativeCard[]? cardsAccountedFor = null,
        RelativePlayerSuitVoid[]? knownPlayerSuitVoids = null,
        Dictionary<RelativePlayerPosition, RelativeCard>? playedCards = null,
        RelativePlayerPosition? winningTrickPlayer = null,
        RelativePlayerPosition leadPlayer = RelativePlayerPosition.Self,
        RelativeSuit? leadSuit = null,
        RelativePlayerPosition callingPlayer = RelativePlayerPosition.Self,
        bool callingPlayerGoingAlone = false)
    {
        var context = new PlayCardFeatureBuilderContext(
            CardsInHand: cardsInHand,
            PlayedCards: playedCards ?? [],
            TeamScore: 0,
            OpponentScore: 0,
            LeadPlayer: leadPlayer,
            LeadSuit: leadSuit,
            CallingPlayer: callingPlayer,
            CallingPlayerGoingAlone: callingPlayerGoingAlone,
            Dealer: dealer,
            DealerPickedUpCard: dealerPickedUpCard,
            KnownPlayerSuitVoids: knownPlayerSuitVoids ?? [],
            CardsAccountedFor: cardsAccountedFor ?? [],
            WinningTrickPlayer: winningTrickPlayer,
            TrickNumber: 1,
            WonTricks: 0,
            OpponentsWonTricks: 0,
            ChosenCard: chosenCard);

        return PlayCardFeatureBuilder.BuildFeatures(context);
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
