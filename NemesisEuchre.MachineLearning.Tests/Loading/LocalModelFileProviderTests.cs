using FluentAssertions;

using NemesisEuchre.MachineLearning.Loading;

namespace NemesisEuchre.MachineLearning.Tests.Loading;

public class LocalModelFileProviderTests
{
    private readonly LocalModelFileProvider _provider = new();

    [Fact]
    public void EnsureModelFile_ReturnsCorrectPath()
    {
        var result = _provider.EnsureModelFile("models", "gen1", "calltrump");

        result.Should().Be(Path.Combine("models", "gen1_calltrump.zip"));
    }

    [Fact]
    public void EnsureModelFile_NormalizesDecisionTypeToLowercase()
    {
        var result = _provider.EnsureModelFile("dir", "name", "PlayCard");

        result.Should().Be(Path.Combine("dir", "name_playcard.zip"));
    }

    [Fact]
    public void EnsureModelFile_CombinesAllComponents()
    {
        var result = _provider.EnsureModelFile("C:/ml/models", "gen5", "DiscardCard");

        result.Should().Be(Path.Combine("C:/ml/models", "gen5_discardcard.zip"));
    }
}
