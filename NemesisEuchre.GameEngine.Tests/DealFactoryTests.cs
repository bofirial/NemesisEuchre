using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests;

public class DealFactoryTests
{
    [Fact]
    public Task CreateDealAsync_WithNullGame_ThrowsArgumentNullException()
    {
        var factory = new DealFactory(new NoOpShuffler());

        var act = async () => await factory.CreateDealAsync(null!);

        return act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("game");
    }

    [Fact]
    public Task CreateDealAsync_WithFewerThan4Players_ThrowsArgumentException()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = new Game();
        game.Players.Add(PlayerPosition.North, new Player { Position = PlayerPosition.North });
        game.Players.Add(PlayerPosition.East, new Player { Position = PlayerPosition.East });

        var act = async () => await factory.CreateDealAsync(game);

        return act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("game")
            .WithMessage("Game must have exactly 4 players.*");
    }

    [Fact]
    public Task CreateDealAsync_WithMoreThan4Players_ThrowsArgumentException()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();
        game.Players.Add((PlayerPosition)99, new Player { Position = (PlayerPosition)99 });

        var act = async () => await factory.CreateDealAsync(game);

        return act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("game")
            .WithMessage("Game must have exactly 4 players.*");
    }

    [Fact]
    public Task CreateDealAsync_WithPreviousDealHavingNullDealer_ThrowsInvalidOperationException()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();
        var previousDeal = new Deal { DealerPosition = null };

        var act = async () => await factory.CreateDealAsync(game, previousDeal);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Previous deal must have a dealer position.");
    }

    [Fact]
    public async Task CreateDealAsync_WithNoPreviousDeal_SetsDealerToNorth()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        deal.DealerPosition.Should().Be(PlayerPosition.North);
    }

    [Fact]
    public async Task CreateDealAsync_WithPreviousDealerNorth_SetsDealerToEast()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();
        var previousDeal = new Deal { DealerPosition = PlayerPosition.North };

        var deal = await factory.CreateDealAsync(game, previousDeal);

        deal.DealerPosition.Should().Be(PlayerPosition.East);
    }

    [Fact]
    public async Task CreateDealAsync_WithPreviousDealerEast_SetsDealerToSouth()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();
        var previousDeal = new Deal { DealerPosition = PlayerPosition.East };

        var deal = await factory.CreateDealAsync(game, previousDeal);

        deal.DealerPosition.Should().Be(PlayerPosition.South);
    }

    [Fact]
    public async Task CreateDealAsync_WithPreviousDealerSouth_SetsDealerToWest()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();
        var previousDeal = new Deal { DealerPosition = PlayerPosition.South };

        var deal = await factory.CreateDealAsync(game, previousDeal);

        deal.DealerPosition.Should().Be(PlayerPosition.West);
    }

    [Fact]
    public async Task CreateDealAsync_WithPreviousDealerWest_SetsDealerToNorth()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();
        var previousDeal = new Deal { DealerPosition = PlayerPosition.West };

        var deal = await factory.CreateDealAsync(game, previousDeal);

        deal.DealerPosition.Should().Be(PlayerPosition.North);
    }

    [Fact]
    public async Task CreateDealAsync_DealsExactly5CardsToEachPlayer()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();

        await factory.CreateDealAsync(game);

        game.Players[PlayerPosition.North].Hand.Should().HaveCount(5);
        game.Players[PlayerPosition.East].Hand.Should().HaveCount(5);
        game.Players[PlayerPosition.South].Hand.Should().HaveCount(5);
        game.Players[PlayerPosition.West].Hand.Should().HaveCount(5);
    }

    [Fact]
    public async Task CreateDealAsync_ClearsExistingPlayerHandsBeforeDealing()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();

        game.Players[PlayerPosition.North].Hand.Add(new Card { Suit = Suit.Spades, Rank = Rank.Ace });
        game.Players[PlayerPosition.East].Hand.Add(new Card { Suit = Suit.Hearts, Rank = Rank.King });

        await factory.CreateDealAsync(game);

        game.Players[PlayerPosition.North].Hand.Should().HaveCount(5);
        game.Players[PlayerPosition.East].Hand.Should().HaveCount(5);
    }

    [Fact]
    public async Task CreateDealAsync_EachPlayerReceivesUniqueCards()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();

        await factory.CreateDealAsync(game);

        var allPlayerCards = game.Players.Values
            .SelectMany(p => p.Hand)
            .ToList();

        allPlayerCards.Should().OnlyHaveUniqueItems(card => $"{card.Suit}-{card.Rank}");
    }

    [Fact]
    public async Task CreateDealAsync_SetsUpCardCorrectly()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        deal.UpCard.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateDealAsync_SetsRemainingDeckTo3Cards()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        deal.Deck.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateDealAsync_AllCardsAreUnique()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        var allCards = game.Players.Values
            .SelectMany(p => p.Hand)
            .Concat(deal.Deck)
            .Append(deal.UpCard!)
            .ToList();

        allCards.Should().HaveCount(24);
        allCards.Should().OnlyHaveUniqueItems(card => $"{card.Suit}-{card.Rank}");
    }

    [Fact]
    public async Task CreateDealAsync_SetsDealStatusToNotStarted()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        deal.DealStatus.Should().Be(DealStatus.NotStarted);
    }

    [Fact]
    public async Task CreateDealAsync_SetsDealerPositionCorrectly()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        deal.DealerPosition.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateDealAsync_InitializesNullablePropertiesToNull()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        deal.Trump.Should().BeNull();
        deal.CallingPlayer.Should().BeNull();
        deal.CurrentTrick.Should().BeNull();
        deal.DealResult.Should().BeNull();
        deal.WinningTeam.Should().BeNull();
    }

    [Fact]
    public async Task CreateDealAsync_SetsCallingPlayerIsGoingAloneToFalse()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        deal.CallingPlayerIsGoingAlone.Should().BeFalse();
    }

    [Fact]
    public async Task CreateDealAsync_TotalCardsAccountedFor_Is24Cards()
    {
        var factory = new DealFactory(new NoOpShuffler());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        var totalCards = game.Players.Values.Sum(p => p.Hand.Count)
            + deal.Deck.Length
            + 1;

        totalCards.Should().Be(24);
    }

    private static Game CreateTestGame()
    {
        var game = new Game();
        game.Players.Add(PlayerPosition.North, new Player { Position = PlayerPosition.North });
        game.Players.Add(PlayerPosition.East, new Player { Position = PlayerPosition.East });
        game.Players.Add(PlayerPosition.South, new Player { Position = PlayerPosition.South });
        game.Players.Add(PlayerPosition.West, new Player { Position = PlayerPosition.West });
        return game;
    }

    private sealed class NoOpShuffler : ICardShuffler
    {
        public void Shuffle<T>(T[] array)
        {
        }
    }
}
