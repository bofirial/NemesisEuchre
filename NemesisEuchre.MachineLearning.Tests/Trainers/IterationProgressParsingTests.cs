using FluentAssertions;

using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.MachineLearning.Tests.Trainers;

public class IterationProgressParsingTests
{
    [Theory]
    [InlineData("[1]\ttraining's rmse: 0.456789", 1, 0.456789)]
    [InlineData("[100]\ttraining's rmse: 0.123456", 100, 0.123456)]
    [InlineData("[200]\ttraining's rmse: 0.098765", 200, 0.098765)]
    public void TryParseIterationProgress_ParsesLightGbmFormat(string message, int expectedIteration, double expectedMetric)
    {
        var result = RegressionModelTrainerBase<CallTrumpTrainingData>.TryParseIterationProgress(
            message, out var iteration, out var metric);

        result.Should().BeTrue();
        iteration.Should().Be(expectedIteration);
        metric.Should().BeApproximately(expectedMetric, 0.0001);
    }

    [Theory]
    [InlineData("[5]\tvalid_0's rmse: 1.23e-04", 5, 1.23e-04)]
    [InlineData("[50]\tvalid_0's rmse: 2.5e+01", 50, 2.5e+01)]
    public void TryParseIterationProgress_ParsesScientificNotation(string message, int expectedIteration, double expectedMetric)
    {
        var result = RegressionModelTrainerBase<CallTrumpTrainingData>.TryParseIterationProgress(
            message, out var iteration, out var metric);

        result.Should().BeTrue();
        iteration.Should().Be(expectedIteration);
        metric.Should().BeApproximately(expectedMetric, expectedMetric * 0.01);
    }

    [Theory]
    [InlineData("Starting training pipeline...")]
    [InlineData("Mapping column 'Features' to numeric")]
    [InlineData("")]
    [InlineData("Training complete.")]
    public void TryParseIterationProgress_ReturnsFalseForNonIterationMessages(string message)
    {
        var result = RegressionModelTrainerBase<CallTrumpTrainingData>.TryParseIterationProgress(
            message, out var iteration, out var metric);

        result.Should().BeFalse();
        iteration.Should().Be(0);
        metric.Should().BeNull();
    }

    [Fact]
    public void TryParseIterationProgress_ParsesZeroIteration()
    {
        var result = RegressionModelTrainerBase<CallTrumpTrainingData>.TryParseIterationProgress(
            "[0]\ttraining's rmse: 1.5", out var iteration, out var metric);

        result.Should().BeTrue();
        iteration.Should().Be(0);
        metric.Should().BeApproximately(1.5, 0.01);
    }
}
