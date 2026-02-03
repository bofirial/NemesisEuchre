using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Utilities;

namespace NemesisEuchre.GameEngine.Tests;

public class DealFactoryTests
{
    [Fact]
    public Task CreateDealAsync_WithNullGame_ThrowsArgumentNullException()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());

        var act = async () => await factory.CreateDealAsync(null!);

        return act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("game");
    }

    [Fact]
    public Task CreateDealAsync_WithFewerThan4Players_ThrowsArgumentException()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
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
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
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
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();
        var previousDeal = new Deal { DealerPosition = null };

        var act = async () => await factory.CreateDealAsync(game, previousDeal);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Previous deal must have a dealer position.");
    }

    [Fact]
    public async Task CreateDealAsync_WithNoPreviousDeal_SetsDealerToValidPosition()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        deal.DealerPosition.Should().BeOneOf(
            PlayerPosition.North,
            PlayerPosition.East,
            PlayerPosition.South,
            PlayerPosition.West);
    }

    [Fact]
    public async Task CreateDealAsync_WithNoPreviousDeal_RandomizesInitialDealerDistribution()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();
        var dealerCounts = new Dictionary<PlayerPosition, int>();

        for (int i = 0; i < 1000; i++)
        {
            var deal = await factory.CreateDealAsync(game);
            dealerCounts.TryGetValue(deal.DealerPosition!.Value, out var count);
            dealerCounts[deal.DealerPosition!.Value] = count + 1;
        }

        dealerCounts[PlayerPosition.North].Should().BeInRange(200, 300);
        dealerCounts[PlayerPosition.East].Should().BeInRange(200, 300);
        dealerCounts[PlayerPosition.South].Should().BeInRange(200, 300);
        dealerCounts[PlayerPosition.West].Should().BeInRange(200, 300);
    }

    [Fact]
    public async Task CreateDealAsync_WithPreviousDealerNorth_SetsDealerToEast()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();
        var previousDeal = new Deal { DealerPosition = PlayerPosition.North };

        var deal = await factory.CreateDealAsync(game, previousDeal);

        deal.DealerPosition.Should().Be(PlayerPosition.East);
    }

    [Fact]
    public async Task CreateDealAsync_WithPreviousDealerEast_SetsDealerToSouth()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();
        var previousDeal = new Deal { DealerPosition = PlayerPosition.East };

        var deal = await factory.CreateDealAsync(game, previousDeal);

        deal.DealerPosition.Should().Be(PlayerPosition.South);
    }

    [Fact]
    public async Task CreateDealAsync_WithPreviousDealerSouth_SetsDealerToWest()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();
        var previousDeal = new Deal { DealerPosition = PlayerPosition.South };

        var deal = await factory.CreateDealAsync(game, previousDeal);

        deal.DealerPosition.Should().Be(PlayerPosition.West);
    }

    [Fact]
    public async Task CreateDealAsync_WithPreviousDealerWest_SetsDealerToNorth()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();
        var previousDeal = new Deal { DealerPosition = PlayerPosition.West };

        var deal = await factory.CreateDealAsync(game, previousDeal);

        deal.DealerPosition.Should().Be(PlayerPosition.North);
    }

    [Fact]
    public async Task CreateDealAsync_DealsExactly5CardsToEachPlayer()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        deal.Players[PlayerPosition.North].StartingHand.Should().HaveCount(5);
        deal.Players[PlayerPosition.East].StartingHand.Should().HaveCount(5);
        deal.Players[PlayerPosition.South].StartingHand.Should().HaveCount(5);
        deal.Players[PlayerPosition.West].StartingHand.Should().HaveCount(5);

        deal.Players[PlayerPosition.North].CurrentHand.Should().HaveCount(5);
        deal.Players[PlayerPosition.East].CurrentHand.Should().HaveCount(5);
        deal.Players[PlayerPosition.South].CurrentHand.Should().HaveCount(5);
        deal.Players[PlayerPosition.West].CurrentHand.Should().HaveCount(5);
    }

    [Fact]
    public async Task CreateDealAsync_InitializesStartingHandsAndCurrentHandsWithSameCards()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        foreach (var position in Enum.GetValues<PlayerPosition>())
        {
            deal.Players[position].StartingHand.Should().BeEquivalentTo(deal.Players[position].CurrentHand);
        }
    }

    [Fact]
    public async Task CreateDealAsync_EachPlayerReceivesUniqueCards()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        var allPlayerCards = deal.Players.Values
            .SelectMany(p => p.StartingHand)
            .ToList();

        allPlayerCards.Should().OnlyHaveUniqueItems(card => $"{card.Suit}-{card.Rank}");
    }

    [Fact]
    public async Task CreateDealAsync_SetsUpCardCorrectly()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        deal.UpCard.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateDealAsync_SetsRemainingDeckTo3Cards()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        deal.Deck.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateDealAsync_AllCardsAreUnique()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        var allCards = deal.Players.Values
            .SelectMany(p => p.StartingHand)
            .Concat(deal.Deck)
            .Append(deal.UpCard!)
            .ToList();

        allCards.Should().HaveCount(24);
        allCards.Should().OnlyHaveUniqueItems(card => $"{card.Suit}-{card.Rank}");
    }

    [Fact]
    public async Task CreateDealAsync_SetsDealStatusToNotStarted()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        deal.DealStatus.Should().Be(DealStatus.NotStarted);
    }

    [Fact]
    public async Task CreateDealAsync_SetsDealerPositionCorrectly()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        deal.DealerPosition.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateDealAsync_InitializesNullablePropertiesToNull()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        deal.Trump.Should().BeNull();
        deal.CallingPlayer.Should().BeNull();
        deal.DealResult.Should().BeNull();
        deal.WinningTeam.Should().BeNull();
    }

    [Fact]
    public async Task CreateDealAsync_SetsCallingPlayerIsGoingAloneToFalse()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        deal.CallingPlayerIsGoingAlone.Should().BeFalse();
    }

    [Fact]
    public async Task CreateDealAsync_TotalCardsAccountedFor_Is24Cards()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        var totalCards = deal.Players.Values.Sum(p => p.StartingHand.Length)
            + deal.Deck.Count
            + 1;

        totalCards.Should().Be(24);
    }

    [Fact]
    public async Task CreateDealAsync_StartingHandsAreImmutableArrays()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        foreach (var position in Enum.GetValues<PlayerPosition>())
        {
            deal.Players[position].StartingHand.Should().BeOfType<Card[]>();
        }
    }

    [Fact]
    public async Task CreateDealAsync_CurrentHandsAreMutableLists()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        foreach (var position in Enum.GetValues<PlayerPosition>())
        {
            deal.Players[position].CurrentHand.Should().BeOfType<List<Card>>();
        }
    }

    [Fact]
    public async Task CreateDealAsync_DealPlayerPositionsAreSetCorrectly()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();

        var deal = await factory.CreateDealAsync(game);

        foreach (var position in Enum.GetValues<PlayerPosition>())
        {
            deal.Players[position].Position.Should().Be(position);
        }
    }

    [Fact]
    public async Task CreateDealAsync_CopiesActorTypeFromGamePlayer()
    {
        var factory = new DealFactory(new NoOpShuffler(), new RandomNumberGenerator());
        var game = CreateTestGame();
        game.Players[PlayerPosition.North].ActorType = ActorType.Chaos;
        game.Players[PlayerPosition.South].ActorType = ActorType.Chad;

        var deal = await factory.CreateDealAsync(game);

        deal.Players[PlayerPosition.North].ActorType.Should().Be(ActorType.Chaos);
        deal.Players[PlayerPosition.East].ActorType.Should().BeNull();
        deal.Players[PlayerPosition.South].ActorType.Should().Be(ActorType.Chad);
        deal.Players[PlayerPosition.West].ActorType.Should().BeNull();
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
