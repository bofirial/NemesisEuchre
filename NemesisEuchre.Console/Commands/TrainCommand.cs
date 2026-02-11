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
    [CliOption(Description = "Decision type to train (CallTrump, Discard, Play, All)")]
    public DecisionType DecisionType { get; set; } = DecisionType.All;

    [CliOption(Description = "Output path for trained models", Required = false)]
    public string? OutputPath { get; set; }

    [CliOption(Description = "Maximum training samples (0 = unlimited)")]
    public int SampleLimit { get; set; }

    [CliOption(Description = "Generation number for models")]
    public int Generation { get; set; } = 1;

    [CliOption(Description = "Load training data from IDV files with the given generation name (e.g. gen2)")]
    public string? IdvName { get; set; }

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
            SampleLimit,
            Generation,
            ansiConsole,
            IdvName);

        resultsRenderer.RenderTrainingResults(results, DecisionType);

        return DetermineExitCode(results);
    }

    private string? ValidateAndPrepareOutputPath()
    {
        var outputPath = OutputPath ?? options.Value.ModelOutputPath;

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
        ansiConsole.MarkupLine($"[dim]Generation: {Generation}[/]");

        if (SampleLimit > 0)
        {
            ansiConsole.MarkupLine($"[dim]Sample Limit: {SampleLimit:N0}[/]");
        }

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
