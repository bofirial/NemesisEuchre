using FluentAssertions;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;

using Spectre.Console.Testing;

namespace NemesisEuchre.Console.Tests.Services;

public class TrainingResultsRendererTests : IDisposable
{
    private readonly TestConsole _testConsole;
    private readonly TrainingResultsRenderer _renderer;
    private bool _disposed;

    public TrainingResultsRendererTests()
    {
        _testConsole = new TestConsole();
        _renderer = new TrainingResultsRenderer(_testConsole);
    }

    [Fact]
    public void BuildLiveTrainingTable_WithEmptySnapshot_ShowsPendingRows()
    {
        var snapshot = new TrainingDisplaySnapshot([], 3, 0);

        var renderable = _renderer.BuildLiveTrainingTable(snapshot, TimeSpan.FromSeconds(5));

        renderable.Should().NotBeNull();
        _testConsole.Write(renderable);
        var output = _testConsole.Output;
        output.Should().Contain("Pending");
        output.Should().Contain("Overall");
        output.Should().Contain("0/3 complete");
    }

    [Fact]
    public void BuildLiveTrainingTable_WithActiveModel_ShowsTrainingPhase()
    {
        var models = new List<ModelDisplayInfo>
        {
            new("PlayCard", TrainingPhase.Training, 50, "Iteration 100 / 200", 100, 200, null, null, null, TimeSpan.FromSeconds(10)),
        };
        var snapshot = new TrainingDisplaySnapshot(models, 3, 0);

        var renderable = _renderer.BuildLiveTrainingTable(snapshot, TimeSpan.FromSeconds(10));

        _testConsole.Write(renderable);
        var output = _testConsole.Output;
        output.Should().Contain("PlayCard");
        output.Should().Contain("Training");
        output.Should().Contain("100");
        output.Should().Contain("200");
    }

    [Fact]
    public void BuildLiveTrainingTable_WithCompletedModel_ShowsMetrics()
    {
        var models = new List<ModelDisplayInfo>
        {
            new("PlayCard", TrainingPhase.Complete, 100, "Complete", null, 200, null, 0.1892, 0.6234, TimeSpan.FromSeconds(45)),
        };
        var snapshot = new TrainingDisplaySnapshot(models, 1, 1);

        var renderable = _renderer.BuildLiveTrainingTable(snapshot, TimeSpan.FromSeconds(45));

        _testConsole.Write(renderable);
        var output = _testConsole.Output;
        output.Should().Contain("Complete");
        output.Should().Contain("0.1892");
        output.Should().Contain("0.6234");
        output.Should().Contain("1/1 complete");
    }

    [Fact]
    public void BuildLiveTrainingTable_WithFailedModel_ShowsFailedPhase()
    {
        var models = new List<ModelDisplayInfo>
        {
            new("CallTrump", TrainingPhase.Failed, 0, "Error: file not found", null, null, null, null, null, TimeSpan.FromSeconds(2)),
        };
        var snapshot = new TrainingDisplaySnapshot(models, 1, 1);

        var renderable = _renderer.BuildLiveTrainingTable(snapshot, TimeSpan.FromSeconds(2));

        _testConsole.Write(renderable);
        var output = _testConsole.Output;
        output.Should().Contain("Failed");
    }

    [Fact]
    public void BuildLiveTrainingTable_PendingRowsForUnstartedModels()
    {
        var models = new List<ModelDisplayInfo>
        {
            new("CallTrump", TrainingPhase.Training, 50, "Training...", 100, 200, null, null, null, TimeSpan.FromSeconds(10)),
        };
        var snapshot = new TrainingDisplaySnapshot(models, 3, 0);

        var renderable = _renderer.BuildLiveTrainingTable(snapshot, TimeSpan.FromSeconds(10));

        _testConsole.Write(renderable);
        var output = _testConsole.Output;
        output.Should().Contain("Pending");
    }

    [Fact]
    public void BuildLiveTrainingTable_ShowsTotalElapsedTime()
    {
        var models = new List<ModelDisplayInfo>
        {
            new("PlayCard", TrainingPhase.Complete, 100, "Complete", null, null, null, null, null, TimeSpan.FromSeconds(45)),
        };
        var snapshot = new TrainingDisplaySnapshot(models, 1, 1);

        var renderable = _renderer.BuildLiveTrainingTable(snapshot, TimeSpan.FromMinutes(1.5));

        _testConsole.Write(renderable);
        var output = _testConsole.Output;
        output.Should().Contain("1m");
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _testConsole.Dispose();
            }

            _disposed = true;
        }
    }
}
