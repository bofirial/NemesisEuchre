using FluentAssertions;

using NemesisEuchre.Console.Services;
using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

using Spectre.Console.Testing;

namespace NemesisEuchre.Console.Tests.Services;

public class GameResultsRendererTests
{
    [Fact]
    public void RenderResults_WhenTeam1Wins_DisplaysTeam1AsWinner()
    {
        var testConsole = new TestConsole();
        var renderer = new GameResultsRenderer(testConsole);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };

        renderer.RenderResults(game);

        testConsole.Output.Should().Contain("Team1");
        testConsole.Output.Should().Contain("wins");
    }

    [Fact]
    public void RenderResults_WhenExecuted_DisplaysScores()
    {
        var testConsole = new TestConsole();
        var renderer = new GameResultsRenderer(testConsole);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 8,
            WinningTeam = Team.Team1,
        };

        renderer.RenderResults(game);

        testConsole.Output.Should().Contain("10");
        testConsole.Output.Should().Contain("8");
    }

    [Fact]
    public void RenderResults_WhenExecuted_DisplaysDealCount()
    {
        var testConsole = new TestConsole();
        var renderer = new GameResultsRenderer(testConsole);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };

        game.CompletedDeals.Add(new Deal { Trump = Suit.Hearts });
        game.CompletedDeals.Add(new Deal { Trump = Suit.Spades });
        game.CompletedDeals.Add(new Deal { Trump = Suit.Diamonds });

        renderer.RenderResults(game);

        testConsole.Output.Should().Contain("3");
    }
}
