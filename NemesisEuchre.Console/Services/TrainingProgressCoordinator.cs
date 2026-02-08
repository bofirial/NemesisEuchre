using NemesisEuchre.Console.Models;
using NemesisEuchre.Foundation.Constants;

using Spectre.Console;

namespace NemesisEuchre.Console.Services;

public interface ITrainingProgressCoordinator
{
    Task<TrainingResults> CoordinateTrainingWithProgressAsync(
        ActorType actorType,
        DecisionType decisionType,
        string outputPath,
        int sampleLimit,
        int generation,
        IAnsiConsole console,
        CancellationToken cancellationToken = default);
}

public class TrainingProgressCoordinator(IModelTrainingOrchestrator trainingOrchestrator) : ITrainingProgressCoordinator
{
    public Task<TrainingResults> CoordinateTrainingWithProgressAsync(
        ActorType actorType,
        DecisionType decisionType,
        string outputPath,
        int sampleLimit,
        int generation,
        IAnsiConsole console,
        CancellationToken cancellationToken = default)
    {
        return console.Progress()
            .StartAsync(async ctx =>
            {
                var overallTask = ctx.AddTask(
                    $"[green]Training {decisionType} models for {actorType}[/]",
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
                    actorType,
                    decisionType,
                    outputPath,
                    sampleLimit,
                    generation,
                    progress);
            });
    }
}
