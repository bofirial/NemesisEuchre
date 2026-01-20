using System.Globalization;

using Spectre.Console;

namespace NemesisEuchre.Console;

public sealed class VersionCommand(IVersionProvider versionProvider)
{
    public void Execute()
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn("[bold]Property[/]")
            .AddColumn("[bold]Value[/]");

        _ = table.AddRow("Version", versionProvider.AssemblyInformationalVersion);
        _ = table.AddRow("Build", versionProvider.AssemblyFileVersion);
        _ = table.AddRow("Commit", versionProvider.GitCommitId[..10]);
        _ = table.AddRow("Commit Date", versionProvider.GitCommitDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        _ = table.AddRow("Configuration", versionProvider.AssemblyConfiguration);
        _ = table.AddRow("Prerelease", versionProvider.IsPrerelease ? "Yes" : "No");

        AnsiConsole.Write(table);
    }
}
