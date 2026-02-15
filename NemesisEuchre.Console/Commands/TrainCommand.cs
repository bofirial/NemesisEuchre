using DotMake.CommandLine;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Options;
using NemesisEuchre.Console.Services;
using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Options;

using Spectre.Console;

namespace NemesisEuchre.Console.Commands;

[CliCommand(
    Description = "Train ML models for Euchre decision making",
    Parent = typeof(DefaultCommand),
    Alias = "tr")]
public class TrainCommand(
    ILogger<TrainCommand> logger,
    IAnsiConsole ansiConsole,
    ITrainingProgressCoordinator progressCoordinator,
    ITrainingResultsRenderer resultsRenderer,
    IOptions<MachineLearningOptions> options) : ICliRunAsyncWithReturn
{
    [CliOption(
        Description = "Decision type to train (CallTrump, Discard, Play, All)",
        Alias = "d")]
    public DecisionType DecisionType { get; set; } = DecisionType.All;

    [CliOption(
        Description = "Load training data from IDV files with the given generation name (e.g. {gen2}_CallTrump.idv)",
        Alias = "s")]
    public required string Source { get; set; }

    [CliOption(
        Description = "Name of the model to create (e.g. {gen1}_calltrump.zip)",
        Alias = "m")]
    public required string ModelName { get; set; }

    [CliOption(
        Description = "Allow overwriting existing model files",
        Alias = "o")]
    public bool Overwrite { get; set; }

    [CliOption(Description = "Number of boosting iterations (10-500)", Alias = "i")]
    public int? NumberOfIterations { get; set; }

    [CliOption(Description = "Learning rate for gradient boosting (0.01-1.0)", Alias = "lr")]
    public double? LearningRate { get; set; }

    [CliOption(Description = "Maximum leaves per tree (2-1024)", Alias = "l")]
    public int? NumberOfLeaves { get; set; }

    [CliOption(Description = "Minimum samples per leaf node (1-1000)", Alias = "msl")]
    public int? MinimumExampleCountPerLeaf { get; set; }

    public async Task<int> RunAsync()
    {
        LoggerMessages.LogTrainingStarting(logger, DecisionType);

        var outputPath = ValidateAndPrepareOutputPath();
        if (outputPath == null)
        {
            return 1;
        }

        var validationResult = ValidateCliHyperparameters();
        if (validationResult != 0)
        {
            return validationResult;
        }

        var mergedOptions = new MergedMachineLearningOptions(
            options.Value,
            numberOfIterations: NumberOfIterations,
            learningRate: LearningRate,
            numberOfLeaves: NumberOfLeaves,
            minimumExampleCountPerLeaf: MinimumExampleCountPerLeaf);

        DisplayTrainingConfiguration(outputPath, mergedOptions.Value);

        var results = await progressCoordinator.CoordinateTrainingWithProgressAsync(
            DecisionType,
            outputPath,
            ModelName,
            ansiConsole,
            Source,
            Overwrite,
            mergedOptions);

        resultsRenderer.RenderTrainingResults(results, DecisionType);

        return DetermineExitCode(results);
    }

    private string? ValidateAndPrepareOutputPath()
    {
        var outputPath = options.Value.ModelOutputPath;

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            ansiConsole.MarkupLine("[red]Error: Model output path is not configured[/]");
            return null;
        }

        if (!Directory.Exists(outputPath))
        {
            ansiConsole.MarkupLine($"[yellow]Creating output directory: {outputPath}[/]");
            Directory.CreateDirectory(outputPath);
        }

        return outputPath;
    }

    private int ValidateCliHyperparameters()
    {
        var errors = new List<string>();

        if (NumberOfIterations.HasValue && (NumberOfIterations < 10 || NumberOfIterations > 500))
        {
            errors.Add($"--iter value {NumberOfIterations} is out of range. Valid range: 10-500");
        }

        if (LearningRate.HasValue && (LearningRate < 0.01 || LearningRate > 1.0))
        {
            errors.Add($"--lr value {LearningRate} is out of range. Valid range: 0.01-1.0");
        }

        if (NumberOfLeaves.HasValue && (NumberOfLeaves < 2 || NumberOfLeaves > 1024))
        {
            errors.Add($"--leaves value {NumberOfLeaves} is out of range. Valid range: 2-1024");
        }

        if (MinimumExampleCountPerLeaf.HasValue && (MinimumExampleCountPerLeaf < 1 || MinimumExampleCountPerLeaf > 1000))
        {
            errors.Add($"--minleaf value {MinimumExampleCountPerLeaf} is out of range. Valid range: 1-1000");
        }

        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                ansiConsole.MarkupLine($"[red]Error:[/] {error}");
            }

            return 1;
        }

        return 0;
    }

    private void DisplayTrainingConfiguration(string outputPath, MachineLearningOptions effectiveOptions)
    {
        ansiConsole.WriteLine();
        ansiConsole.MarkupLine("[bold]Configuration:[/]");
        ansiConsole.MarkupLine($"  Output: [cyan]{outputPath}[/]");
        ansiConsole.MarkupLine($"  Model Name: [cyan]{ModelName}[/]");
        ansiConsole.MarkupLine($"  Source: [cyan]{Source}[/]");
        ansiConsole.WriteLine();
        ansiConsole.MarkupLine("[bold]Hyperparameters:[/]");

        var iterSource = NumberOfIterations.HasValue ? "[yellow](CLI)[/]" : "[dim](Config)[/]";
        ansiConsole.MarkupLine($"  Number of Iterations: [cyan]{effectiveOptions.NumberOfIterations}[/] {iterSource}");

        var lrSource = LearningRate.HasValue ? "[yellow](CLI)[/]" : "[dim](Config)[/]";
        ansiConsole.MarkupLine($"  Learning Rate: [cyan]{effectiveOptions.LearningRate}[/] {lrSource}");

        var leavesSource = NumberOfLeaves.HasValue ? "[yellow](CLI)[/]" : "[dim](Config)[/]";
        ansiConsole.MarkupLine($"  Number of Leaves: [cyan]{effectiveOptions.NumberOfLeaves}[/] {leavesSource}");

        var minLeafSource = MinimumExampleCountPerLeaf.HasValue ? "[yellow](CLI)[/]" : "[dim](Config)[/]";
        ansiConsole.MarkupLine($"  Min Examples Per Leaf: [cyan]{effectiveOptions.MinimumExampleCountPerLeaf}[/] {minLeafSource}");

        ansiConsole.WriteLine();
    }

    private int DetermineExitCode(TrainingResults results)
    {
        if (results.FailedModels > 0)
        {
            LoggerMessages.LogTrainingCompletedWithFailures(logger, results.FailedModels);
            return 2;
        }

        LoggerMessages.LogTrainingCompletedSuccessfully(logger);
        return 0;
    }
}
