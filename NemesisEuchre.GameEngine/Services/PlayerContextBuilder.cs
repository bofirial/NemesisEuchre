using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Services;

public interface IPlayerContextBuilder
{
    (short teamScore, short opponentScore) GetScores(Deal deal, PlayerPosition playerPosition);

    RelativePlayerPosition GetRelativeDealerPosition(Deal deal, PlayerPosition playerPosition);
}

public class PlayerContextBuilder : IPlayerContextBuilder
{
    public (short teamScore, short opponentScore) GetScores(Deal deal, PlayerPosition playerPosition)
    {
        var isTeam1 = playerPosition.GetTeam() == Team.Team1;
        return isTeam1 ? (teamScore: deal.Team1Score, opponentScore: deal.Team2Score) : (teamScore: deal.Team2Score, opponentScore: deal.Team1Score);
    }

    public RelativePlayerPosition GetRelativeDealerPosition(Deal deal, PlayerPosition playerPosition)
    {
        return deal.DealerPosition!.Value.ToRelativePosition(playerPosition);
    }
}
