using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.GameEngine.Tests.Validation;

public class DealValidatorTests
{
    private readonly DealValidator _validator = new();

    [Fact]
    public void ValidateDealPreconditions_WithNullDeal_ThrowsArgumentNullException()
    {
        var act = () => _validator.ValidateDealPreconditions(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(DealStatus.SelectingTrump)]
    [InlineData(DealStatus.Playing)]
    [InlineData(DealStatus.Scoring)]
    [InlineData(DealStatus.Complete)]
    public void ValidateDealPreconditions_WithIncorrectStatus_ThrowsInvalidOperationException(DealStatus status)
    {
        var deal = new Deal { DealStatus = status };

        var act = () => _validator.ValidateDealPreconditions(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Deal must be in NotStarted status, but was {status}");
    }

    [Fact]
    public void ValidateDealPreconditions_WithNullDealerPosition_ThrowsInvalidOperationException()
    {
        var deal = new Deal
        {
            DealStatus = DealStatus.NotStarted,
            DealerPosition = null,
        };

        var act = () => _validator.ValidateDealPreconditions(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("DealerPosition must be set");
    }

    [Fact]
    public void ValidateDealPreconditions_WithNullUpCard_ThrowsInvalidOperationException()
    {
        var deal = new Deal
        {
            DealStatus = DealStatus.NotStarted,
            DealerPosition = PlayerPosition.North,
            UpCard = null,
        };

        var act = () => _validator.ValidateDealPreconditions(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("UpCard must be set");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void ValidateDealPreconditions_WithWrongNumberOfPlayers_ThrowsInvalidOperationException(int playerCount)
    {
        var deal = new Deal
        {
            DealStatus = DealStatus.NotStarted,
            DealerPosition = PlayerPosition.North,
            UpCard = new Card(Suit.Hearts, Rank.Ace),
        };

        for (int i = 0; i < playerCount; i++)
        {
            deal.Players.Add((PlayerPosition)i, new DealPlayer());
        }

        var act = () => _validator.ValidateDealPreconditions(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Deal must have exactly 4 players, but had {playerCount}");
    }

    [Fact]
    public void ValidateDealPreconditions_WithValidDeal_DoesNotThrow()
    {
        var deal = new Deal
        {
            DealStatus = DealStatus.NotStarted,
            DealerPosition = PlayerPosition.North,
            UpCard = new Card(Suit.Hearts, Rank.Ace),
        };

        deal.Players.Add(PlayerPosition.North, new DealPlayer());
        deal.Players.Add(PlayerPosition.East, new DealPlayer());
        deal.Players.Add(PlayerPosition.South, new DealPlayer());
        deal.Players.Add(PlayerPosition.West, new DealPlayer());

        var act = () => _validator.ValidateDealPreconditions(deal);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(6)]
    public void ValidateAllTricksPlayed_WithWrongNumberOfTricks_ThrowsInvalidOperationException(int trickCount)
    {
        var deal = new Deal();

        for (int i = 0; i < trickCount; i++)
        {
            deal.CompletedTricks.Add(new Trick());
        }

        var act = () => _validator.ValidateAllTricksPlayed(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Deal must have exactly 5 completed tricks, but had {trickCount}");
    }

    [Fact]
    public void ValidateAllTricksPlayed_WithFiveTricks_DoesNotThrow()
    {
        var deal = new Deal();

        for (int i = 0; i < 5; i++)
        {
            deal.CompletedTricks.Add(new Trick());
        }

        var act = () => _validator.ValidateAllTricksPlayed(deal);

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateDealCompleted_WithNullDealResult_ThrowsInvalidOperationException()
    {
        var deal = new Deal
        {
            DealResult = null,
            WinningTeam = Team.Team1,
        };

        var act = () => _validator.ValidateDealCompleted(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("DealResult must be set after scoring");
    }

    [Fact]
    public void ValidateDealCompleted_WithNullWinningTeam_ThrowsInvalidOperationException()
    {
        var deal = new Deal
        {
            DealResult = DealResult.WonStandardBid,
            WinningTeam = null,
        };

        var act = () => _validator.ValidateDealCompleted(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("WinningTeam must be set after scoring");
    }

    [Fact]
    public void ValidateDealCompleted_WithValidDealResult_DoesNotThrow()
    {
        var deal = new Deal
        {
            DealResult = DealResult.WonStandardBid,
            WinningTeam = Team.Team1,
        };

        var act = () => _validator.ValidateDealCompleted(deal);

        act.Should().NotThrow();
    }
}
