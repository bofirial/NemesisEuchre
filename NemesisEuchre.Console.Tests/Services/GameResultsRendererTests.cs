using FluentAssertions;

using Moq;

using NemesisEuchre.Console.Models;
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
        trick1.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.Nine), PlayerPosition.North));
        trick1.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.Ten), PlayerPosition.East));
        trick1.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.Jack), PlayerPosition.West));

        var trick2 = new Trick { TrickNumber = 2, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick2.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.Queen), PlayerPosition.North));
        trick2.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.King), PlayerPosition.East));
        trick2.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.Ace), PlayerPosition.West));

        var trick3 = new Trick { TrickNumber = 3, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick3.CardsPlayed.Add(new PlayedCard(new Card(Suit.Clubs, Rank.Nine), PlayerPosition.North));
        trick3.CardsPlayed.Add(new PlayedCard(new Card(Suit.Clubs, Rank.Ten), PlayerPosition.East));
        trick3.CardsPlayed.Add(new PlayedCard(new Card(Suit.Clubs, Rank.Jack), PlayerPosition.West));

        var trick4 = new Trick { TrickNumber = 4, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick4.CardsPlayed.Add(new PlayedCard(new Card(Suit.Clubs, Rank.Queen), PlayerPosition.North));
        trick4.CardsPlayed.Add(new PlayedCard(new Card(Suit.Clubs, Rank.King), PlayerPosition.East));
        trick4.CardsPlayed.Add(new PlayedCard(new Card(Suit.Clubs, Rank.Ace), PlayerPosition.West));

        var trick5 = new Trick { TrickNumber = 5, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick5.CardsPlayed.Add(new PlayedCard(new Card(Suit.Diamonds, Rank.Nine), PlayerPosition.North));
        trick5.CardsPlayed.Add(new PlayedCard(new Card(Suit.Diamonds, Rank.Ten), PlayerPosition.East));
        trick5.CardsPlayed.Add(new PlayedCard(new Card(Suit.Diamonds, Rank.Jack), PlayerPosition.West));

        game.CompletedDeals.Add(new Deal
        {
            Trump = Suit.Hearts,
            ChosenDecision = CallTrumpDecision.CallHeartsAndGoAlone,
            CallingPlayer = PlayerPosition.North,
            DealerPosition = PlayerPosition.South,
            UpCard = new Card(Suit.Hearts, Rank.Nine),
            WinningTeam = Team.Team1,
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                { PlayerPosition.North, new DealPlayer { Position = PlayerPosition.North, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
                { PlayerPosition.East, new DealPlayer { Position = PlayerPosition.East, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
                { PlayerPosition.South, new DealPlayer { Position = PlayerPosition.South, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
                { PlayerPosition.West, new DealPlayer { Position = PlayerPosition.West, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
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
            new Card(Suit.Spades, Rank.Jack),
            new Card(Suit.Hearts, Rank.Ace),
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
            new Card(Suit.Hearts, Rank.Ace),
            new Card(Suit.Diamonds, Rank.King),
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
            new Card(Suit.Hearts, Rank.Ace),
            new Card(Suit.Spades, Rank.Jack),
            new Card(Suit.Spades, Rank.Nine),
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

    [Fact]
    public void BuildLiveResultsTable_DisplaysProgressAndWinRates()
    {
        var testConsole = new TestConsole();
        var mockMapper = new Mock<ICallTrumpDecisionMapper>();
        var renderer = new GameResultsRenderer(testConsole, mockMapper.Object, new Mock<IDecisionRenderer>().Object);

        var snapshot = new BatchProgressSnapshot(450, 260, 180, 10, 2250, 9000, 3600, 450, 27000);
        var elapsed = TimeSpan.FromSeconds(10);

        var renderable = renderer.BuildLiveResultsTable(snapshot, 1000, elapsed);
        testConsole.Write(renderable);

        var output = testConsole.Output;
        output.Should().Contain("450");
        output.Should().Contain("1,000");
        output.Should().Contain("45.0%");
        output.Should().Contain("260");
        output.Should().Contain("180");
        output.Should().Contain("Batch Game Results (Live)");
    }

    [Fact]
    public void BuildLiveResultsTable_DisplaysDecisionCounts()
    {
        var testConsole = new TestConsole();
        var mockMapper = new Mock<ICallTrumpDecisionMapper>();
        var renderer = new GameResultsRenderer(testConsole, mockMapper.Object, new Mock<IDecisionRenderer>().Object);

        var snapshot = new BatchProgressSnapshot(100, 60, 35, 5, 500, 2000, 800, 100, 6000);
        var elapsed = TimeSpan.FromSeconds(5);

        var renderable = renderer.BuildLiveResultsTable(snapshot, 200, elapsed);
        testConsole.Write(renderable);

        var output = testConsole.Output;
        output.Should().Contain("800");
        output.Should().Contain("100");
        output.Should().Contain("6000");
        output.Should().Contain("500");
        output.Should().Contain("2000");
    }

    [Fact]
    public void BuildLiveResultsTable_DisplaysThroughputAndEstimatedRemaining()
    {
        var testConsole = new TestConsole();
        var mockMapper = new Mock<ICallTrumpDecisionMapper>();
        var renderer = new GameResultsRenderer(testConsole, mockMapper.Object, new Mock<IDecisionRenderer>().Object);

        var snapshot = new BatchProgressSnapshot(500, 300, 190, 10, 2500, 10000, 4000, 500, 30000);
        var elapsed = TimeSpan.FromSeconds(10);

        var renderable = renderer.BuildLiveResultsTable(snapshot, 1000, elapsed);
        testConsole.Write(renderable);

        var output = testConsole.Output;
        output.Should().Contain("Throughput");
        output.Should().Contain("50 games/sec");
        output.Should().Contain("Estimated Remaining");
        output.Should().Contain("10.0s");
    }

    [Fact]
    public void BuildLiveResultsTable_WithZeroCompleted_OmitsThroughput()
    {
        var testConsole = new TestConsole();
        var mockMapper = new Mock<ICallTrumpDecisionMapper>();
        var renderer = new GameResultsRenderer(testConsole, mockMapper.Object, new Mock<IDecisionRenderer>().Object);

        var snapshot = BatchProgressSnapshot.Empty;
        var elapsed = TimeSpan.FromSeconds(1);

        var renderable = renderer.BuildLiveResultsTable(snapshot, 100, elapsed);
        testConsole.Write(renderable);

        var output = testConsole.Output;
        output.Should().NotContain("Throughput");
        output.Should().NotContain("Estimated Remaining");
    }

    private static Deal CreateMinimalDeal(Suit trump)
    {
        var nonTrumpSuit = trump == Suit.Clubs ? Suit.Diamonds : Suit.Clubs;

        var trick1 = new Trick { TrickNumber = 1, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick1.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.King), PlayerPosition.North));
        trick1.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Queen), PlayerPosition.East));
        trick1.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Ten), PlayerPosition.South));
        trick1.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Nine), PlayerPosition.West));

        var trick2 = new Trick { TrickNumber = 2, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick2.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.King), PlayerPosition.North));
        trick2.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.Queen), PlayerPosition.East));
        trick2.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.Ten), PlayerPosition.South));
        trick2.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.Nine), PlayerPosition.West));

        var trick3 = new Trick { TrickNumber = 3, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick3.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Ace), PlayerPosition.North));
        trick3.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.King), PlayerPosition.East));
        trick3.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Queen), PlayerPosition.South));
        trick3.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Ten), PlayerPosition.West));

        var trick4 = new Trick { TrickNumber = 4, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick4.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.Ace), PlayerPosition.North));
        trick4.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.King), PlayerPosition.East));
        trick4.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.Queen), PlayerPosition.South));
        trick4.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.Ten), PlayerPosition.West));

        var trick5 = new Trick { TrickNumber = 5, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };
        trick5.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Ace), PlayerPosition.North));
        trick5.CardsPlayed.Add(new PlayedCard(new Card(trump, Rank.Ace), PlayerPosition.East));
        trick5.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.King), PlayerPosition.South));
        trick5.CardsPlayed.Add(new PlayedCard(new Card(nonTrumpSuit, Rank.Queen), PlayerPosition.West));

        return new Deal
        {
            Trump = trump,
            DealerPosition = PlayerPosition.North,
            CallingPlayer = PlayerPosition.East,
            ChosenDecision = CallTrumpDecision.CallHearts,
            UpCard = new Card(trump, Rank.Nine),
            WinningTeam = Team.Team1,
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                { PlayerPosition.North, new DealPlayer { Position = PlayerPosition.North, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
                { PlayerPosition.East, new DealPlayer { Position = PlayerPosition.East, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
                { PlayerPosition.South, new DealPlayer { Position = PlayerPosition.South, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
                { PlayerPosition.West, new DealPlayer { Position = PlayerPosition.West, Actor = new Actor(ActorType.Chaos, null), StartingHand = [] } },
            },
            CompletedTricks = [trick1, trick2, trick3, trick4, trick5],
        };
    }
}
