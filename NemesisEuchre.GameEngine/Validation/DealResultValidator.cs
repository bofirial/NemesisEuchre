using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Validation;

public interface IDealResultValidator
{
    void ValidateDeal(Deal deal);
}

public class DealResultValidator : IDealResultValidator
{
    private const int TotalTricksInDeal = 5;

    public void ValidateDeal(Deal deal)
    {
        ArgumentNullException.ThrowIfNull(deal);
        ValidateCompletedTricksCount(deal);
        ValidateCallingPlayerExists(deal);
        ValidateAllTricksHaveWinningTeam(deal);
    }

    private static void ValidateCompletedTricksCount(Deal deal)
    {
        if (deal.CompletedTricks.Count != TotalTricksInDeal)
        {
            throw new InvalidOperationException(
                $"Deal must have exactly {TotalTricksInDeal} completed tricks");
        }
    }

    private static void ValidateCallingPlayerExists(Deal deal)
    {
        if (deal.CallingPlayer is null)
        {
            throw new InvalidOperationException("Deal must have a CallingPlayer set");
        }
    }

    private static void ValidateAllTricksHaveWinningTeam(Deal deal)
    {
        if (deal.CompletedTricks.Any(trick => trick.WinningTeam is null))
        {
            throw new InvalidOperationException(
                "All completed tricks must have WinningTeam set");
        }
    }
}
