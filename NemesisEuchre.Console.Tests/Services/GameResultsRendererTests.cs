using FluentAssertions;

using Moq;

using NemesisEuchre.Console.Services;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Mappers;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

using Spectre.Console.Testing;

namespace NemesisEuchre.Console.Tests.Services;

public class GameResultsRendererTests
{
    [Fact]
    public void RenderResults_WhenTeam1Wins_DisplaysTeam1AsWinner()
    {
        var testConsole = new TestConsole();
        var mockMapper = new Mock<ICallTrumpDecisionMapper>();
        var renderer = new GameResultsRenderer(testConsole, mockMapper.Object, new Mock<IDecisionRenderer>().Object);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };

        renderer.RenderResults(game, false);

        testConsole.Output.Should().Contain("Team1");
        testConsole.Output.Should().Contain("wins");
    }

    [Fact]
    public void RenderResults_WhenExecuted_DisplaysScores()
    {
        var testConsole = new TestConsole();
        var mockMapper = new Mock<ICallTrumpDecisionMapper>();
        var renderer = new GameResultsRenderer(testConsole, mockMapper.Object, new Mock<IDecisionRenderer>().Object);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 8,
            WinningTeam = Team.Team1,
        };

        renderer.RenderResults(game, false);

        testConsole.Output.Should().Contain("10");
        testConsole.Output.Should().Contain("8");
    }

    [Fact]
    public void RenderResults_WhenExecuted_DisplaysDealCount()
    {
        var testConsole = new TestConsole();
        var mockMapper = new Mock<ICallTrumpDecisionMapper>();
        var renderer = new GameResultsRenderer(testConsole, mockMapper.Object, new Mock<IDecisionRenderer>().Object);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };

        game.CompletedDeals.Add(CreateMinimalDeal(Suit.Hearts));
        game.CompletedDeals.Add(CreateMinimalDeal(Suit.Spades));
        game.CompletedDeals.Add(CreateMinimalDeal(Suit.Diamonds));

        renderer.RenderResults(game, false);

        testConsole.Output.Should().Contain("3");
    }

    [Fact]
    public void RenderResults_WhenDealHasGoingAlone_DisplaysWentAloneStatus()
    {
        var testConsole = new TestConsole();
        var mockMapper = new Mock<ICallTrumpDecisionMapper>();
        mockMapper.Setup(m => m.IsGoingAloneDecision(It.IsAny<CallTrumpDecision>())).Returns(true);
        var renderer = new GameResultsRenderer(testConsole, mockMapper.Object, new Mock<IDecisionRenderer>().Object);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };

        var trick1 = new Trick { TrickNumber = 1, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick1.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.North, Card = new Card { Rank = Rank.Nine, Suit = Suit.Hearts } });
        trick1.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.East, Card = new Card { Rank = Rank.Ten, Suit = Suit.Hearts } });
        trick1.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.West, Card = new Card { Rank = Rank.Jack, Suit = Suit.Hearts } });

        var trick2 = new Trick { TrickNumber = 2, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick2.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.North, Card = new Card { Rank = Rank.Queen, Suit = Suit.Hearts } });
        trick2.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.East, Card = new Card { Rank = Rank.King, Suit = Suit.Hearts } });
        trick2.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.West, Card = new Card { Rank = Rank.Ace, Suit = Suit.Hearts } });

        var trick3 = new Trick { TrickNumber = 3, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick3.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.North, Card = new Card { Rank = Rank.Nine, Suit = Suit.Clubs } });
        trick3.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.East, Card = new Card { Rank = Rank.Ten, Suit = Suit.Clubs } });
        trick3.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.West, Card = new Card { Rank = Rank.Jack, Suit = Suit.Clubs } });

        var trick4 = new Trick { TrickNumber = 4, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick4.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.North, Card = new Card { Rank = Rank.Queen, Suit = Suit.Clubs } });
        trick4.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.East, Card = new Card { Rank = Rank.King, Suit = Suit.Clubs } });
        trick4.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.West, Card = new Card { Rank = Rank.Ace, Suit = Suit.Clubs } });

        var trick5 = new Trick { TrickNumber = 5, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick5.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.North, Card = new Card { Rank = Rank.Nine, Suit = Suit.Diamonds } });
        trick5.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.East, Card = new Card { Rank = Rank.Ten, Suit = Suit.Diamonds } });
        trick5.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.West, Card = new Card { Rank = Rank.Jack, Suit = Suit.Diamonds } });

        game.CompletedDeals.Add(new Deal
        {
            Trump = Suit.Hearts,
            ChosenDecision = CallTrumpDecision.CallHeartsAndGoAlone,
            CallingPlayer = PlayerPosition.North,
            DealerPosition = PlayerPosition.South,
            UpCard = new Card { Rank = Rank.Nine, Suit = Suit.Hearts },
            WinningTeam = Team.Team1,
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                { PlayerPosition.North, new DealPlayer { Position = PlayerPosition.North, StartingHand = [] } },
                { PlayerPosition.East, new DealPlayer { Position = PlayerPosition.East, StartingHand = [] } },
                { PlayerPosition.South, new DealPlayer { Position = PlayerPosition.South, StartingHand = [] } },
                { PlayerPosition.West, new DealPlayer { Position = PlayerPosition.West, StartingHand = [] } },
            },
            CompletedTricks = [trick1, trick2, trick3, trick4, trick5],
        });

        renderer.RenderResults(game, false);

        testConsole.Output.Should().Contain("and Go");
        testConsole.Output.Should().Contain("Alone");
    }

    [Fact]
    public void RenderResults_WhenDealHasPlayerHands_DisplaysCardStrings()
    {
        var testConsole = new TestConsole();
        var mockMapper = new Mock<ICallTrumpDecisionMapper>();
        var renderer = new GameResultsRenderer(testConsole, mockMapper.Object, new Mock<IDecisionRenderer>().Object);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };

        var deal = CreateMinimalDeal(Suit.Spades);
        deal.Players[PlayerPosition.North].StartingHand =
        [
            new Card { Rank = Rank.Jack, Suit = Suit.Spades },
            new Card { Rank = Rank.Ace, Suit = Suit.Hearts },
        ];

        game.CompletedDeals.Add(deal);

        renderer.RenderResults(game, false);

        testConsole.Output.Should().Contain("J♠ ");
        testConsole.Output.Should().Contain("A♥ ");
    }

    [Fact]
    public void RenderResults_WhenDealHasRedCards_DisplaysRedMarkup()
    {
        var testConsole = new TestConsole();
        var mockMapper = new Mock<ICallTrumpDecisionMapper>();
        var renderer = new GameResultsRenderer(testConsole, mockMapper.Object, new Mock<IDecisionRenderer>().Object);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };

        var deal = CreateMinimalDeal(Suit.Spades);
        deal.Players[PlayerPosition.North].StartingHand =
        [
            new Card { Rank = Rank.Ace, Suit = Suit.Hearts },
            new Card { Rank = Rank.King, Suit = Suit.Diamonds },
        ];

        game.CompletedDeals.Add(deal);

        renderer.RenderResults(game, false);

        testConsole.Output.Should().Contain("A♥ ");
        testConsole.Output.Should().Contain("K♦ ");
    }

    [Fact]
    public void RenderResults_WhenDealHasTrump_SortsTrumpCardsFirst()
    {
        var testConsole = new TestConsole();
        var mockMapper = new Mock<ICallTrumpDecisionMapper>();
        var renderer = new GameResultsRenderer(testConsole, mockMapper.Object, new Mock<IDecisionRenderer>().Object);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };

        var deal = CreateMinimalDeal(Suit.Spades);
        deal.Players[PlayerPosition.North].StartingHand =
        [
            new Card { Rank = Rank.Ace, Suit = Suit.Hearts },
            new Card { Rank = Rank.Jack, Suit = Suit.Spades },
            new Card { Rank = Rank.Nine, Suit = Suit.Spades },
        ];

        game.CompletedDeals.Add(deal);

        renderer.RenderResults(game, false);

        var output = testConsole.Output;
        output.Should().Contain("J♠");
        output.Should().Contain("9♠");
        output.Should().Contain("A♥");
    }

    [Fact]
    public void RenderResults_WhenPlayerHasNoHand_DisplaysNA()
    {
        var testConsole = new TestConsole();
        var mockMapper = new Mock<ICallTrumpDecisionMapper>();
        var renderer = new GameResultsRenderer(testConsole, mockMapper.Object, new Mock<IDecisionRenderer>().Object);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };

        var deal = CreateMinimalDeal(Suit.Spades);
        deal.Players[PlayerPosition.North].StartingHand = [];

        game.CompletedDeals.Add(deal);

        renderer.RenderResults(game, false);

        testConsole.Output.Should().Contain("N/A");
    }

    [Fact]
    public void RenderResults_WhenTeam2Wins_DisplaysCorrectWinnerWithYellowColor()
    {
        var testConsole = new TestConsole();
        var mockMapper = new Mock<ICallTrumpDecisionMapper>();
        var renderer = new GameResultsRenderer(testConsole, mockMapper.Object, new Mock<IDecisionRenderer>().Object);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 7,
            Team2Score = 10,
            WinningTeam = Team.Team2,
        };

        renderer.RenderResults(game, false);

        testConsole.Output.Should().Contain("Team2");
        testConsole.Output.Should().Contain("wins");
    }

    [Fact]
    public void RenderResults_WhenNoDealsPlayed_HandlesGracefully()
    {
        var testConsole = new TestConsole();
        var mockMapper = new Mock<ICallTrumpDecisionMapper>();
        var renderer = new GameResultsRenderer(testConsole, mockMapper.Object, new Mock<IDecisionRenderer>().Object);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 0,
            Team2Score = 0,
            WinningTeam = Team.Team1,
        };

        renderer.RenderResults(game, false);

        testConsole.Output.Should().Contain("0");
    }

    private static Deal CreateMinimalDeal(Suit trump)
    {
        var nonTrumpSuit = trump == Suit.Clubs ? Suit.Diamonds : Suit.Clubs;

        var trick1 = new Trick { TrickNumber = 1, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick1.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.North, Card = new Card { Rank = Rank.King, Suit = nonTrumpSuit } });
        trick1.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.East, Card = new Card { Rank = Rank.Queen, Suit = nonTrumpSuit } });
        trick1.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.South, Card = new Card { Rank = Rank.Ten, Suit = nonTrumpSuit } });
        trick1.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.West, Card = new Card { Rank = Rank.Nine, Suit = nonTrumpSuit } });

        var trick2 = new Trick { TrickNumber = 2, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick2.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.North, Card = new Card { Rank = Rank.King, Suit = trump } });
        trick2.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.East, Card = new Card { Rank = Rank.Queen, Suit = trump } });
        trick2.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.South, Card = new Card { Rank = Rank.Ten, Suit = trump } });
        trick2.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.West, Card = new Card { Rank = Rank.Nine, Suit = trump } });

        var trick3 = new Trick { TrickNumber = 3, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick3.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.North, Card = new Card { Rank = Rank.Ace, Suit = nonTrumpSuit } });
        trick3.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.East, Card = new Card { Rank = Rank.King, Suit = nonTrumpSuit } });
        trick3.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.South, Card = new Card { Rank = Rank.Queen, Suit = nonTrumpSuit } });
        trick3.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.West, Card = new Card { Rank = Rank.Ten, Suit = nonTrumpSuit } });

        var trick4 = new Trick { TrickNumber = 4, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick4.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.North, Card = new Card { Rank = Rank.Ace, Suit = trump } });
        trick4.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.East, Card = new Card { Rank = Rank.King, Suit = trump } });
        trick4.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.South, Card = new Card { Rank = Rank.Queen, Suit = trump } });
        trick4.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.West, Card = new Card { Rank = Rank.Ten, Suit = trump } });

        var trick5 = new Trick { TrickNumber = 5, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick5.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.North, Card = new Card { Rank = Rank.Ace, Suit = nonTrumpSuit } });
        trick5.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.East, Card = new Card { Rank = Rank.Ace, Suit = trump } });
        trick5.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.South, Card = new Card { Rank = Rank.King, Suit = nonTrumpSuit } });
        trick5.CardsPlayed.Add(new PlayedCard { PlayerPosition = PlayerPosition.West, Card = new Card { Rank = Rank.Queen, Suit = nonTrumpSuit } });

        return new Deal
        {
            Trump = trump,
            DealerPosition = PlayerPosition.North,
            CallingPlayer = PlayerPosition.East,
            ChosenDecision = CallTrumpDecision.CallHearts,
            UpCard = new Card { Rank = Rank.Nine, Suit = trump },
            WinningTeam = Team.Team1,
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                { PlayerPosition.North, new DealPlayer { Position = PlayerPosition.North, StartingHand = [] } },
                { PlayerPosition.East, new DealPlayer { Position = PlayerPosition.East, StartingHand = [] } },
                { PlayerPosition.South, new DealPlayer { Position = PlayerPosition.South, StartingHand = [] } },
                { PlayerPosition.West, new DealPlayer { Position = PlayerPosition.West, StartingHand = [] } },
            },
            CompletedTricks = [trick1, trick2, trick3, trick4, trick5],
        };
    }
}
