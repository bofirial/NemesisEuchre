using FluentAssertions;

using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.Tests.Models;

public class MetricsCalculatorTests
{
    [Fact]
    public void CalculatePerClassMetrics_WithPerfectPredictions_ReturnsAllOnes()
    {
        var confusionMatrix = new int[][]
        {
            [10, 0, 0],
            [0, 15, 0],
            [0, 0, 20],
        };

        var metrics = MetricsCalculator.CalculatePerClassMetrics(confusionMatrix);

        metrics.Should().HaveCount(3);
        foreach (var classMetric in metrics)
        {
            classMetric.Precision.Should().Be(1.0);
            classMetric.Recall.Should().Be(1.0);
            classMetric.F1Score.Should().Be(1.0);
            classMetric.Support.Should().BeGreaterThan(0);
        }

        metrics[0].ClassLabel.Should().Be(0);
        metrics[0].Support.Should().Be(10);
        metrics[1].ClassLabel.Should().Be(1);
        metrics[1].Support.Should().Be(15);
        metrics[2].ClassLabel.Should().Be(2);
        metrics[2].Support.Should().Be(20);
    }

    [Fact]
    public void CalculatePerClassMetrics_WithMixedResults_ReturnsCorrectMetrics()
    {
        var confusionMatrix = new int[][]
        {
            [8, 1, 1],
            [2, 15, 3],
            [1, 2, 10],
        };

        var metrics = MetricsCalculator.CalculatePerClassMetrics(confusionMatrix);

        metrics.Should().HaveCount(3);

        metrics[0].ClassLabel.Should().Be(0);
        metrics[0].Precision.Should().BeApproximately(8.0 / 11.0, 0.001);
        metrics[0].Recall.Should().BeApproximately(8.0 / 10.0, 0.001);
        metrics[0].F1Score.Should().BeApproximately(2 * (8.0 / 11.0) * (8.0 / 10.0) / ((8.0 / 11.0) + (8.0 / 10.0)), 0.001);
        metrics[0].Support.Should().Be(10);

        metrics[1].ClassLabel.Should().Be(1);
        metrics[1].Precision.Should().BeApproximately(15.0 / 18.0, 0.001);
        metrics[1].Recall.Should().BeApproximately(15.0 / 20.0, 0.001);
        metrics[1].F1Score.Should().BeApproximately(2 * (15.0 / 18.0) * (15.0 / 20.0) / ((15.0 / 18.0) + (15.0 / 20.0)), 0.001);
        metrics[1].Support.Should().Be(20);

        metrics[2].ClassLabel.Should().Be(2);
        metrics[2].Precision.Should().BeApproximately(10.0 / 14.0, 0.001);
        metrics[2].Recall.Should().BeApproximately(10.0 / 13.0, 0.001);
        metrics[2].F1Score.Should().BeApproximately(2 * (10.0 / 14.0) * (10.0 / 13.0) / ((10.0 / 14.0) + (10.0 / 13.0)), 0.001);
        metrics[2].Support.Should().Be(13);
    }

    [Fact]
    public void CalculatePerClassMetrics_WithNoActualExamples_ReturnsNaNRecall()
    {
        var confusionMatrix = new int[][]
        {
            [10, 0],
            [0, 0],
        };

        var metrics = MetricsCalculator.CalculatePerClassMetrics(confusionMatrix);

        metrics.Should().HaveCount(2);

        metrics[0].Precision.Should().Be(1.0);
        metrics[0].Recall.Should().Be(1.0);
        metrics[0].F1Score.Should().Be(1.0);
        metrics[0].Support.Should().Be(10);

        metrics[1].Precision.Should().Be(double.NaN);
        metrics[1].Recall.Should().Be(double.NaN);
        metrics[1].F1Score.Should().Be(double.NaN);
        metrics[1].Support.Should().Be(0);
    }

    [Fact]
    public void CalculatePerClassMetrics_WithNoPredictions_ReturnsNaNPrecision()
    {
        var confusionMatrix = new int[][]
        {
            [8, 2, 0],
            [3, 12, 0],
            [0, 0, 0],
        };

        var metrics = MetricsCalculator.CalculatePerClassMetrics(confusionMatrix);

        metrics.Should().HaveCount(3);

        metrics[0].ClassLabel.Should().Be(0);
        metrics[0].Precision.Should().BeApproximately(8.0 / 11.0, 0.001);
        metrics[0].Recall.Should().BeApproximately(8.0 / 10.0, 0.001);
        metrics[0].Support.Should().Be(10);

        metrics[1].ClassLabel.Should().Be(1);
        metrics[1].Precision.Should().BeApproximately(12.0 / 14.0, 0.001);
        metrics[1].Recall.Should().BeApproximately(12.0 / 15.0, 0.001);
        metrics[1].Support.Should().Be(15);

        metrics[2].ClassLabel.Should().Be(2);
        metrics[2].Precision.Should().Be(double.NaN);
        metrics[2].Recall.Should().Be(double.NaN);
        metrics[2].F1Score.Should().Be(double.NaN);
        metrics[2].Support.Should().Be(0);
    }

    [Fact]
    public void CalculatePerClassMetrics_WithNullMatrix_ThrowsArgumentNullException()
    {
        var act = () => MetricsCalculator.CalculatePerClassMetrics(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CalculatePerClassMetrics_WithLargeNumberOfClasses_HandlesCorrectly()
    {
        var confusionMatrix = new int[11][];
        for (int i = 0; i < 11; i++)
        {
            confusionMatrix[i] = new int[11];
            for (int j = 0; j < 11; j++)
            {
                confusionMatrix[i][j] = i == j ? 100 : 5;
            }
        }

        var metrics = MetricsCalculator.CalculatePerClassMetrics(confusionMatrix);

        metrics.Should().HaveCount(11);

        for (int i = 0; i < 11; i++)
        {
            metrics[i].ClassLabel.Should().Be(i);
            metrics[i].Precision.Should().BeGreaterThan(0);
            metrics[i].Recall.Should().BeGreaterThan(0);
            metrics[i].F1Score.Should().BeGreaterThan(0);
            metrics[i].Support.Should().Be(150);

            const double expectedPrecision = 100.0 / 150.0;
            const double expectedRecall = 100.0 / 150.0;
            metrics[i].Precision.Should().BeApproximately(expectedPrecision, 0.001);
            metrics[i].Recall.Should().BeApproximately(expectedRecall, 0.001);
        }
    }

    [Fact]
    public void CalculatePerClassMetrics_WithZeroPredictionsForClass_ReturnsNaNForPrecisionAndF1()
    {
        var confusionMatrix = new int[][]
        {
            [10, 0, 8],
            [5, 0, 10],
            [3, 0, 9],
        };

        var metrics = MetricsCalculator.CalculatePerClassMetrics(confusionMatrix);

        metrics.Should().HaveCount(3);

        metrics[1].ClassLabel.Should().Be(1);
        metrics[1].Precision.Should().Be(double.NaN);
        metrics[1].Recall.Should().BeApproximately(0.0, 0.001);
        metrics[1].F1Score.Should().Be(double.NaN);
        metrics[1].Support.Should().Be(15);
    }
}
