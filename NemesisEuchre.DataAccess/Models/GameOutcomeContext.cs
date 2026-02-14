using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Models;

public sealed record GameOutcomeContext(
    bool DidTeam1WinGame,
    bool DidTeam2WinGame)
{
    public static GameOutcomeContext None => new(false, false);

    public static GameOutcomeContext From(Game game)
    {
        return new(game.WinningTeam == Team.Team1, game.WinningTeam == Team.Team2);
    }
}
