using DotMake.CommandLine;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Options;

using Spectre.Console;

namespace NemesisEuchre.Console.Commands;

[CliCommand(
    Description = "Train ML models for Euchre decision making",
    Parent = typeof(DefaultCommand))]
public class TrainCommand(
    ILogger<TrainCommand> logger,
    IAnsiConsole ansiConsole,
    IModelTrainingOrchestrator trainingOrchestrator,
    ITrainingResultsRenderer resultsRenderer,
    IOptions<MachineLearningOptions> options) : ICliRunAsyncWithReturn
{
    [CliOption(Description = "Actor type to train models for")]
    public required ActorType ActorType { get; set; }

    [CliOption(Description = "Decision type to train (CallTrump, Discard, Play, All)")]
    public DecisionType DecisionType { get; set; } = DecisionType.All;

    [CliOption(Description = "Output path for trained models")]
    public string? OutputPath { get; set; }

    [CliOption(Description = "Maximum training samples (0 = unlimited)")]
    public int SampleLimit { get; set; }

    [CliOption(Description = "Generation number for models")]
    public int Generation { get; set; } = 1;

    public async Task<int> RunAsync()
    {
        LoggerMessages.LogTrainingStarting(logger, ActorType, DecisionType, Generation);

        var outputPath = OutputPath ?? options.Value.ModelOutputPath;

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            ansiConsole.MarkupLine("[red]Error: Model output path is not configured[/]");
            return 1;
        }

        if (!Directory.Exists(outputPath))
        {
            ansiConsole.MarkupLine($"[yellow]Creating output directory: {outputPath}[/]");
            Directory.CreateDirectory(outputPath);
        }

        ansiConsole.WriteLine();
        ansiConsole.MarkupLine($"[dim]Output: {outputPath}[/]");
        ansiConsole.MarkupLine($"[dim]Generation: {Generation}[/]");

        if (SampleLimit > 0)
        {
            ansiConsole.MarkupLine($"[dim]Sample Limit: {SampleLimit:N0}[/]");
        }

        ansiConsole.WriteLine();

        var results = await ansiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var overallTask = ctx.AddTask(
                    $"[green]Training {DecisionType} models for {ActorType}[/]",
                    maxValue: 100);

                var modelTasks = new Dictionary<string, ProgressTask>();

                var progress = new Progress<TrainingProgress>(p =>
                {
                    if (!modelTasks.TryGetValue(p.ModelType, out var task))
                    {
                        task = ctx.AddTask($"[blue]{p.ModelType}[/]", maxValue: 100);
                        modelTasks[p.ModelType] = task;
                    }

                    task.Value = p.PercentComplete;

                    var statusEmoji = p.Phase switch
                    {
                        TrainingPhase.LoadingData => "📊",
                        TrainingPhase.Training => "🔄",
                        TrainingPhase.Saving => "💾",
                        TrainingPhase.Complete => "✓",
                        TrainingPhase.Failed => "✗",
                        _ => string.Empty
                    };

                    var message = p.Message ?? p.Phase.ToString();
                    task.Description = $"[blue]{p.ModelType}[/] {statusEmoji} {message}";

                    if (p.Phase is TrainingPhase.Complete or TrainingPhase.Failed)
                    {
                        task.StopTask();
                    }

                    var completedModels = modelTasks.Values.Count(t => t.IsFinished);
                    var totalModels = DecisionType == DecisionType.All ? 3 : 1;
                    overallTask.Value = completedModels * 100.0 / totalModels;
                });

                return await trainingOrchestrator.TrainModelsAsync(
                    ActorType,
                    DecisionType,
                    outputPath,
                    SampleLimit,
                    Generation,
                    progress);
            });

        resultsRenderer.RenderTrainingResults(results, ActorType, DecisionType);

        if (results.FailedModels > 0)
        {
            LoggerMessages.LogTrainingCompletedWithFailures(logger, results.FailedModels);
            return 2;
        }

        LoggerMessages.LogTrainingCompletedSuccessfully(logger);
        return 0;
    }
}
