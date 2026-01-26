using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Validation;

public interface ITrickPlayingValidator
{
    void ValidatePreconditions(Deal deal);

    void ValidateCardChoice(RelativeCard chosenCard, RelativeCard[] validCards);
}

public class TrickPlayingValidator : ITrickPlayingValidator
{
    private const int PlayersPerTrick = 4;

    public void ValidatePreconditions(Deal deal)
    {
        ArgumentNullException.ThrowIfNull(deal);

        if (deal.DealStatus != DealStatus.Playing)
        {
            throw new InvalidOperationException($"Deal must be in Playing status, but was {deal.DealStatus}");
        }

        if (deal.Trump == null)
        {
            throw new InvalidOperationException("Trump must be set");
        }

        if (deal.CallingPlayer == null)
        {
            throw new InvalidOperationException("CallingPlayer must be set");
        }

        if (deal.Players.Count != PlayersPerTrick)
        {
            throw new InvalidOperationException($"Deal must have exactly {PlayersPerTrick} players, but had {deal.Players.Count}");
        }
    }

    public void ValidateCardChoice(RelativeCard chosenCard, RelativeCard[] validCards)
    {
        if (!validCards.Contains(chosenCard))
        {
            throw new InvalidOperationException("ChosenCard was not included in ValidCards");
        }
    }
}
