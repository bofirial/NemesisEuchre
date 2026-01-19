using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace NemesisEuchre.Console;

internal sealed class NemesisEuchreApplication(
    ILogger<NemesisEuchreApplication> logger,
    IHostApplicationLifetime applicationLifetime) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() => RunApplication(cancellationToken), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        NemesisEuchreApplicationLogMessages.LogApplicationShuttingDown(logger);
        return Task.CompletedTask;
    }

    private void RunApplication(CancellationToken cancellationToken)
    {
        try
        {
            NemesisEuchreApplicationLogMessages.LogStartingUp(logger);

            AnsiConsole.Write(
                new FigletText("NemesisEuchre")
                    .Centered()
                    .Color(Color.Blue));

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green]Welcome to NemesisEuchre - AI-Powered Euchre Strategy[/]");
            AnsiConsole.WriteLine();

            NemesisEuchreApplicationLogMessages.LogConfigurationLoaded(logger);

            AnsiConsole.MarkupLine("[yellow]Application infrastructure is ready.[/]");
            AnsiConsole.MarkupLine("[dim]Press Ctrl+C to exit[/]");

            _ = cancellationToken.WaitHandle.WaitOne();
        }
        catch (Exception ex)
        {
            NemesisEuchreApplicationLogMessages.LogApplicationError(logger, ex);
        }
        finally
        {
            // Stop the application
            applicationLifetime.StopApplication();
        }
    }
}
