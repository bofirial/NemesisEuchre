using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.MachineLearning.Tests.Trainers;

public class ModelTrainerTests
{
    [Fact]
    public void Constructor_ShouldInitialize_WhenGivenValidLogger()
    {
        var mockLogger = new Mock<ILogger<ModelTrainer>>();

        var trainer = new ModelTrainer(mockLogger.Object);

        trainer.Should().NotBeNull();
    }

    [Fact]
    public Task TrainModelAsync_ShouldThrowNotImplementedException_InCurrentVersion()
    {
        var mockLogger = new Mock<ILogger<ModelTrainer>>();
        var trainer = new ModelTrainer(mockLogger.Object);

        var act = () => trainer.TrainModelAsync("input.csv", "output.zip");

        return act.Should().ThrowAsync<NotImplementedException>();
    }
}
