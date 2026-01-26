using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests;

public class GameWinnerCalculatorTests
{
    private readonly GameWinnerCalculator _calculator = new();

    [Fact]
    public void DetermineWinner_WithNullGame_ThrowsArgumentNullException()
    {
        var act = () => _calculator.DetermineWinner(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DetermineWinner_WithTiedScores_ThrowsInvalidOperationException()
    {
        var game = new Game
        {
            Team1Score = 5,
            Team2Score = 5,
        };

        var act = () => _calculator.DetermineWinner(game);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Game ended in a tie (5-5), which should not occur in Euchre");
    }

    [Fact]
    public void DetermineWinner_WhenTeam1ScoreIsHigher_ReturnsTeam1()
    {
        var game = new Game
        {
            Team1Score = 10,
            Team2Score = 7,
        };

        var winner = _calculator.DetermineWinner(game);

        winner.Should().Be(Team.Team1);
    }

    [Fact]
    public void DetermineWinner_WhenTeam2ScoreIsHigher_ReturnsTeam2()
    {
        var game = new Game
        {
            Team1Score = 8,
            Team2Score = 10,
        };

        var winner = _calculator.DetermineWinner(game);

        winner.Should().Be(Team.Team2);
    }

    [Theory]
    [InlineData(10, 0)]
    [InlineData(15, 14)]
    [InlineData(1, 0)]
    public void DetermineWinner_WithVariousTeam1Victories_ReturnsTeam1(short team1Score, short team2Score)
    {
        var game = new Game
        {
            Team1Score = team1Score,
            Team2Score = team2Score,
        };

        var winner = _calculator.DetermineWinner(game);

        winner.Should().Be(Team.Team1);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(14, 15)]
    [InlineData(0, 1)]
    public void DetermineWinner_WithVariousTeam2Victories_ReturnsTeam2(short team1Score, short team2Score)
    {
        var game = new Game
        {
            Team1Score = team1Score,
            Team2Score = team2Score,
        };

        var winner = _calculator.DetermineWinner(game);

        winner.Should().Be(Team.Team2);
    }
}
