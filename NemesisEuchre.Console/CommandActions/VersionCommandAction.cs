using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;

using Spectre.Console;

namespace NemesisEuchre.Console.CommandActions;

public sealed class VersionCommandAction() : SynchronousCommandLineAction
{
    public override int Invoke(ParseResult parseResult)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn("[bold]Property[/]")
            .AddColumn("[bold]Value[/]");

        table.AddRow("Version", ThisAssembly.AssemblyInformationalVersion);
        table.AddRow("Build", ThisAssembly.AssemblyFileVersion);
        table.AddRow("Commit", ThisAssembly.GitCommitId[..10]);
        table.AddRow("Commit Date", ThisAssembly.GitCommitDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        table.AddRow("Configuration", ThisAssembly.AssemblyConfiguration);
        table.AddRow("Prerelease", ThisAssembly.IsPrerelease ? "Yes" : "No");

        AnsiConsole.Write(
            new FigletText("NemesisEuchre")
                .Centered()
                .Color(Color.Blue));

        AnsiConsole.Write(table);

        return 0;
    }
}
