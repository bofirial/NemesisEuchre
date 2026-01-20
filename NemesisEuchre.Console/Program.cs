using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NemesisEuchre.Console;

public static class Program
{
    private static Task Main(string[] args)
    {
        if (args.Length > 0 && args[0].Equals("version", StringComparison.OrdinalIgnoreCase))
        {
            var versionProvider = new GitVersionProvider();
            var versionCommand = new VersionCommand(versionProvider);
            versionCommand.Execute();
            return Task.CompletedTask;
        }

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, config) =>
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args))
            .ConfigureServices((_, services) =>
                services.AddHostedService<EuchreGameServiceHost>())
            .Build();

        return host.RunAsync();
    }
}
