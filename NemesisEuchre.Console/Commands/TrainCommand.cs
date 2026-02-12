using DotMake.CommandLine;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;
using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Options;

using Spectre.Console;

namespace NemesisEuchre.Console.Commands;

[CliCommand(
    Description = "Train ML models for Euchre decision making",
    Parent = typeof(DefaultCommand))]
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

    public async Task<int> RunAsync()
    {
        LoggerMessages.LogTrainingStarting(logger, DecisionType);

        var outputPath = ValidateAndPrepareOutputPath();
        if (outputPath == null)
        {
            return 1;
        }

        DisplayTrainingConfiguration(outputPath);

        var results = await progressCoordinator.CoordinateTrainingWithProgressAsync(
            DecisionType,
            outputPath,
            ModelName,
            ansiConsole,
            Source,
            Overwrite);

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

    private void DisplayTrainingConfiguration(string outputPath)
    {
        ansiConsole.WriteLine();
        ansiConsole.MarkupLine($"[dim]Output: {outputPath}[/]");
        ansiConsole.MarkupLine($"[dim]Model Name: {ModelName}[/]");

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
