using System.CommandLine;
using System.CommandLine.Invocation;

using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace NemesisEuchre.Console.CommandActions;

public class DefaultCommandAction(ILogger<DefaultCommandAction> logger) : SynchronousCommandLineAction
{
    public override int Invoke(ParseResult parseResult)
    {
        try
        {
            LoggerMessages.LogStartingUp(logger);

            AnsiConsole.Write(
                new FigletText("NemesisEuchre")
                    .Centered()
                    .Color(Color.Blue));

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green]Welcome to NemesisEuchre - AI-Powered Euchre Strategy[/]");
            AnsiConsole.WriteLine();

            LoggerMessages.LogConfigurationLoaded(logger);

            AnsiConsole.MarkupLine("[yellow]Application infrastructure is ready.[/]");
        }
        catch (Exception ex)
        {
            LoggerMessages.LogApplicationError(logger, ex);

            return ex.HResult;
        }

        return 0;
    }
}
