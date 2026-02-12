using DotMake.CommandLine;

using NemesisEuchre.Console.Services;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.DataAccess.Repositories;

using Spectre.Console;

namespace NemesisEuchre.Console.Commands;

[CliCommand(
    Description = "Display results of a previously played game",
    Parent = typeof(DefaultCommand))]
public class ShowGameCommand(
    IAnsiConsole ansiConsole,
    IApplicationBanner applicationBanner,
    IGameRepository gameRepository,
    IEntityToGameMapper gameMapper,
    IGameResultsRenderer gameResultsRenderer) : ICliRunAsyncWithReturn
{
    [CliOption(
        Description = "The ID of the game to display",
        Alias = "g",
        Required = true)]
    public int GameId { get; set; }

    [CliOption(
        Description = "Show decisions made during the game",
        Alias = "s")]
    public bool ShowDecisions { get; set; }

    public async Task<int> RunAsync()
    {
        applicationBanner.Display();

        var gameEntity = await ansiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Loading game...", async _ =>
                await gameRepository.GetGameByIdAsync(GameId, ShowDecisions).ConfigureAwait(false));

        if (gameEntity is null)
        {
            ansiConsole.MarkupLine($"[red]Game with ID {GameId} was not found.[/]");
            return 1;
        }

        var game = gameMapper.Map(gameEntity, ShowDecisions);

        gameResultsRenderer.RenderResults(game, ShowDecisions);

        return 0;
    }
}
