using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Validation;

public interface ITrumpSelectionValidator
{
    void ValidatePreconditions(Deal deal);

    void ValidateDecision(CallTrumpDecision decision, CallTrumpDecision[] validDecisions);

    void ValidateDiscard(Card cardToDiscard, Card[] validCards);
}

public class TrumpSelectionValidator : ITrumpSelectionValidator
{
    public void ValidatePreconditions(Deal deal)
    {
        ArgumentNullException.ThrowIfNull(deal);

        DealValidationHelpers.ValidateDealStatus(deal, DealStatus.SelectingTrump);
        DealValidationHelpers.ValidateDealerPosition(deal);
        DealValidationHelpers.ValidateUpCard(deal);
        DealValidationHelpers.ValidatePlayerCount(deal);
    }

    public void ValidateDecision(CallTrumpDecision decision, CallTrumpDecision[] validDecisions)
    {
        if (!validDecisions.Contains(decision))
        {
            throw new InvalidOperationException("CallTrumpDecision was not included in ValidDecisions");
        }
    }

    public void ValidateDiscard(Card cardToDiscard, Card[] validCards)
    {
        if (!validCards.Contains(cardToDiscard))
        {
            throw new InvalidOperationException("CardToDiscard was not included in ValidCardsToDiscard");
        }
    }
}
