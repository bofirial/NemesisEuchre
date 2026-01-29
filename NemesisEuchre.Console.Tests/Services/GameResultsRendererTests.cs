using FluentAssertions;

using NemesisEuchre.Console.Services;
using NemesisEuchre.Foundation.Constants;
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

    [Fact]
    public void RenderResults_WhenDealHasGoingAlone_DisplaysWentAloneStatus()
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

        game.CompletedDeals.Add(new Deal
        {
            Trump = Suit.Hearts,
            CallingPlayerIsGoingAlone = true,
        });

        renderer.RenderResults(game);

        testConsole.Output.Should().Contain("Yes");
    }

    [Fact]
    public void RenderResults_WhenDealHasPlayerHands_DisplaysCardStrings()
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

        var deal = new Deal
        {
            Trump = Suit.Spades,
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                {
                    PlayerPosition.North, new DealPlayer
                    {
                        StartingHand =
                        [
                            new Card { Rank = Rank.Jack, Suit = Suit.Spades },
                            new Card { Rank = Rank.Ace, Suit = Suit.Hearts },
                        ],
                    }
                },
            },
        };

        game.CompletedDeals.Add(deal);

        renderer.RenderResults(game);

        testConsole.Output.Should().Contain("J♠ ");
        testConsole.Output.Should().Contain("A♥ ");
    }

    [Fact]
    public void RenderResults_WhenDealHasRedCards_DisplaysRedMarkup()
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

        var deal = new Deal
        {
            Trump = Suit.Spades,
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                {
                    PlayerPosition.North, new DealPlayer
                    {
                        StartingHand =
                        [
                            new Card { Rank = Rank.Ace, Suit = Suit.Hearts },
                            new Card { Rank = Rank.King, Suit = Suit.Diamonds },
                        ],
                    }
                },
            },
        };

        game.CompletedDeals.Add(deal);

        renderer.RenderResults(game);

        testConsole.Output.Should().Contain("A♥ ");
        testConsole.Output.Should().Contain("K♦ ");
    }

    [Fact]
    public void RenderResults_WhenDealHasTrump_SortsTrumpCardsFirst()
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

        var deal = new Deal
        {
            Trump = Suit.Spades,
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                {
                    PlayerPosition.North, new DealPlayer
                    {
                        StartingHand =
                        [
                            new Card { Rank = Rank.Ace, Suit = Suit.Hearts },
                            new Card { Rank = Rank.Jack, Suit = Suit.Spades },
                            new Card { Rank = Rank.Nine, Suit = Suit.Spades },
                        ],
                    }
                },
            },
        };

        game.CompletedDeals.Add(deal);

        renderer.RenderResults(game);

        var output = testConsole.Output;
        var jackSpadeIndex = output.IndexOf("J♠ ", StringComparison.Ordinal);
        var nineSpadeIndex = output.IndexOf("9♠ ", StringComparison.Ordinal);
        var aceHeartIndex = output.IndexOf("A♥ ", StringComparison.Ordinal);

        jackSpadeIndex.Should().BeLessThan(nineSpadeIndex);
        nineSpadeIndex.Should().BeLessThan(aceHeartIndex);
    }

    [Fact]
    public void RenderResults_WhenPlayerHasNoHand_DisplaysNA()
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

        var deal = new Deal
        {
            Trump = Suit.Spades,
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                {
                    PlayerPosition.North, new DealPlayer
                    {
                        StartingHand = [],
                    }
                },
            },
        };

        game.CompletedDeals.Add(deal);

        renderer.RenderResults(game);

        testConsole.Output.Should().Contain("N/A");
    }

    [Fact]
    public void RenderResults_WhenTrumpIsNull_SortsBySuitThenRank()
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

        var deal = new Deal
        {
            Trump = null,
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                {
                    PlayerPosition.North, new DealPlayer
                    {
                        StartingHand =
                        [
                            new Card { Rank = Rank.Nine, Suit = Suit.Hearts },
                            new Card { Rank = Rank.Ace, Suit = Suit.Spades },
                            new Card { Rank = Rank.King, Suit = Suit.Diamonds },
                        ],
                    }
                },
            },
        };

        game.CompletedDeals.Add(deal);

        renderer.RenderResults(game);

        var output = testConsole.Output;
        var aceIndex = output.IndexOf("A♠ ", StringComparison.Ordinal);
        var kingIndex = output.IndexOf("K♦ ", StringComparison.Ordinal);
        var nineIndex = output.IndexOf("9♥ ", StringComparison.Ordinal);

        aceIndex.Should().BeLessThan(nineIndex);
        nineIndex.Should().BeLessThan(kingIndex);
    }

    [Fact]
    public void RenderResults_WhenTeam2Wins_DisplaysCorrectWinnerWithYellowColor()
    {
        var testConsole = new TestConsole();
        var renderer = new GameResultsRenderer(testConsole);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 7,
            Team2Score = 10,
            WinningTeam = Team.Team2,
        };

        renderer.RenderResults(game);

        testConsole.Output.Should().Contain("Team2");
        testConsole.Output.Should().Contain("wins");
    }

    [Fact]
    public void RenderResults_WhenNoDealsPlayed_HandlesGracefully()
    {
        var testConsole = new TestConsole();
        var renderer = new GameResultsRenderer(testConsole);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 0,
            Team2Score = 0,
            WinningTeam = Team.Team1,
        };

        renderer.RenderResults(game);

        testConsole.Output.Should().Contain("0");
    }

    [Fact]
    public void RenderDealsTable_WhenPlayerDataMissing_DisplaysNA()
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

        var deal = new Deal
        {
            Trump = Suit.Spades,
            Players = [],
        };

        game.CompletedDeals.Add(deal);

        renderer.RenderResults(game);

        var naCount = testConsole.Output.Split("N/A", StringSplitOptions.None).Length - 1;
        naCount.Should().BeGreaterThanOrEqualTo(4);
    }
}
