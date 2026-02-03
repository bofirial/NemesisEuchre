using FluentAssertions;

using NemesisEuchre.GameEngine.Utilities;

namespace NemesisEuchre.GameEngine.Tests.Utilities;

public class RandomNumberGeneratorTests
{
    [Fact]
    public void NextInt_WithMaxValue_ReturnsValueInRange()
    {
        var generator = new RandomNumberGenerator();
        var results = new HashSet<int>();

        for (int i = 0; i < 100; i++)
        {
            var result = generator.NextInt(10);
            result.Should().BeInRange(0, 9);
            results.Add(result);
        }

        results.Should().HaveCountGreaterThan(1, "should generate different values");
    }

    [Fact]
    public void NextInt_WithMinAndMaxValue_ReturnsValueInRange()
    {
        var generator = new RandomNumberGenerator();
        var results = new HashSet<int>();

        for (int i = 0; i < 100; i++)
        {
            var result = generator.NextInt(5, 15);
            result.Should().BeInRange(5, 14);
            results.Add(result);
        }

        results.Should().HaveCountGreaterThan(1, "should generate different values");
    }

    [Fact]
    public void NextInt_WithMaxValue1_AlwaysReturns0()
    {
        var generator = new RandomNumberGenerator();

        for (int i = 0; i < 10; i++)
        {
            var result = generator.NextInt(1);
            result.Should().Be(0);
        }
    }

    [Fact]
    public void NextInt_MultipleInstances_ProduceDifferentSequences()
    {
        var generator1 = new RandomNumberGenerator();
        var generator2 = new RandomNumberGenerator();

        var results1 = new List<int>();
        var results2 = new List<int>();

        for (int i = 0; i < 10; i++)
        {
            results1.Add(generator1.NextInt(100));
            results2.Add(generator2.NextInt(100));
        }

        results1.Should().NotBeEquivalentTo(results2, "different instances should produce different sequences");
    }

    [Fact]
    public void NextInt_ConsecutiveCalls_ProducesDifferentValues()
    {
        var generator = new RandomNumberGenerator();
        var results = new HashSet<int>();

        for (int i = 0; i < 50; i++)
        {
            results.Add(generator.NextInt(100));
        }

        results.Count.Should().BeGreaterThan(10, "should produce diverse values");
    }

    [Fact]
    public void NextInt_WithMinMaxSameValue_ReturnsMinValue()
    {
        var generator = new RandomNumberGenerator();

        for (int i = 0; i < 10; i++)
        {
            var result = generator.NextInt(5, 5);
            result.Should().Be(5);
        }
    }
}
