using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Mappers;

public interface ICallTrumpDecisionMapper
{
    CallTrumpDecision[] GetValidRound1Decisions();

    CallTrumpDecision[] GetValidRound2Decisions(Suit upcardSuit, bool isDealer, bool stickTheDealer);

    Suit ConvertDecisionToSuit(CallTrumpDecision decision);

    bool IsGoingAloneDecision(CallTrumpDecision decision);
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Record positional parameters")]
internal sealed record CallTrumpDecisionMetadata(
    Suit? Suit,
    bool IsGoingAlone,
    CallTrumpDecision BaseDecision);

public class CallTrumpDecisionMapper : ICallTrumpDecisionMapper
{
    private static readonly Dictionary<CallTrumpDecision, CallTrumpDecisionMetadata> DecisionMetadata = new()
    {
        [CallTrumpDecision.Pass] = new(null, false, CallTrumpDecision.Pass),
        [CallTrumpDecision.OrderItUp] = new(null, false, CallTrumpDecision.OrderItUp),
        [CallTrumpDecision.OrderItUpAndGoAlone] = new(null, true, CallTrumpDecision.OrderItUp),
        [CallTrumpDecision.CallClubs] = new(Suit.Clubs, false, CallTrumpDecision.CallClubs),
        [CallTrumpDecision.CallClubsAndGoAlone] = new(Suit.Clubs, true, CallTrumpDecision.CallClubs),
        [CallTrumpDecision.CallDiamonds] = new(Suit.Diamonds, false, CallTrumpDecision.CallDiamonds),
        [CallTrumpDecision.CallDiamondsAndGoAlone] = new(Suit.Diamonds, true, CallTrumpDecision.CallDiamonds),
        [CallTrumpDecision.CallHearts] = new(Suit.Hearts, false, CallTrumpDecision.CallHearts),
        [CallTrumpDecision.CallHeartsAndGoAlone] = new(Suit.Hearts, true, CallTrumpDecision.CallHearts),
        [CallTrumpDecision.CallSpades] = new(Suit.Spades, false, CallTrumpDecision.CallSpades),
        [CallTrumpDecision.CallSpadesAndGoAlone] = new(Suit.Spades, true, CallTrumpDecision.CallSpades),
    };

    private static readonly Dictionary<Suit, (CallTrumpDecision Base, CallTrumpDecision Alone)> SuitToDecisions = new()
    {
        [Suit.Clubs] = (CallTrumpDecision.CallClubs, CallTrumpDecision.CallClubsAndGoAlone),
        [Suit.Diamonds] = (CallTrumpDecision.CallDiamonds, CallTrumpDecision.CallDiamondsAndGoAlone),
        [Suit.Hearts] = (CallTrumpDecision.CallHearts, CallTrumpDecision.CallHeartsAndGoAlone),
        [Suit.Spades] = (CallTrumpDecision.CallSpades, CallTrumpDecision.CallSpadesAndGoAlone),
    };

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
            var (baseDecision, aloneDecision) = SuitToDecisions[suit];
            decisions.Add(baseDecision);
            decisions.Add(aloneDecision);
        }

        return [.. decisions];
    }

    public Suit ConvertDecisionToSuit(CallTrumpDecision decision)
    {
        if (!DecisionMetadata.TryGetValue(decision, out var metadata))
        {
            throw new ArgumentOutOfRangeException(nameof(decision), $"Unknown decision: {decision}");
        }

        return metadata.Suit ?? throw new ArgumentOutOfRangeException(nameof(decision), $"Cannot convert {decision} to Suit");
    }

    public bool IsGoingAloneDecision(CallTrumpDecision decision)
    {
        return DecisionMetadata.TryGetValue(decision, out var metadata) && metadata.IsGoingAlone;
    }
}
