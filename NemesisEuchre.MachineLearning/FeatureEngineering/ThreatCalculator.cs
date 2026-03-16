using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

internal static class ThreatCalculator
{
    public static readonly Rank[] TrumpRanks =
        [Rank.Nine, Rank.Ten, Rank.Queen, Rank.King, Rank.Ace, Rank.LeftBower, Rank.RightBower];

    public static readonly Rank[] NonTrumpSameColorRanks =
        [Rank.Nine, Rank.Ten, Rank.Queen, Rank.King, Rank.Ace];

    public static readonly Rank[] NonTrumpOppositeColorRanks =
        [Rank.Nine, Rank.Ten, Rank.Jack, Rank.Queen, Rank.King, Rank.Ace];

    public static float CalculateThreats(
        RelativeCard card,
        RelativeCard[] effectiveAccountedFor,
        RelativePlayerSuitVoid[] knownPlayerSuitVoids)
    {
        if (card.Suit == RelativeSuit.Trump)
        {
            if (BothOpponentsVoidIn(knownPlayerSuitVoids, RelativeSuit.Trump))
            {
                return 0f;
            }

            return CountUnaccountedHigherCards(card.Rank, RelativeSuit.Trump, TrumpRanks, effectiveAccountedFor);
        }

        float threats = 0f;

        if (!BothOpponentsVoidIn(knownPlayerSuitVoids, RelativeSuit.Trump))
        {
            threats += CountAllUnaccountedTrumpCards(effectiveAccountedFor);
        }

        if (!BothOpponentsVoidIn(knownPlayerSuitVoids, card.Suit))
        {
            Rank[] ranksForSuit = card.Suit == RelativeSuit.NonTrumpSameColor
                ? NonTrumpSameColorRanks
                : NonTrumpOppositeColorRanks;

            threats += CountUnaccountedHigherCards(card.Rank, card.Suit, ranksForSuit, effectiveAccountedFor);
        }

        return threats;
    }

    public static bool BothOpponentsVoidIn(RelativePlayerSuitVoid[] voids, RelativeSuit suit)
    {
        bool lhoVoid = false;
        bool rhoVoid = false;

        for (int i = 0; i < voids.Length; i++)
        {
            if (voids[i].Suit == suit)
            {
                if (voids[i].PlayerPosition == RelativePlayerPosition.LeftHandOpponent)
                {
                    lhoVoid = true;
                }
                else if (voids[i].PlayerPosition == RelativePlayerPosition.RightHandOpponent)
                {
                    rhoVoid = true;
                }
            }
        }

        return lhoVoid && rhoVoid;
    }

    public static float CountUnaccountedHigherCards(
        Rank rank,
        RelativeSuit suit,
        Rank[] validRanks,
        RelativeCard[] effectiveAccountedFor)
    {
        float count = 0f;

        for (int i = 0; i < validRanks.Length; i++)
        {
            if (validRanks[i] > rank && !IsAccountedFor(validRanks[i], suit, effectiveAccountedFor))
            {
                count++;
            }
        }

        return count;
    }

    public static float CountAllUnaccountedTrumpCards(RelativeCard[] effectiveAccountedFor)
    {
        float count = 0f;

        for (int i = 0; i < TrumpRanks.Length; i++)
        {
            if (!IsAccountedFor(TrumpRanks[i], RelativeSuit.Trump, effectiveAccountedFor))
            {
                count++;
            }
        }

        return count;
    }

    public static bool IsAccountedFor(Rank rank, RelativeSuit suit, RelativeCard[] effectiveAccountedFor)
    {
        for (int i = 0; i < effectiveAccountedFor.Length; i++)
        {
            if (effectiveAccountedFor[i].Rank == rank && effectiveAccountedFor[i].Suit == suit)
            {
                return true;
            }
        }

        return false;
    }
}
