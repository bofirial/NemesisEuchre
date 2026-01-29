using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Validation;

public interface ITrickPlayingValidator
{
    void ValidatePreconditions(Deal deal);

    void ValidateCardChoice(Card chosenCard, Card[] validCards);
}

public class TrickPlayingValidator : ITrickPlayingValidator
{
    public void ValidatePreconditions(Deal deal)
    {
        ArgumentNullException.ThrowIfNull(deal);

        DealValidationHelpers.ValidateDealStatus(deal, DealStatus.Playing);
        DealValidationHelpers.ValidateTrump(deal);
        DealValidationHelpers.ValidateCallingPlayer(deal);
        DealValidationHelpers.ValidatePlayerCount(deal);
    }

    public void ValidateCardChoice(Card chosenCard, Card[] validCards)
    {
        if (!validCards.Contains(chosenCard))
        {
            throw new InvalidOperationException("ChosenCard was not included in ValidCards");
        }
    }
}
