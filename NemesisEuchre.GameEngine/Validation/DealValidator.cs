using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Validation;

public interface IDealValidator
{
    void ValidateDealPreconditions(Deal deal);

    void ValidateAllTricksPlayed(Deal deal);

    void ValidateDealCompleted(Deal deal);
}

public class DealValidator : IDealValidator
{
    private const int TricksPerDeal = 5;

    public void ValidateDealPreconditions(Deal deal)
    {
        ArgumentNullException.ThrowIfNull(deal);

        DealValidationHelpers.ValidateDealStatus(deal, DealStatus.NotStarted);
        DealValidationHelpers.ValidateDealerPosition(deal);
        DealValidationHelpers.ValidateUpCard(deal);
        DealValidationHelpers.ValidatePlayerCount(deal);
    }

    public void ValidateAllTricksPlayed(Deal deal)
    {
        if (deal.CompletedTricks.Count != TricksPerDeal)
        {
            throw new InvalidOperationException($"Deal must have exactly {TricksPerDeal} completed tricks, but had {deal.CompletedTricks.Count}");
        }
    }

    public void ValidateDealCompleted(Deal deal)
    {
        if (deal.DealResult == null)
        {
            throw new InvalidOperationException("DealResult must be set after scoring");
        }

        if (deal.WinningTeam == null)
        {
            throw new InvalidOperationException("WinningTeam must be set after scoring");
        }
    }
}
