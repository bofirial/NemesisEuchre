using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.GameEngine.Tests.Validation;

public class DealResultValidatorTests
{
    private readonly DealResultValidator _validator = new();

    [Fact]
    public void ValidateDeal_WithNullDeal_ThrowsArgumentNullException()
    {
        var act = () => _validator.ValidateDeal(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(7)]
    public void ValidateDeal_WithWrongNumberOfTricks_ThrowsInvalidOperationException(int trickCount)
    {
        var deal = new Deal
        {
            CallingPlayer = PlayerPosition.North,
        };

        for (int i = 0; i < trickCount; i++)
        {
            deal.CompletedTricks.Add(new Trick { WinningTeam = Team.Team1 });
        }

        var act = () => _validator.ValidateDeal(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Deal must have exactly 5 completed tricks");
    }

    [Fact]
    public void ValidateDeal_WithNullCallingPlayer_ThrowsInvalidOperationException()
    {
        var deal = new Deal
        {
            CallingPlayer = null,
        };

        for (int i = 0; i < 5; i++)
        {
            deal.CompletedTricks.Add(new Trick { WinningTeam = Team.Team1 });
        }

        var act = () => _validator.ValidateDeal(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Deal must have a CallingPlayer set");
    }

    [Fact]
    public void ValidateDeal_WithTrickMissingWinningTeam_ThrowsInvalidOperationException()
    {
        var deal = new Deal
        {
            CallingPlayer = PlayerPosition.North,
            CompletedTricks =
            [
                new Trick { WinningTeam = Team.Team1 },
                new Trick { WinningTeam = Team.Team1 },
                new Trick { WinningTeam = null },
                new Trick { WinningTeam = Team.Team2 },
                new Trick { WinningTeam = Team.Team2 }
            ],
        };

        var act = () => _validator.ValidateDeal(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("All completed tricks must have WinningTeam set");
    }

    [Fact]
    public void ValidateDeal_WithValidDeal_DoesNotThrow()
    {
        var deal = new Deal
        {
            CallingPlayer = PlayerPosition.North,
            CompletedTricks =
            [
                new Trick { WinningTeam = Team.Team1 },
                new Trick { WinningTeam = Team.Team1 },
                new Trick { WinningTeam = Team.Team1 },
                new Trick { WinningTeam = Team.Team2 },
                new Trick { WinningTeam = Team.Team2 }
            ],
        };

        var act = () => _validator.ValidateDeal(deal);

        act.Should().NotThrow();
    }
}
