using System.Collections.Concurrent;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Foundation.Constants;

using Spectre.Console;

namespace NemesisEuchre.Console.Services;

public interface ITrainingProgressCoordinator
{
    Task<TrainingResults> CoordinateTrainingWithProgressAsync(
        DecisionType decisionType,
        string outputPath,
        string modelName,
        IAnsiConsole console,
        string idvName,
        bool allowOverwrite = false,
        CancellationToken cancellationToken = default);
}

public class TrainingProgressCoordinator(IModelTrainingOrchestrator trainingOrchestrator) : ITrainingProgressCoordinator
{
    public Task<TrainingResults> CoordinateTrainingWithProgressAsync(
        DecisionType decisionType,
        string outputPath,
        string modelName,
        IAnsiConsole console,
        string idvName,
        bool allowOverwrite = false,
        CancellationToken cancellationToken = default)
    {
        return console.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new ElapsedTimeColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var overallTask = ctx.AddTask(
                    $"[green]Training {decisionType} models[/]",
                    maxValue: 100);

                var modelTasks = new ConcurrentDictionary<string, ProgressTask>();

                var progress = new Progress<TrainingProgress>(p =>
                {
                    var task = modelTasks.GetOrAdd(
                        p.ModelType,
                        key => ctx.AddTask($"[blue]{key}[/]", maxValue: 100));

                    task.Value = p.PercentComplete;

                    var statusEmoji = p.Phase switch
                    {
                        TrainingPhase.LoadingData => "📊",
                        TrainingPhase.Training => "🔄",
                        TrainingPhase.Saving => "💾",
                        TrainingPhase.Complete => "✓",
                        TrainingPhase.Failed => "✗",
                        _ => string.Empty,
                    };

                    var message = p.Message ?? p.Phase.ToString();
                    task.Description = $"[blue]{p.ModelType}[/] {statusEmoji} {message}";

                    if (p.Phase is TrainingPhase.Complete or TrainingPhase.Failed)
                    {
                        task.StopTask();
                    }

                    var completedModels = modelTasks.Values.Count(t => t.IsFinished);
                    var totalModels = decisionType == DecisionType.All ? 3 : 1;
                    overallTask.Value = completedModels * 100.0 / totalModels;
                });

                return await trainingOrchestrator.TrainModelsAsync(
                    decisionType,
                    outputPath,
                    modelName,
                    progress,
                    idvName,
                    allowOverwrite);
            });
    }
}
