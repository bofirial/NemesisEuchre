using FluentAssertions;

using NemesisEuchre.Console.Services.Orchestration;

namespace NemesisEuchre.Console.Tests.Services.Orchestration;

public class SubBatchStrategyTests
{
    private readonly SubBatchStrategy _strategy = new();

    [Theory]
    [InlineData(5000, 10000, false)]
    [InlineData(10000, 10000, false)]
    [InlineData(10001, 10000, true)]
    [InlineData(20000, 10000, true)]
    [InlineData(50000, 10000, true)]
    public void ShouldUseSubBatches_ReturnsCorrectResult(int totalGames, int maxPerBatch, bool expected)
    {
        var result = _strategy.ShouldUseSubBatches(totalGames, maxPerBatch);

        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateSubBatchSizes_SingleBatch_ReturnsOneSize()
    {
        var result = _strategy.CalculateSubBatchSizes(5000, 10000).ToList();

        result.Should().ContainSingle();
        result[0].Should().Be(5000);
    }

    [Fact]
    public void CalculateSubBatchSizes_ExactMultiple_ReturnsEqualSizes()
    {
        var result = _strategy.CalculateSubBatchSizes(20000, 10000).ToList();

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(size => size.Should().Be(10000));
    }

    [Fact]
    public void CalculateSubBatchSizes_WithRemainder_LastBatchIsSmaller()
    {
        var result = _strategy.CalculateSubBatchSizes(25000, 10000).ToList();

        result.Should().HaveCount(3);
        result[0].Should().Be(10000);
        result[1].Should().Be(10000);
        result[2].Should().Be(5000);
    }

    [Fact]
    public void CalculateSubBatchSizes_LargeNumber_ReturnsCorrectBatches()
    {
        var result = _strategy.CalculateSubBatchSizes(100000, 10000).ToList();

        result.Should().HaveCount(10);
        result.Should().AllSatisfy(size => size.Should().Be(10000));
        result.Sum().Should().Be(100000);
    }

    [Fact]
    public void CalculateSubBatchSizes_OneGameOverLimit_ReturnsTwoBatches()
    {
        var result = _strategy.CalculateSubBatchSizes(10001, 10000).ToList();

        result.Should().HaveCount(2);
        result[0].Should().Be(10000);
        result[1].Should().Be(1);
    }

    [Fact]
    public void CalculateSubBatchSizes_SmallBatch_ReturnsSingleBatch()
    {
        var result = _strategy.CalculateSubBatchSizes(100, 10000).ToList();

        result.Should().ContainSingle();
        result[0].Should().Be(100);
    }

    [Fact]
    public void CalculateSubBatchSizes_AllBatchesSumToTotal()
    {
        var testCases = new[]
        {
            (TotalGames: 15000, MaxPerBatch: 10000),
            (TotalGames: 27500, MaxPerBatch: 10000),
            (TotalGames: 99999, MaxPerBatch: 10000),
            (TotalGames: 10001, MaxPerBatch: 5000),
        };

        foreach (var (totalGames, maxPerBatch) in testCases)
        {
            var result = _strategy.CalculateSubBatchSizes(totalGames, maxPerBatch).ToList();
            result.Sum().Should().Be(totalGames, $"batches for {totalGames} games should sum to total");
        }
    }
}
