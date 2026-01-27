using DotMake.CommandLine;

using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Services;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.GameEngine;
using NemesisEuchre.GameEngine.Models;

using Spectre.Console;

namespace NemesisEuchre.Console.Commands;

[CliCommand(Description = "Nemesis Euchre")]
public class DefaultCommand(
    ILogger<DefaultCommand> logger,
    IAnsiConsole ansiConsole,
    IApplicationBanner applicationBanner,
    IGameOrchestrator gameOrchestrator,
    IGameRepository gameRepository,
    IGameResultsRenderer gameResultsRenderer) : ICliRunAsyncWithReturn
{
    public async Task<int> RunAsync()
    {
        LoggerMessages.LogStartingUp(logger);

        applicationBanner.Display();

        ansiConsole.MarkupLine("[green]Welcome to NemesisEuchre - AI-Powered Euchre Strategy[/]");
        ansiConsole.MarkupLine("[dim]Playing a game between 4 ChaosBots...[/]");
        ansiConsole.WriteLine();

        var game = await ansiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Playing game...", async _ => await gameOrchestrator.OrchestrateGameAsync());

        await PersistCompletedGameAsync(game);

        gameResultsRenderer.RenderResults(game);

        return 0;
    }

    private async Task PersistCompletedGameAsync(Game game)
    {
        try
        {
            LoggerMessages.LogPersistingCompletedGame(logger, game.GameStatus);

            var gameId = await gameRepository.SaveCompletedGameAsync(game);

            LoggerMessages.LogGamePersistedSuccessfully(logger, gameId);
        }
        catch (Exception ex)
        {
            LoggerMessages.LogGamePersistenceFailed(logger, ex);
        }
    }
}
