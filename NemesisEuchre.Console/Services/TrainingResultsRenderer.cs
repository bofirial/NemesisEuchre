using NemesisEuchre.Console.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

using Spectre.Console;

namespace NemesisEuchre.Console.Services;

public interface ITrainingResultsRenderer
{
    void RenderTrainingResults(TrainingResults results, ActorType actorType, DecisionType decisionType);
}

public class TrainingResultsRenderer(IAnsiConsole console) : ITrainingResultsRenderer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "Nested Conditional")]
    public void RenderTrainingResults(TrainingResults results, ActorType actorType, DecisionType decisionType)
    {
        console.WriteLine();
        console.Write(new Rule($"[yellow]Training Results - {actorType} ({decisionType})[/]").LeftJustified());
        console.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[bold]Model Type[/]").Centered())
            .AddColumn(new TableColumn("[bold]Status[/]").Centered())
            .AddColumn(new TableColumn("[bold]MAE[/]").Centered())
            .AddColumn(new TableColumn("[bold]R²[/]").Centered())
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

            table.AddRow(
                result.ModelType,
                statusMarkup,
                maeMarkup,
                rSquaredMarkup,
                pathMarkup);
        }

        console.Write(table);
        console.WriteLine();

        var summaryColor = results.FailedModels == 0 ? "green" : "yellow";
        console.MarkupLine(
            $"[{summaryColor}]Training Summary: {results.SuccessfulModels} succeeded, {results.FailedModels} failed[/]");
        console.MarkupLine($"[dim]Duration: {results.TotalDuration.TotalSeconds:F1}s[/]");
        console.WriteLine();
    }
}
