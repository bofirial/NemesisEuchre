using FluentAssertions;

using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.GameEngine.Tests.Validation;

public class TrickValidatorTests
{
    private readonly TrickValidator _validator = new();

    [Fact]
    public void ValidateTrick_WithNullTrick_ThrowsArgumentNullException()
    {
        var act = () => _validator.ValidateTrick(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateTrick_WithValidTrick_DoesNotThrow()
    {
        var trick = new Trick();

        var act = () => _validator.ValidateTrick(trick);

        act.Should().NotThrow();
    }
}
