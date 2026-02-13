using NemesisEuchre.Console.Models.BehavioralTests;

using Spectre.Console;

namespace NemesisEuchre.Console.Services.BehavioralTests;

public interface ITestResultsRenderer
{
    void RenderResults(BehavioralTestSuiteResult suiteResult);
}

public class TestResultsRenderer(IAnsiConsole console) : ITestResultsRenderer
{
    public void RenderResults(BehavioralTestSuiteResult suiteResult)
    {
        console.WriteLine();

        RenderFailureDetails(suiteResult);
        console.WriteLine();

        console.Write(new Rule($"[yellow]Behavioral Test Results - {suiteResult.ModelName}[/]"));
        console.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[bold]Type[/]").Centered())
            .AddColumn(new TableColumn("[bold]Scenario[/]"))
            .AddColumn(new TableColumn("[bold]Result[/]").Centered())
            .AddColumn(new TableColumn("[bold]Model Choice[/]"))
            .AddColumn(new TableColumn("[bold]Expected[/]"));

        foreach (var result in suiteResult.Results)
        {
            var resultMarkup = result.Passed
                ? "[green]Pass[/]"
                : "[red]Fail[/]";

            table.AddRow(
                result.DecisionType.ToString(),
                result.TestName,
                resultMarkup,
                Markup.Escape(result.ChosenOptionDisplay),
                result.AssertionDescription);
        }

        console.Write(Align.Center(table));
        console.WriteLine();

        var passed = suiteResult.Results.Count(r => r.Passed);
        var failed = suiteResult.Results.Count(r => !r.Passed);
        var summaryColor = failed == 0 ? "green" : "red";

        console.Write(Align.Center(new Markup(
            $"[{summaryColor}]Summary: {passed} passed, {failed} failed out of {suiteResult.Results.Count} tests[/]")));

        console.Write(Align.Center(new Markup($"[dim]Duration: {suiteResult.Duration.TotalSeconds:F1}s[/]")));
    }

    private void RenderFailureDetails(BehavioralTestSuiteResult suiteResult)
    {
        var failures = suiteResult.Results.Where(r => !r.Passed).ToList();
        if (failures.Count == 0)
        {
            return;
        }

        foreach (var failure in failures)
        {
            console.MarkupLine($"[red]FAILED: {Markup.Escape(failure.TestName)}[/]");

            if (failure.FailureReason != null)
            {
                console.MarkupLine($"  [dim]{Markup.Escape(failure.FailureReason)}[/]");
            }

            if (failure.OptionScores.Count > 0)
            {
                console.MarkupLine("  [dim]Scores:[/]");

                foreach (var (option, score) in failure.OptionScores.OrderByDescending(s => s.Value))
                {
                    var marker = option == failure.ChosenOptionDisplay ? " [yellow]<- chosen[/]" : string.Empty;
                    console.MarkupLine($"    {Markup.Escape(option),-35} -> {score,8:F4}{marker}");
                }
            }

            console.WriteLine();
        }
    }
}
