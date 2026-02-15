using System.Diagnostics;

using Microsoft.Extensions.Options;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Options;

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
        IOptions<MachineLearningOptions>? optionsOverride = null,
        CancellationToken cancellationToken = default);
}

public class TrainingProgressCoordinator(
    IModelTrainingOrchestrator trainingOrchestrator,
    ITrainingResultsRenderer trainingResultsRenderer) : ITrainingProgressCoordinator
{
    public Task<TrainingResults> CoordinateTrainingWithProgressAsync(
        DecisionType decisionType,
        string outputPath,
        string modelName,
        IAnsiConsole console,
        string idvName,
        bool allowOverwrite = false,
        IOptions<MachineLearningOptions>? optionsOverride = null,
        CancellationToken cancellationToken = default)
    {
        var totalModels = decisionType == DecisionType.All ? 3 : 1;
        var displayState = new TrainingDisplayState(totalModels);
        var stopwatch = Stopwatch.StartNew();

        return console.Live(new Text(string.Empty))
            .AutoClear(true)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {
                var initialSnapshot = new TrainingDisplaySnapshot([], totalModels, 0);
                ctx.UpdateTarget(trainingResultsRenderer.BuildLiveTrainingTable(initialSnapshot, stopwatch.Elapsed));

                var progress = new Progress<TrainingProgress>(displayState.Update);

                var trainingTask = trainingOrchestrator.TrainModelsAsync(
                    decisionType,
                    outputPath,
                    modelName,
                    progress,
                    idvName,
                    allowOverwrite,
                    optionsOverride,
                    cancellationToken);

                while (!trainingTask.IsCompleted)
                {
                    displayState.RefreshSnapshot();
                    var snapshot = displayState.LatestSnapshot;
                    if (snapshot != null)
                    {
                        ctx.UpdateTarget(
                            trainingResultsRenderer.BuildLiveTrainingTable(snapshot, stopwatch.Elapsed));
                    }

                    await Task.WhenAny(trainingTask, Task.Delay(250, CancellationToken.None));
                }

                return await trainingTask;
            });
    }
}
