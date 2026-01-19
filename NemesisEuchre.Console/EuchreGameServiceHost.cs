using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace NemesisEuchre.Console;

public sealed class EuchreGameServiceHost(
    ILogger<EuchreGameServiceHost> logger,
    IHostApplicationLifetime applicationLifetime) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() => RunApplication(cancellationToken), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        LoggerMessages.LogApplicationShuttingDown(logger);
        return Task.CompletedTask;
    }

    private void RunApplication(CancellationToken cancellationToken)
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
            AnsiConsole.MarkupLine("[dim]Press Ctrl+C to exit[/]");

            _ = cancellationToken.WaitHandle.WaitOne();
        }
        catch (Exception ex)
        {
            LoggerMessages.LogApplicationError(logger, ex);
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }
}
