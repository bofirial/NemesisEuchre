using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Mappers;

public interface ICallTrumpDecisionMapper
{
    CallTrumpDecision[] GetValidRound1Decisions();

    CallTrumpDecision[] GetValidRound2Decisions(Suit upcardSuit, bool isDealer, bool stickTheDealer);

    Suit ConvertDecisionToSuit(CallTrumpDecision decision);

    bool IsGoingAloneDecision(CallTrumpDecision decision);
}

public class CallTrumpDecisionMapper : ICallTrumpDecisionMapper
{
    public CallTrumpDecision[] GetValidRound1Decisions()
    {
        return [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp, CallTrumpDecision.OrderItUpAndGoAlone];
    }

    public CallTrumpDecision[] GetValidRound2Decisions(Suit upcardSuit, bool isDealer, bool stickTheDealer)
    {
        var decisions = new List<CallTrumpDecision>();

        if (!isDealer || !stickTheDealer)
        {
            decisions.Add(CallTrumpDecision.Pass);
        }

        foreach (var suit in Enum.GetValues<Suit>().Where(suit => suit != upcardSuit))
        {
            AddSuitDecisions(decisions, suit);
        }

        return [.. decisions];
    }

    public Suit ConvertDecisionToSuit(CallTrumpDecision decision)
    {
        return decision switch
        {
            CallTrumpDecision.CallClubs or CallTrumpDecision.CallClubsAndGoAlone => Suit.Clubs,
            CallTrumpDecision.CallDiamonds or CallTrumpDecision.CallDiamondsAndGoAlone => Suit.Diamonds,
            CallTrumpDecision.CallHearts or CallTrumpDecision.CallHeartsAndGoAlone => Suit.Hearts,
            CallTrumpDecision.CallSpades or CallTrumpDecision.CallSpadesAndGoAlone => Suit.Spades,
            CallTrumpDecision.Pass or
            CallTrumpDecision.OrderItUp or
            CallTrumpDecision.OrderItUpAndGoAlone or
            _ => throw new ArgumentOutOfRangeException(nameof(decision), $"Cannot convert {decision} to Suit")
        };
    }

    public bool IsGoingAloneDecision(CallTrumpDecision decision)
    {
        return decision is CallTrumpDecision.OrderItUpAndGoAlone
            or CallTrumpDecision.CallClubsAndGoAlone
            or CallTrumpDecision.CallDiamondsAndGoAlone
            or CallTrumpDecision.CallHeartsAndGoAlone
            or CallTrumpDecision.CallSpadesAndGoAlone;
    }

    private static void AddSuitDecisions(List<CallTrumpDecision> decisions, Suit suit)
    {
        var (callDecision, callAloneDecision) = suit switch
        {
            Suit.Clubs => (CallTrumpDecision.CallClubs, CallTrumpDecision.CallClubsAndGoAlone),
            Suit.Diamonds => (CallTrumpDecision.CallDiamonds, CallTrumpDecision.CallDiamondsAndGoAlone),
            Suit.Hearts => (CallTrumpDecision.CallHearts, CallTrumpDecision.CallHeartsAndGoAlone),
            Suit.Spades => (CallTrumpDecision.CallSpades, CallTrumpDecision.CallSpadesAndGoAlone),
            _ => throw new InvalidOperationException($"Invalid Suit: {suit}")
        };

        decisions.Add(callDecision);
        decisions.Add(callAloneDecision);
    }
}
