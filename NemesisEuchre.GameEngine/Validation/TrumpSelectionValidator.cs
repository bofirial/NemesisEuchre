using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Validation;

public interface ITrumpSelectionValidator
{
    void ValidatePreconditions(Deal deal);

    void ValidateDecision(CallTrumpDecision decision, CallTrumpDecision[] validDecisions);

    void ValidateDiscard(RelativeCard cardToDiscard, RelativeCard[] validCards);
}

public class TrumpSelectionValidator : ITrumpSelectionValidator
{
    private const int PlayersPerDeal = 4;

    public void ValidatePreconditions(Deal deal)
    {
        ArgumentNullException.ThrowIfNull(deal);

        if (deal.DealStatus != DealStatus.SelectingTrump)
        {
            throw new InvalidOperationException($"Deal must be in SelectingTrump status, but was {deal.DealStatus}");
        }

        if (deal.DealerPosition == null)
        {
            throw new InvalidOperationException("DealerPosition must be set");
        }

        if (deal.UpCard == null)
        {
            throw new InvalidOperationException("UpCard must be set");
        }

        if (deal.Players.Count != PlayersPerDeal)
        {
            throw new InvalidOperationException($"Deal must have exactly {PlayersPerDeal} players, but had {deal.Players.Count}");
        }
    }

    public void ValidateDecision(CallTrumpDecision decision, CallTrumpDecision[] validDecisions)
    {
        if (!validDecisions.Contains(decision))
        {
            throw new InvalidOperationException("CallTrumpDecision was not included in ValidDecisions");
        }
    }

    public void ValidateDiscard(RelativeCard cardToDiscard, RelativeCard[] validCards)
    {
        if (!validCards.Contains(cardToDiscard))
        {
            throw new InvalidOperationException("CardToDiscard was not included in ValidCardsToDiscard");
        }
    }
}
