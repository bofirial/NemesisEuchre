using DotMake.CommandLine;

using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Services;
using NemesisEuchre.Foundation;

using Spectre.Console;

namespace NemesisEuchre.Console.Commands;

[CliCommand(
    Description = "Merge multiple IDV generation files into a single combined file",
    Parent = typeof(DefaultCommand),
    Alias = "mg")]
public class MergeCommand(
    ILogger<MergeCommand> logger,
    IAnsiConsole ansiConsole,
    IIdvMergeService mergeService) : ICliRunAsyncWithReturn
{
    [CliOption(Description = "Source generation name (specify multiple times)", Alias = "s")]
    public required IList<string> Source { get; set; }

    [CliOption(Description = "Output generation name", Alias = "o")]
    public required string Output { get; set; }

    [CliOption(Description = "Overwrite existing output files if they exist")]
    public bool Overwrite { get; set; }

    public async Task<int> RunAsync()
    {
        if (Source.Count < 2)
        {
            ansiConsole.MarkupLine("[red]Error:[/] At least 2 source generation names are required.");
            return 1;
        }

        try
        {
            await ansiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(
                    "Merging IDV files...",
                    async ctx => await mergeService.MergeAsync(
                            [.. Source],
                            Output,
                            Overwrite,
                            status => ctx.Status(status)));

            ansiConsole.MarkupLine($"[green]Successfully merged {Source.Count} source(s) into '{Output}'.[/]");
            return 0;
        }
        catch (FileNotFoundException ex)
        {
            ansiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return 1;
        }
        catch (InvalidOperationException ex)
        {
            ansiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            LoggerMessages.LogMergeCommandFailed(logger, ex);
            ansiConsole.MarkupLine("[red]Error:[/] An unexpected error occurred during merge.");
            return 1;
        }
    }
}
