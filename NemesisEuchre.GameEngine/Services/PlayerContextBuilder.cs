using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Services;

public interface IPlayerContextBuilder
{
    (short TeamScore, short OpponentScore) GetScores(Deal deal, PlayerPosition playerPosition);

    RelativePlayerPosition GetRelativeDealerPosition(Deal deal, PlayerPosition playerPosition);
}

public class PlayerContextBuilder : IPlayerContextBuilder
{
    public (short TeamScore, short OpponentScore) GetScores(Deal deal, PlayerPosition playerPosition)
    {
        var isTeam1 = playerPosition.GetTeam() == Team.Team1;
        return isTeam1 ? (deal.Team1Score, deal.Team2Score) : (deal.Team2Score, deal.Team1Score);
    }

    public RelativePlayerPosition GetRelativeDealerPosition(Deal deal, PlayerPosition playerPosition)
    {
        return deal.DealerPosition!.Value.ToRelativePosition(playerPosition);
    }
}
