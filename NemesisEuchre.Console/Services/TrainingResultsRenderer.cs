using Humanizer;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Foundation.Constants;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace NemesisEuchre.Console.Services;

public interface ITrainingResultsRenderer
{
    void RenderTrainingResults(TrainingResults results, DecisionType decisionType);

    IRenderable BuildLiveTrainingTable(TrainingDisplaySnapshot snapshot, TimeSpan totalElapsed);
}

public class TrainingResultsRenderer(IAnsiConsole console) : ITrainingResultsRenderer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "Nested Conditional")]
    public void RenderTrainingResults(TrainingResults results, DecisionType decisionType)
    {
        console.WriteLine();
        console.Write(new Rule($"[yellow]Training Results - ({decisionType})[/]").LeftJustified());
        console.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[bold]Model Type[/]").Centered())
            .AddColumn(new TableColumn("[bold]Status[/]").Centered())
            .AddColumn(new TableColumn("[bold]MAE[/]").Centered())
            .AddColumn(new TableColumn("[bold]R²[/]").Centered())
            .AddColumn(new TableColumn("[bold]Duration[/]").Centered())
            .AddColumn(new TableColumn("[bold]Model Path[/]"));

        foreach (var result in results.Results.OrderBy(r => r.ModelType))
        {
            var statusMarkup = result.Success
                ? "[green]✓ Success[/]"
                : "[red]✗ Failed[/]";

            var maeMarkup = result.MeanAbsoluteError.HasValue
                ? $"{result.MeanAbsoluteError.Value:F4}"
                : "[dim]-[/]";

            var rSquaredMarkup = result.RSquared.HasValue
                ? $"{result.RSquared.Value:F4}"
                : "[dim]-[/]";

            string pathMarkup;
            if (result.Success && result.ModelPath != null)
            {
                pathMarkup = $"[dim]{result.ModelPath}[/]";
            }
            else if (result.ErrorMessage != null)
            {
                pathMarkup = $"[red]{result.ErrorMessage}[/]";
            }
            else
            {
                pathMarkup = "[dim]-[/]";
            }

            var durationMarkup = RenderingUtilities.FormatNullableDuration(result.Duration);

            table.AddRow(
                result.ModelType,
                statusMarkup,
                maeMarkup,
                rSquaredMarkup,
                durationMarkup,
                pathMarkup);
        }

        console.Write(table);
        console.WriteLine();

        var summaryColor = results.FailedModels == 0 ? "green" : "yellow";
        console.MarkupLine(
            $"[{summaryColor}]Training Summary: {results.SuccessfulModels} succeeded, {results.FailedModels} failed[/]");
        console.MarkupLine($"[dim]Duration: {results.TotalDuration.Humanize(2, countEmptyUnits: true, minUnit: TimeUnit.Second)}[/]");
        console.WriteLine();
    }

    public IRenderable BuildLiveTrainingTable(TrainingDisplaySnapshot snapshot, TimeSpan totalElapsed)
    {
        var table = CreateStyledTable()
            .AddColumn(new TableColumn("[bold]Model[/]").Centered())
            .AddColumn(new TableColumn("[bold]Phase[/]").Centered())
            .AddColumn(new TableColumn("[bold]MAE[/]").Centered())
            .AddColumn(new TableColumn("[bold]R²[/]").Centered())
            .AddColumn(new TableColumn("[bold]Elapsed[/]").Centered());

        foreach (var model in snapshot.Models)
        {
            var phaseDisplay = model.Phase switch
            {
                TrainingPhase.LoadingData => "[blue]📊 Loading[/]",
                TrainingPhase.Training => "[yellow]🔄 Training[/]",
                TrainingPhase.Saving => "[blue]💾 Saving[/]",
                TrainingPhase.Complete => "[green]✓ Complete[/]",
                TrainingPhase.Failed => "[red]✗ Failed[/]",
                _ => "[dim]-[/]",
            };

            var maeDisplay = RenderingUtilities.FormatNullableMetric(model.ValidationMae);

            var rSquaredDisplay = RenderingUtilities.FormatNullableMetric(model.ValidationRSquared);

            var elapsedDisplay = model.Elapsed.TotalSeconds >= 0.1
                ? model.Elapsed.Humanize(2, countEmptyUnits: true, minUnit: TimeUnit.Second)
                : "[dim]-[/]";

            table.AddRow(model.ModelType, phaseDisplay, maeDisplay, rSquaredDisplay, elapsedDisplay);
        }

        for (var i = snapshot.Models.Count; i < snapshot.TotalModels; i++)
        {
            table.AddRow("[dim]—[/]", "[dim]⏳ Pending[/]", "[dim]-[/]", "[dim]-[/]", "[dim]-[/]");
        }

        var overallProgress = $"{snapshot.CompletedModels}/{snapshot.TotalModels} complete";
        table.AddRow("[bold]Overall[/]", $"[bold]{overallProgress}[/]", string.Empty, string.Empty, $"[bold]{totalElapsed.Humanize(2, countEmptyUnits: true, minUnit: TimeUnit.Second)}[/]");

        return new Rows(new Markup("[bold yellow]Training Models (Live)[/]"), new Text(string.Empty), table);
    }

    private static Table CreateStyledTable()
    {
        return RenderingUtilities.CreateStyledTable();
    }
}
