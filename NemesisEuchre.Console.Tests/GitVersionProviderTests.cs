using FluentAssertions;

namespace NemesisEuchre.Console.Tests;

public sealed class GitVersionProviderTests
{
    private static readonly string[] ValidConfigurationModes = ["Debug", "Release"];

    [Fact]
    public void AssemblyInformationalVersionShouldNotBeEmpty()
    {
        var provider = new GitVersionProvider();

        _ = provider.AssemblyInformationalVersion.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GitCommitIdShouldBe40CharactersLong()
    {
        var provider = new GitVersionProvider();

        _ = provider.GitCommitId.Should().HaveLength(40);
    }

    [Fact]
    public void GitCommitDateShouldBeInPast()
    {
        var provider = new GitVersionProvider();

        _ = provider.GitCommitDate.Should().BeBefore(DateTime.UtcNow);
    }

    [Fact]
    public void AssemblyConfigurationShouldBeValidBuildConfiguration()
    {
        var provider = new GitVersionProvider();

        _ = ValidConfigurationModes.Should().Contain(provider.AssemblyConfiguration);
    }
}
