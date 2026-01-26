using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public interface IGameWinnerCalculator
{
    Team DetermineWinner(Game game);
}

public class GameWinnerCalculator : IGameWinnerCalculator
{
    public Team DetermineWinner(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        if (game.Team1Score == game.Team2Score)
        {
            throw new InvalidOperationException(
                $"Game ended in a tie ({game.Team1Score}-{game.Team2Score}), which should not occur in Euchre");
        }

        return game.Team1Score > game.Team2Score ? Team.Team1 : Team.Team2;
    }
}
