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
    private const int PlayersPerDeal = 4;
    private const int TricksPerDeal = 5;

    public void ValidateDealPreconditions(Deal deal)
    {
        ArgumentNullException.ThrowIfNull(deal);

        if (deal.DealStatus != DealStatus.NotStarted)
        {
            throw new InvalidOperationException($"Deal must be in NotStarted status, but was {deal.DealStatus}");
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
