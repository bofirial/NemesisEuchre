using System.Collections.Concurrent;
using System.Diagnostics;

using NemesisEuchre.Console.Models;

namespace NemesisEuchre.Console.Services;

internal sealed class TrainingDisplayState(int totalModels)
{
    private readonly ConcurrentDictionary<string, ModelState> _models = new();
    private volatile TrainingDisplaySnapshot? _latestSnapshot;

    public TrainingDisplaySnapshot? LatestSnapshot => _latestSnapshot;

    public void Update(TrainingProgress progress)
    {
        var state = _models.GetOrAdd(progress.ModelType, _ => new ModelState());

        state.Phase = progress.Phase;
        state.PercentComplete = progress.PercentComplete;
        state.Message = progress.Message;
        state.CurrentIteration = progress.CurrentIteration;
        state.TotalIterations = progress.TotalIterations;
        state.TrainingMetric = progress.TrainingMetric;

        if (progress.ValidationMae.HasValue)
        {
            state.ValidationMae = progress.ValidationMae;
        }

        if (progress.ValidationRSquared.HasValue)
        {
            state.ValidationRSquared = progress.ValidationRSquared;
        }

        if (!state.TimerStarted)
        {
            state.Stopwatch.Start();
            state.TimerStarted = true;
        }

        if (progress.Phase is TrainingPhase.Complete or TrainingPhase.Failed)
        {
            state.Stopwatch.Stop();
        }

        RebuildSnapshot();
    }

    public void RefreshSnapshot()
    {
        if (!_models.IsEmpty)
        {
            RebuildSnapshot();
        }
    }

    public void SetValidationMetrics(string modelType, double? mae, double? rSquared)
    {
        if (_models.TryGetValue(modelType, out var state))
        {
            state.ValidationMae = mae;
            state.ValidationRSquared = rSquared;
            RebuildSnapshot();
        }
    }

    private void RebuildSnapshot()
    {
        var models = _models
            .Select(kvp => new ModelDisplayInfo(
                kvp.Key,
                kvp.Value.Phase,
                kvp.Value.PercentComplete,
                kvp.Value.Message,
                kvp.Value.CurrentIteration,
                kvp.Value.TotalIterations,
                kvp.Value.TrainingMetric,
                kvp.Value.ValidationMae,
                kvp.Value.ValidationRSquared,
                kvp.Value.Stopwatch.Elapsed))
            .OrderBy(m => m.ModelType)
            .ToList();

        var completedModels = models.Count(m => m.Phase is TrainingPhase.Complete or TrainingPhase.Failed);

        _latestSnapshot = new TrainingDisplaySnapshot(models, totalModels, completedModels);
    }

    private sealed class ModelState
    {
        public Stopwatch Stopwatch { get; } = new();

        public bool TimerStarted { get; set; }

        public TrainingPhase Phase { get; set; }

        public int PercentComplete { get; set; }

        public string? Message { get; set; }

        public int? CurrentIteration { get; set; }

        public int? TotalIterations { get; set; }

        public double? TrainingMetric { get; set; }

        public double? ValidationMae { get; set; }

        public double? ValidationRSquared { get; set; }
    }
}
