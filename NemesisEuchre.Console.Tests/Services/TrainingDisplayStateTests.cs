using FluentAssertions;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;

namespace NemesisEuchre.Console.Tests.Services;

public class TrainingDisplayStateTests
{
    [Fact]
    public void LatestSnapshot_InitiallyNull()
    {
        var state = new TrainingDisplayState(3);

        state.LatestSnapshot.Should().BeNull();
    }

    [Fact]
    public void Update_FirstProgress_CreatesSnapshot()
    {
        var state = new TrainingDisplayState(3);

        state.Update(new TrainingProgress("PlayCard", TrainingPhase.LoadingData, 0, "Loading..."));

        state.LatestSnapshot.Should().NotBeNull();
        state.LatestSnapshot!.TotalModels.Should().Be(3);
        state.LatestSnapshot.CompletedModels.Should().Be(0);
        state.LatestSnapshot.Models.Should().HaveCount(1);
    }

    [Fact]
    public void Update_ModelPhaseTracked()
    {
        var state = new TrainingDisplayState(1);

        state.Update(new TrainingProgress("PlayCard", TrainingPhase.Training, 50, "Training..."));

        var model = state.LatestSnapshot!.Models[0];
        model.ModelType.Should().Be("PlayCard");
        model.Phase.Should().Be(TrainingPhase.Training);
        model.PercentComplete.Should().Be(50);
        model.Message.Should().Be("Training...");
    }

    [Fact]
    public void Update_ValidationMetricsFromProgress()
    {
        var state = new TrainingDisplayState(1);

        state.Update(new TrainingProgress(
            "PlayCard",
            TrainingPhase.Complete,
            100,
            "Complete",
            ValidationMae: 0.1892,
            ValidationRSquared: 0.6234));

        var model = state.LatestSnapshot!.Models[0];
        model.ValidationMae.Should().Be(0.1892);
        model.ValidationRSquared.Should().Be(0.6234);
    }

    [Fact]
    public void Update_CompletedModelsCounted()
    {
        var state = new TrainingDisplayState(3);

        state.Update(new TrainingProgress("PlayCard", TrainingPhase.Complete, 100, "Complete"));
        state.Update(new TrainingProgress("CallTrump", TrainingPhase.Training, 50, "Training..."));

        state.LatestSnapshot!.CompletedModels.Should().Be(1);
    }

    [Fact]
    public void Update_FailedModelsCounted()
    {
        var state = new TrainingDisplayState(2);

        state.Update(new TrainingProgress("PlayCard", TrainingPhase.Failed, 0, "Error"));
        state.Update(new TrainingProgress("CallTrump", TrainingPhase.Complete, 100, "Complete"));

        state.LatestSnapshot!.CompletedModels.Should().Be(2);
    }

    [Fact]
    public void Update_ModelsOrderedByName()
    {
        var state = new TrainingDisplayState(3);

        state.Update(new TrainingProgress("PlayCard", TrainingPhase.LoadingData, 0));
        state.Update(new TrainingProgress("CallTrump", TrainingPhase.LoadingData, 0));
        state.Update(new TrainingProgress("DiscardCard", TrainingPhase.LoadingData, 0));

        state.LatestSnapshot!.Models.Select(m => m.ModelType)
            .Should().ContainInOrder("CallTrump", "DiscardCard", "PlayCard");
    }

    [Fact]
    public void Update_ElapsedTimeTracked()
    {
        var state = new TrainingDisplayState(1);

        state.Update(new TrainingProgress("PlayCard", TrainingPhase.LoadingData, 0));

        Thread.Sleep(50);

        state.Update(new TrainingProgress("PlayCard", TrainingPhase.Training, 25));

        state.LatestSnapshot!.Models[0].Elapsed.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public void Update_CompletedModelStopsTimer()
    {
        var state = new TrainingDisplayState(1);

        state.Update(new TrainingProgress("PlayCard", TrainingPhase.LoadingData, 0));
        Thread.Sleep(50);
        state.Update(new TrainingProgress("PlayCard", TrainingPhase.Complete, 100));

        var elapsedAfterComplete = state.LatestSnapshot!.Models[0].Elapsed;
        Thread.Sleep(50);

        state.LatestSnapshot!.Models[0].Elapsed.Should().Be(elapsedAfterComplete);
    }

    [Fact]
    public void SetValidationMetrics_UpdatesExistingModel()
    {
        var state = new TrainingDisplayState(1);
        state.Update(new TrainingProgress("PlayCard", TrainingPhase.Complete, 100));

        state.SetValidationMetrics("PlayCard", 0.15, 0.72);

        var model = state.LatestSnapshot!.Models[0];
        model.ValidationMae.Should().Be(0.15);
        model.ValidationRSquared.Should().Be(0.72);
    }

    [Fact]
    public void SetValidationMetrics_IgnoresUnknownModel()
    {
        var state = new TrainingDisplayState(1);
        state.Update(new TrainingProgress("PlayCard", TrainingPhase.Complete, 100));

        state.SetValidationMetrics("UnknownModel", 0.15, 0.72);

        state.LatestSnapshot!.Models.Should().HaveCount(1);
        state.LatestSnapshot.Models[0].ValidationMae.Should().BeNull();
    }

    [Fact]
    public void RefreshSnapshot_UpdatesElapsedTimeBetweenUpdates()
    {
        var state = new TrainingDisplayState(1);

        state.Update(new TrainingProgress("PlayCard", TrainingPhase.Training, 50));
        var elapsedBefore = state.LatestSnapshot!.Models[0].Elapsed;

        Thread.Sleep(100);

        state.RefreshSnapshot();
        var elapsedAfter = state.LatestSnapshot!.Models[0].Elapsed;

        elapsedAfter.Should().BeGreaterThan(elapsedBefore);
    }

    [Fact]
    public void RefreshSnapshot_WhenNoModels_SnapshotRemainsNull()
    {
        var state = new TrainingDisplayState(1);

        state.RefreshSnapshot();

        state.LatestSnapshot.Should().BeNull();
    }

    [Fact]
    public void RefreshSnapshot_CompletedModel_ElapsedDoesNotChange()
    {
        var state = new TrainingDisplayState(1);

        state.Update(new TrainingProgress("PlayCard", TrainingPhase.LoadingData, 0));
        Thread.Sleep(50);
        state.Update(new TrainingProgress("PlayCard", TrainingPhase.Complete, 100));

        var elapsedAtComplete = state.LatestSnapshot!.Models[0].Elapsed;
        Thread.Sleep(100);

        state.RefreshSnapshot();

        state.LatestSnapshot!.Models[0].Elapsed.Should().Be(elapsedAtComplete);
    }

    [Fact]
    public void RefreshSnapshot_PreservesModelState()
    {
        var state = new TrainingDisplayState(1);

        state.Update(new TrainingProgress(
            "PlayCard",
            TrainingPhase.Training,
            50,
            "Training model (IDV)..."));

        state.RefreshSnapshot();

        var model = state.LatestSnapshot!.Models[0];
        model.ModelType.Should().Be("PlayCard");
        model.Phase.Should().Be(TrainingPhase.Training);
        model.PercentComplete.Should().Be(50);
        model.Message.Should().Be("Training model (IDV)...");
    }

    [Fact]
    public void Update_MultipleModelsTrackedIndependently()
    {
        var state = new TrainingDisplayState(3);

        state.Update(new TrainingProgress(
            "PlayCard",
            TrainingPhase.Complete,
            100,
            "Complete",
            ValidationMae: 0.19,
            ValidationRSquared: 0.62));
        state.Update(new TrainingProgress(
            "CallTrump",
            TrainingPhase.Training,
            50,
            "Training model (IDV)..."));
        state.Update(new TrainingProgress("DiscardCard", TrainingPhase.LoadingData, 0));

        var snapshot = state.LatestSnapshot!;
        snapshot.Models.Should().HaveCount(3);
        snapshot.CompletedModels.Should().Be(1);

        var playCard = snapshot.Models.First(m => m.ModelType == "PlayCard");
        playCard.Phase.Should().Be(TrainingPhase.Complete);
        playCard.ValidationMae.Should().Be(0.19);

        var callTrump = snapshot.Models.First(m => m.ModelType == "CallTrump");
        callTrump.Phase.Should().Be(TrainingPhase.Training);

        var discardCard = snapshot.Models.First(m => m.ModelType == "DiscardCard");
        discardCard.Phase.Should().Be(TrainingPhase.LoadingData);
    }
}
