using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;

namespace NemesisEuchre.DataAccess.Extensions;

public static class DealResultExtensions
{
    public static short? CalculateRelativeDealPoints(
        this DealResult? dealResult,
        PlayerPosition playerPosition,
        Team? dealWinningTeam)
    {
        if (dealResult == null || dealWinningTeam == null)
        {
            return null;
        }

        if (dealResult == DealResult.ThrowIn)
        {
            return 0;
        }

        var playerTeam = playerPosition.GetTeam();
        var winningTeam = dealWinningTeam.Value;

        short basePoints = dealResult.Value switch
        {
            DealResult.WonStandardBid => 1,
            DealResult.WonGotAllTricks => 2,
            DealResult.WonAndWentAlone => 4,
            DealResult.OpponentsEuchred => 2,
            DealResult.ThrowIn => throw new InvalidOperationException("ThrowIn should have been handled earlier"),
            _ => throw new ArgumentOutOfRangeException(nameof(dealResult), dealResult, "Invalid DealResult value"),
        };

        return playerTeam == winningTeam ? basePoints : (short)-basePoints;
    }
}
