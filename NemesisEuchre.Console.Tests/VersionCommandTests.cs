using FluentAssertions;

using Moq;

namespace NemesisEuchre.Console.Tests;

public sealed class VersionCommandTests
{
    private readonly Mock<IVersionProvider> _mockVersionProvider;

    public VersionCommandTests()
    {
        _mockVersionProvider = new Mock<IVersionProvider>();
        _ = _mockVersionProvider.Setup(v => v.AssemblyInformationalVersion).Returns("0.1.0-test");
        _ = _mockVersionProvider.Setup(v => v.AssemblyFileVersion).Returns("0.1.0.12345");
        _ = _mockVersionProvider.Setup(v => v.GitCommitId).Returns("abcdef1234567890abcdef1234567890abcdef12");
        _ = _mockVersionProvider.Setup(v => v.GitCommitDate).Returns(new DateTime(2025, 1, 13, 12, 0, 0, DateTimeKind.Utc));
        _ = _mockVersionProvider.Setup(v => v.AssemblyConfiguration).Returns("Debug");
        _ = _mockVersionProvider.Setup(v => v.IsPrerelease).Returns(value: true);
    }

    [Fact]
    public void ExecuteShouldNotThrow()
    {
        var command = new VersionCommand(_mockVersionProvider.Object);

        var action = command.Execute;

        _ = action.Should().NotThrow();
    }

    [Fact]
    public void ExecuteShouldAccessVersionProperties()
    {
        var command = new VersionCommand(_mockVersionProvider.Object);

        command.Execute();

        _mockVersionProvider.Verify(v => v.AssemblyInformationalVersion, Times.AtLeastOnce);
        _mockVersionProvider.Verify(v => v.AssemblyFileVersion, Times.AtLeastOnce);
        _mockVersionProvider.Verify(v => v.GitCommitId, Times.AtLeastOnce);
        _mockVersionProvider.Verify(v => v.GitCommitDate, Times.AtLeastOnce);
        _mockVersionProvider.Verify(v => v.AssemblyConfiguration, Times.AtLeastOnce);
        _mockVersionProvider.Verify(v => v.IsPrerelease, Times.AtLeastOnce);
    }
}
