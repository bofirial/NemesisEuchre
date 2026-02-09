using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public interface IGameScoreUpdater
{
    Task UpdateGameScoreAsync(Game game, Deal deal);
}

public class GameScoreUpdater : IGameScoreUpdater
{
    public Task UpdateGameScoreAsync(Game game, Deal deal)
    {
        ValidateInputs(game, deal);

        if (deal.DealResult!.Value == DealResult.ThrowIn)
        {
            return Task.CompletedTask;
        }

        var scoreChange = CalculateScoreChange(deal);

        ApplyScoreChange(game, deal.WinningTeam!.Value, scoreChange);

        return Task.CompletedTask;
    }

    private static void ValidateInputs(Game game, Deal deal)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(deal);

        if (deal.DealResult is null)
        {
            throw new InvalidOperationException("Deal must have a DealResult before scoring.");
        }

        if (deal.DealResult.Value != DealResult.ThrowIn)
        {
            if (deal.WinningTeam is null)
            {
                throw new InvalidOperationException("Deal must have a WinningTeam before scoring.");
            }

            if (deal.CallingPlayer is null)
            {
                throw new InvalidOperationException("Deal must have a CallingPlayer before scoring.");
            }
        }
    }

    private static short CalculateScoreChange(Deal deal)
    {
        return deal.DealResult!.Value switch
        {
            DealResult.WonStandardBid => 1,
            DealResult.WonGotAllTricks => 2,
            DealResult.OpponentsEuchred => 2,
            DealResult.WonAndWentAlone => 4,
            DealResult.ThrowIn => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(deal), $"Unknown DealResult: {deal.DealResult}"),
        };
    }

    private static void ApplyScoreChange(Game game, Team winningTeam, short points)
    {
        if (winningTeam == Team.Team1)
        {
            game.Team1Score += points;
        }
        else
        {
            game.Team2Score += points;
        }
    }
}
