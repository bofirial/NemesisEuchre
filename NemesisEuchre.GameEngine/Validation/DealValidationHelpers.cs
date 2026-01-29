using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Validation;

public static class DealValidationHelpers
{
    public const int PlayersPerDeal = 4;

    public static void ValidatePlayerCount(Deal deal)
    {
        if (deal.Players.Count != PlayersPerDeal)
        {
            throw new InvalidOperationException(
                $"Deal must have exactly {PlayersPerDeal} players, but had {deal.Players.Count}");
        }
    }

    public static void ValidateDealerPosition(Deal deal)
    {
        if (deal.DealerPosition == null)
        {
            throw new InvalidOperationException("DealerPosition must be set");
        }
    }

    public static void ValidateUpCard(Deal deal)
    {
        if (deal.UpCard == null)
        {
            throw new InvalidOperationException("UpCard must be set");
        }
    }

    public static void ValidateTrump(Deal deal)
    {
        if (deal.Trump == null)
        {
            throw new InvalidOperationException("Trump must be set");
        }
    }

    public static void ValidateCallingPlayer(Deal deal)
    {
        if (deal.CallingPlayer == null)
        {
            throw new InvalidOperationException("CallingPlayer must be set");
        }
    }

    public static void ValidateDealStatus(Deal deal, DealStatus expectedStatus)
    {
        if (deal.DealStatus != expectedStatus)
        {
            throw new InvalidOperationException(
                $"Deal must be in {expectedStatus} status, but was {deal.DealStatus}");
        }
    }
}
