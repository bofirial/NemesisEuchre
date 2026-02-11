using FluentAssertions;

using Moq;

using NemesisEuchre.GameEngine.Selection;
using NemesisEuchre.GameEngine.Utilities;

namespace NemesisEuchre.GameEngine.Tests.Selection;

public class BoltzmannSelectorTests
{
    [Fact]
    public void SelectWeighted_WithNullOptions_ThrowsArgumentNullException()
    {
        var scores = new List<float> { 1.0f };
        var mockRandom = new Mock<IRandomNumberGenerator>();

        var act = () => BoltzmannSelector.SelectWeighted<int>(
            null!,
            scores,
            1.0f,
            mockRandom.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void SelectWeighted_WithNullScores_ThrowsArgumentNullException()
    {
        var options = new List<int> { 1 };
        var mockRandom = new Mock<IRandomNumberGenerator>();

        var act = () => BoltzmannSelector.SelectWeighted(
            options,
            null!,
            1.0f,
            mockRandom.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("scores");
    }

    [Fact]
    public void SelectWeighted_WithNullRandom_ThrowsArgumentNullException()
    {
        var options = new List<int> { 1 };
        var scores = new List<float> { 1.0f };

        var act = () => BoltzmannSelector.SelectWeighted(
            options,
            scores,
            1.0f,
            null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("random");
    }

    [Fact]
    public void SelectWeighted_WithEmptyOptions_ThrowsArgumentException()
    {
        var options = new List<int>();
        var scores = new List<float>();
        var mockRandom = new Mock<IRandomNumberGenerator>();

        var act = () => BoltzmannSelector.SelectWeighted(
            options,
            scores,
            1.0f,
            mockRandom.Object);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Options list cannot be empty.*")
            .WithParameterName("options");
    }

    [Fact]
    public void SelectWeighted_WithMismatchedCounts_ThrowsArgumentException()
    {
        var options = new List<int> { 1, 2 };
        var scores = new List<float> { 1.0f };
        var mockRandom = new Mock<IRandomNumberGenerator>();

        var act = () => BoltzmannSelector.SelectWeighted(
            options,
            scores,
            1.0f,
            mockRandom.Object);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Options and scores must have the same count.*")
            .WithParameterName("scores");
    }

    [Fact]
    public void SelectWeighted_WithZeroTemperature_ThrowsArgumentException()
    {
        var options = new List<int> { 1 };
        var scores = new List<float> { 1.0f };
        var mockRandom = new Mock<IRandomNumberGenerator>();

        var act = () => BoltzmannSelector.SelectWeighted(
            options,
            scores,
            0.0f,
            mockRandom.Object);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Temperature must be greater than zero.*")
            .WithParameterName("temperature");
    }

    [Fact]
    public void SelectWeighted_WithNegativeTemperature_ThrowsArgumentException()
    {
        var options = new List<int> { 1 };
        var scores = new List<float> { 1.0f };
        var mockRandom = new Mock<IRandomNumberGenerator>();

        var act = () => BoltzmannSelector.SelectWeighted(
            options,
            scores,
            -1.0f,
            mockRandom.Object);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Temperature must be greater than zero.*")
            .WithParameterName("temperature");
    }

    [Fact]
    public void SelectWeighted_WithNaNScore_ThrowsArgumentException()
    {
        var options = new List<int> { 1, 2 };
        var scores = new List<float> { 1.0f, float.NaN };
        var mockRandom = new Mock<IRandomNumberGenerator>();

        var act = () => BoltzmannSelector.SelectWeighted(
            options,
            scores,
            1.0f,
            mockRandom.Object);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Score at index 1 is NaN or Infinity.*")
            .WithParameterName("scores");
    }

    [Fact]
    public void SelectWeighted_WithInfinityScore_ThrowsArgumentException()
    {
        var options = new List<int> { 1, 2 };
        var scores = new List<float> { 1.0f, float.PositiveInfinity };
        var mockRandom = new Mock<IRandomNumberGenerator>();

        var act = () => BoltzmannSelector.SelectWeighted(
            options,
            scores,
            1.0f,
            mockRandom.Object);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Score at index 1 is NaN or Infinity.*")
            .WithParameterName("scores");
    }

    [Fact]
    public void SelectWeighted_WithSingleOption_ReturnsThatOption()
    {
        var options = new List<string> { "only" };
        var scores = new List<float> { 5.0f };
        var mockRandom = new Mock<IRandomNumberGenerator>();

        var result = BoltzmannSelector.SelectWeighted(
            options,
            scores,
            1.0f,
            mockRandom.Object);

        result.Should().Be("only");
        mockRandom.Verify(x => x.NextDouble(), Times.Never);
    }

    [Fact]
    public void SelectWeighted_WithIdenticalScores_SelectsUniformly()
    {
        var options = new List<string> { "A", "B", "C" };
        var scores = new List<float> { 2.0f, 2.0f, 2.0f };
        var random = new RandomNumberGenerator();

        var results = new Dictionary<string, int>
        {
            { "A", 0 },
            { "B", 0 },
            { "C", 0 },
        };

        const int iterations = 3000;
        for (int i = 0; i < iterations; i++)
        {
            var result = BoltzmannSelector.SelectWeighted(options, scores, 1.0f, random);
            results[result]++;
        }

        const double expectedCount = iterations / 3.0;
        results["A"].Should().BeInRange((int)(expectedCount * 0.8), (int)(expectedCount * 1.2));
        results["B"].Should().BeInRange((int)(expectedCount * 0.8), (int)(expectedCount * 1.2));
        results["C"].Should().BeInRange((int)(expectedCount * 0.8), (int)(expectedCount * 1.2));
    }

    [Fact]
    public void SelectWeighted_WithLowTemperature_FavorsBestOption()
    {
        var options = new List<string> { "poor", "good", "best" };
        var scores = new List<float> { -1.0f, 1.0f, 3.0f };
        var random = new RandomNumberGenerator();

        var results = new Dictionary<string, int>
        {
            { "poor", 0 },
            { "good", 0 },
            { "best", 0 },
        };

        const int iterations = 1000;
        for (int i = 0; i < iterations; i++)
        {
            var result = BoltzmannSelector.SelectWeighted(options, scores, 0.5f, random);
            results[result]++;
        }

        results["best"].Should().BeGreaterThan((int)(iterations * 0.9));
        results["good"].Should().BeLessThan((int)(iterations * 0.1));
        results["poor"].Should().BeLessThan((int)(iterations * 0.01));
    }

    [Fact]
    public void SelectWeighted_WithHighTemperature_SelectsMoreRandomly()
    {
        var options = new List<string> { "poor", "good", "best" };
        var scores = new List<float> { -1.0f, 1.0f, 3.0f };
        var random = new RandomNumberGenerator();

        var results = new Dictionary<string, int>
        {
            { "poor", 0 },
            { "good", 0 },
            { "best", 0 },
        };

        const int iterations = 3000;
        for (int i = 0; i < iterations; i++)
        {
            var result = BoltzmannSelector.SelectWeighted(options, scores, 5.0f, random);
            results[result]++;
        }

        results["best"].Should().BeLessThan((int)(iterations * 0.6));
        results["poor"].Should().BeGreaterThanOrEqualTo((int)(iterations * 0.2));
    }

    [Fact]
    public void SelectWeighted_WithNegativeScores_HandlesCorrectly()
    {
        var options = new List<string> { "worst", "bad", "okay" };
        var scores = new List<float> { -4.0f, -2.0f, -1.0f };
        var random = new RandomNumberGenerator();

        var results = new Dictionary<string, int>
        {
            { "worst", 0 },
            { "bad", 0 },
            { "okay", 0 },
        };

        const int iterations = 1000;
        for (int i = 0; i < iterations; i++)
        {
            var result = BoltzmannSelector.SelectWeighted(options, scores, 1.0f, random);
            results[result]++;
        }

        results["okay"].Should().BeGreaterThan(results["bad"]);
        results["bad"].Should().BeGreaterThan(results["worst"]);
    }

    [Fact]
    public void SelectWeighted_WithTemperature2_MatchesExpectedDistribution()
    {
        var options = new List<string> { "Pass", "CallDiamonds", "CallDiamondsAlone", "CallSpades" };
        var scores = new List<float> { -1.0f, 2.0f, 4.0f, -1.0f };
        var random = new RandomNumberGenerator();

        var results = new Dictionary<string, int>
        {
            { "Pass", 0 },
            { "CallDiamonds", 0 },
            { "CallDiamondsAlone", 0 },
            { "CallSpades", 0 },
        };

        const int iterations = 10000;
        for (int i = 0; i < iterations; i++)
        {
            var result = BoltzmannSelector.SelectWeighted(options, scores, 2.0f, random);
            results[result]++;
        }

        var totalWeight = Math.Exp(-0.5) + Math.Exp(1.0) + Math.Exp(2.0) + Math.Exp(-0.5);
        var expectedCallDiamondsAlone = Math.Exp(2.0) / totalWeight;
        var expectedCallDiamonds = Math.Exp(1.0) / totalWeight;

        var actualCallDiamondsAlone = results["CallDiamondsAlone"] / (double)iterations;
        var actualCallDiamonds = results["CallDiamonds"] / (double)iterations;

        actualCallDiamondsAlone.Should().BeApproximately(expectedCallDiamondsAlone, 0.02);
        actualCallDiamonds.Should().BeApproximately(expectedCallDiamonds, 0.02);
    }

    [Fact]
    public void SelectWeighted_WithMockedRandom_SelectsExpectedOption()
    {
        var options = new List<string> { "first", "second", "third" };
        var scores = new List<float> { 1.0f, 2.0f, 3.0f };
        var mockRandom = new Mock<IRandomNumberGenerator>();

        var weight1 = Math.Exp(1.0);
        var weight2 = Math.Exp(2.0);
        var weight3 = Math.Exp(3.0);
        var totalWeight = weight1 + weight2 + weight3;

        mockRandom.Setup(x => x.NextDouble()).Returns(0.5);

        var result = BoltzmannSelector.SelectWeighted(options, scores, 1.0f, mockRandom.Object);

        var cumulativeThreshold = (weight1 + weight2) / totalWeight;
        if (cumulativeThreshold > 0.5)
        {
            result.Should().Be("second");
        }
        else
        {
            result.Should().Be("third");
        }
    }
}
