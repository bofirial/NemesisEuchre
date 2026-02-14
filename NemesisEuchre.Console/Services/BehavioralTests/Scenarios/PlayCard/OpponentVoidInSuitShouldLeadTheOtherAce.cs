using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.PlayCard;

public class OpponentVoidInSuitShouldLeadTheOtherAce(
    IPlayCardInferenceFeatureBuilder featureBuilder)
    : PlayCardBehavioralTest(featureBuilder)
{
    public override string Name => "2 Aces in hand should lead suit not played";

    public override string Description =>
        "With two non-trump aces, lead the ace whose suit has more unaccounted cards and no known opponent void";

    public override string AssertionDescription => "Should lead ace in unplayed suit";

    protected override short TrickNumber => 4;

    protected override short WonTricks => 2;

    protected override short OpponentsWonTricks => 1;

    protected override IReadOnlyList<PlayCardTestCase> GetTestCases()
    {
        return [
            BuildTestCase(
                RelativeSuit.NonTrumpSameColor,
                RelativeSuit.NonTrumpOppositeColor1,
                RelativePlayerPosition.LeftHandOpponent),
            BuildTestCase(
                RelativeSuit.NonTrumpSameColor,
                RelativeSuit.NonTrumpOppositeColor1,
                RelativePlayerPosition.RightHandOpponent),
            BuildTestCase(
                RelativeSuit.NonTrumpOppositeColor1,
                RelativeSuit.NonTrumpSameColor,
                RelativePlayerPosition.LeftHandOpponent),
            BuildTestCase(
                RelativeSuit.NonTrumpOppositeColor1,
                RelativeSuit.NonTrumpSameColor,
                RelativePlayerPosition.RightHandOpponent),

            BuildTestCase(
                RelativeSuit.NonTrumpSameColor,
                RelativeSuit.NonTrumpOppositeColor2,
                RelativePlayerPosition.LeftHandOpponent),
            BuildTestCase(
                RelativeSuit.NonTrumpSameColor,
                RelativeSuit.NonTrumpOppositeColor2,
                RelativePlayerPosition.RightHandOpponent),
            BuildTestCase(
                RelativeSuit.NonTrumpOppositeColor2,
                RelativeSuit.NonTrumpSameColor,
                RelativePlayerPosition.LeftHandOpponent),
            BuildTestCase(
                RelativeSuit.NonTrumpOppositeColor2,
                RelativeSuit.NonTrumpSameColor,
                RelativePlayerPosition.RightHandOpponent),

            BuildTestCase(
                RelativeSuit.NonTrumpOppositeColor1,
                RelativeSuit.NonTrumpOppositeColor2,
                RelativePlayerPosition.LeftHandOpponent),
            BuildTestCase(
                RelativeSuit.NonTrumpOppositeColor1,
                RelativeSuit.NonTrumpOppositeColor2,
                RelativePlayerPosition.RightHandOpponent),
            BuildTestCase(
                RelativeSuit.NonTrumpOppositeColor2,
                RelativeSuit.NonTrumpOppositeColor1,
                RelativePlayerPosition.LeftHandOpponent),
            BuildTestCase(
                RelativeSuit.NonTrumpOppositeColor2,
                RelativeSuit.NonTrumpOppositeColor1,
                RelativePlayerPosition.RightHandOpponent),
        ];
    }

    private static string ShortSuit(RelativeSuit suit)
    {
        return suit switch
        {
            RelativeSuit.Trump => "T",
            RelativeSuit.NonTrumpSameColor => "SC ",
            RelativeSuit.NonTrumpOppositeColor1 => "OC1",
            RelativeSuit.NonTrumpOppositeColor2 => "OC2",
            _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, null),
        };
    }

    private static string ShortPosition(RelativePlayerPosition pos)
    {
        return pos switch
        {
            RelativePlayerPosition.Self => "Self",
            RelativePlayerPosition.LeftHandOpponent => "LHO",
            RelativePlayerPosition.Partner => "Partner",
            RelativePlayerPosition.RightHandOpponent => "RHO",
            _ => throw new ArgumentOutOfRangeException(nameof(pos), pos, null),
        };
    }

    private PlayCardTestCase BuildTestCase(
        RelativeSuit playedSuit,
        RelativeSuit openSuit,
        RelativePlayerPosition voidOpponent)
    {
        var aceOfPlayed = new RelativeCard(Rank.Ace, playedSuit);
        var aceOfOpen = new RelativeCard(Rank.Ace, openSuit);
        var hand = new[] { aceOfPlayed, aceOfOpen };

        var otherSuit = new[] { RelativeSuit.NonTrumpSameColor, RelativeSuit.NonTrumpOppositeColor1, RelativeSuit.NonTrumpOppositeColor2 }.SingleOrDefault(x => x != playedSuit && x != openSuit);

        var cardsAccountedFor = new RelativeCard[]
        {
            new(Rank.RightBower, RelativeSuit.Trump),
            new(Rank.LeftBower, RelativeSuit.Trump),
            new(Rank.Ace, RelativeSuit.Trump),
            new(Rank.King, RelativeSuit.Trump),

            new(Rank.Queen, RelativeSuit.Trump),
            new(Rank.King, playedSuit),
            new(Rank.Queen, playedSuit),
            new(Rank.Ten, playedSuit),

            new(Rank.Ace, otherSuit),
            new(Rank.King, otherSuit),
            new(Rank.Queen, otherSuit),
            new(Rank.Ten, otherSuit),
        };

        RelativePlayerSuitVoid[] voids =
        [
            new() { PlayerPosition = voidOpponent, Suit = playedSuit },
        ];

        var label = $"{Name} ({ShortSuit(openSuit)} over {ShortSuit(otherSuit)} [[{ShortPosition(voidOpponent)} void]])";

        return new PlayCardTestCase(
            label,
            hand,
            hand,
            IsExpectedOverride: card => card.Suit == openSuit,
            CardsAccountedFor: cardsAccountedFor,
            KnownPlayerSuitVoids: voids);
    }
}
