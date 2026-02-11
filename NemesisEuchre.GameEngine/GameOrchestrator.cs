using Microsoft.Extensions.Options;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Options;

namespace NemesisEuchre.GameEngine;

public interface IGameOrchestrator
{
    Task<Game> OrchestrateGameAsync(Actor[]? team1Actors = null, Actor[]? team2Actors = null);
}

public class GameOrchestrator(
    IGameFactory gameFactory,
    IDealFactory dealFactory,
    IDealOrchestrator dealOrchestrator,
    IGameScoreUpdater gameScoreUpdater,
    IGameWinnerCalculator gameWinnerCalculator,
    IOptions<GameOptions> gameOptions) : IGameOrchestrator
{
    private const int MaxDealsPerGame = 100;

    public async Task<Game> OrchestrateGameAsync(Actor[]? team1Actors = null, Actor[]? team2Actors = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(gameOptions.Value.WinningScore, 1);

        var game = await gameFactory.CreateGameAsync(team1Actors, team2Actors).ConfigureAwait(false);

        InitializeGame(game);

        await ProcessDealsAsync(game).ConfigureAwait(false);

        FinalizeGame(game);

        return game;
    }

    private static void InitializeGame(Game game)
    {
        game.GameStatus = GameStatus.Playing;
    }

    private void FinalizeGame(Game game)
    {
        game.GameStatus = GameStatus.Complete;
        game.WinningTeam = gameWinnerCalculator.DetermineWinner(game);
    }

    private async Task ProcessDealsAsync(Game game)
    {
        for (var dealNumber = 1; dealNumber <= MaxDealsPerGame; dealNumber++)
        {
            await ProcessSingleDealAsync(game).ConfigureAwait(false);

            if (GameIsComplete(game))
            {
                break;
            }
        }

        if (!GameIsComplete(game))
        {
            throw new InvalidOperationException($"Game did not complete within the maximum number of deals allowed ({MaxDealsPerGame})");
        }
    }

    private async Task ProcessSingleDealAsync(Game game)
    {
        game.CurrentDeal = await dealFactory.CreateDealAsync(game, game.CompletedDeals.LastOrDefault()).ConfigureAwait(false);
        game.CurrentDeal.DealNumber = (short)(game.CompletedDeals.Count + 1);

        await dealOrchestrator.OrchestrateDealAsync(game.CurrentDeal).ConfigureAwait(false);

        await gameScoreUpdater.UpdateGameScoreAsync(game, game.CurrentDeal).ConfigureAwait(false);

        game.CurrentDeal.Team1Score = game.Team1Score;
        game.CurrentDeal.Team2Score = game.Team2Score;

        game.CompletedDeals.Add(game.CurrentDeal);
        game.CurrentDeal = null;
    }

    private bool GameIsComplete(Game game)
    {
        return game.Team1Score >= gameOptions.Value.WinningScore || game.Team2Score >= gameOptions.Value.WinningScore;
    }
}
