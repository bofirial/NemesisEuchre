using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public class GameOrchestrator(IGameFactory gameFactory, IDealFactory dealFactory, IDealOrchestrator dealOrchestrator, IGameScoreUpdater gameScoreUpdater) : IGameOrchestrator
{
    public async Task<Game> OrchestrateGameAsync(GameOptions gameOptions)
    {
        ArgumentNullException.ThrowIfNull(gameOptions);
        ArgumentOutOfRangeException.ThrowIfLessThan(gameOptions.WinningScore, 1);

        var game = await gameFactory.CreateGameAsync(gameOptions);

        InitializeGame(game);

        await ProcessDealsAsync(game);

        FinalizeGame(game);

        return game;
    }

    private static void InitializeGame(Game game)
    {
        game.GameStatus = GameStatus.Playing;
    }

    private static void FinalizeGame(Game game)
    {
        game.GameStatus = GameStatus.Complete;
        game.WinningTeam = DetermineWinner(game);
    }

    private static bool GameIsComplete(Game game)
    {
        return game.Team1Score >= game.WinningScore || game.Team2Score >= game.WinningScore;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0046:Convert to conditional expression",
        Justification = "Multiple conditional expressions would be present and nested")]
    private static Team DetermineWinner(Game game)
    {
        if (game.Team1Score == game.Team2Score)
        {
            throw new InvalidOperationException(
                $"Game ended in a tie ({game.Team1Score}-{game.Team2Score}), which should not occur in Euchre");
        }

        return game.Team1Score > game.Team2Score ? Team.Team1 : Team.Team2;
    }

    private async Task ProcessDealsAsync(Game game)
    {
        for (var dealNumber = 1; dealNumber <= 100; dealNumber++)
        {
            await ProcessSingleDealAsync(game);

            if (GameIsComplete(game))
            {
                break;
            }
        }

        if (!GameIsComplete(game))
        {
            throw new InvalidOperationException("Game did not complete within the maximum number of deals allowed (100)");
        }
    }

    private async Task ProcessSingleDealAsync(Game game)
    {
        game.CurrentDeal = await dealFactory.CreateDealAsync(game, game.CompletedDeals.LastOrDefault());

        await dealOrchestrator.OrchestrateDealAsync(game.CurrentDeal);

        await gameScoreUpdater.UpdateGameScoreAsync(game, game.CurrentDeal);

        game.CompletedDeals.Add(game.CurrentDeal);
        game.CurrentDeal = null;
    }
}
