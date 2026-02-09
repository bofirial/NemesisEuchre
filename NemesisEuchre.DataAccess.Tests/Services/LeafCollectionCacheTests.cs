using FluentAssertions;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Services;

namespace NemesisEuchre.DataAccess.Tests.Services;

public class LeafCollectionCacheTests
{
    [Fact]
    public void Constructor_ExtractsGamePlayers_AndClearsFromParent()
    {
        var game = CreateGameWithLeaves();
        var cache = new LeafCollectionCache([game]);

        cache.GamePlayers.Should().HaveCount(2);
        game.GamePlayers.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ExtractsDealDeckCards_AndClearsFromParent()
    {
        var game = CreateGameWithLeaves();
        var deal = game.Deals.First();
        var cache = new LeafCollectionCache([game]);

        cache.DealDeckCards.Should().HaveCount(3);
        deal.DealDeckCards.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ExtractsDealKnownPlayerSuitVoids_AndClearsFromParent()
    {
        var game = CreateGameWithLeaves();
        var deal = game.Deals.First();
        var cache = new LeafCollectionCache([game]);

        cache.DealKnownPlayerSuitVoids.Should().HaveCount(1);
        deal.DealKnownPlayerSuitVoids.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ExtractsDealPlayerStartingHandCards_AndClearsFromParent()
    {
        var game = CreateGameWithLeaves();
        var dealPlayer = game.Deals.First().DealPlayers.First();
        var cache = new LeafCollectionCache([game]);

        cache.DealPlayerStartingHandCards.Should().HaveCount(2);
        dealPlayer.StartingHandCards.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ExtractsTrickCardsPlayed_AndClearsFromParent()
    {
        var game = CreateGameWithLeaves();
        var trick = game.Deals.First().Tricks.First();
        var cache = new LeafCollectionCache([game]);

        cache.TrickCardsPlayed.Should().HaveCount(2);
        trick.TrickCardsPlayed.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ExtractsCallTrumpDecisionLeaves()
    {
        var game = CreateGameWithLeaves();
        var cache = new LeafCollectionCache([game]);

        cache.CallTrumpCardsInHand.Should().HaveCount(1);
        cache.CallTrumpValidDecisions.Should().HaveCount(1);
        cache.CallTrumpPredictedPoints.Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_ExtractsDiscardCardDecisionLeaves()
    {
        var game = CreateGameWithLeaves();
        var cache = new LeafCollectionCache([game]);

        cache.DiscardCardsInHand.Should().HaveCount(1);
        cache.DiscardPredictedPoints.Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_ExtractsPlayCardDecisionLeaves()
    {
        var game = CreateGameWithLeaves();
        var cache = new LeafCollectionCache([game]);

        cache.PlayCardCardsInHand.Should().HaveCount(1);
        cache.PlayCardPlayedCards.Should().HaveCount(1);
        cache.PlayCardValidCards.Should().HaveCount(1);
        cache.PlayCardKnownVoids.Should().HaveCount(1);
        cache.PlayCardAccountedForCards.Should().HaveCount(1);
        cache.PlayCardPredictedPoints.Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_ClearsAllPlayCardDecisionCollections()
    {
        var game = CreateGameWithLeaves();
        var playCardDecision = game.Deals.First().Tricks.First().PlayCardDecisions[0];
        _ = new LeafCollectionCache([game]);

        playCardDecision.CardsInHand.Should().BeEmpty();
        playCardDecision.PlayedCards.Should().BeEmpty();
        playCardDecision.ValidCards.Should().BeEmpty();
        playCardDecision.KnownVoids.Should().BeEmpty();
        playCardDecision.CardsAccountedFor.Should().BeEmpty();
        playCardDecision.PredictedPoints.Should().BeEmpty();
    }

    [Fact]
    public void LeafCount_ReturnsCorrectTotal()
    {
        var game = CreateGameWithLeaves();
        var cache = new LeafCollectionCache([game]);

        // 2 GamePlayers + 3 DealDeckCards + 1 DealKnownVoid + 2 StartingHand
        // + 2 TrickCards + 1 CT CardsInHand + 1 CT Valid + 1 CT Predicted
        // + 1 DC CardsInHand + 1 DC Predicted
        // + 1 PC CardsInHand + 1 PC Played + 1 PC Valid + 1 PC KnownVoid
        // + 1 PC AccountedFor + 1 PC Predicted = 21
        cache.LeafCount.Should().Be(21);
    }

    [Fact]
    public void Constructor_WithEmptyCollections_ProducesEmptyCache()
    {
        var game = new GameEntity();
        var cache = new LeafCollectionCache([game]);

        cache.LeafCount.Should().Be(0);
        cache.GamePlayers.Should().BeEmpty();
        cache.DealDeckCards.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithEmptyEntitiesList_ProducesEmptyCache()
    {
        var cache = new LeafCollectionCache([]);

        cache.LeafCount.Should().Be(0);
    }

    [Fact]
    public void PopulateForeignKeys_SetsGamePlayerForeignKeys()
    {
        var game = CreateGameWithLeaves();
        game.GameId = 42;
        var cache = new LeafCollectionCache([game]);

        cache.PopulateForeignKeys();

        cache.GamePlayers.Should().AllSatisfy(gp => gp.GameId.Should().Be(42));
    }

    [Fact]
    public void PopulateForeignKeys_SetsDealDeckCardForeignKeys()
    {
        var game = CreateGameWithLeaves();
        var deal = game.Deals.First();
        deal.DealId = 99;
        var cache = new LeafCollectionCache([game]);

        cache.PopulateForeignKeys();

        cache.DealDeckCards.Should().AllSatisfy(dc => dc.DealId.Should().Be(99));
    }

    [Fact]
    public void PopulateForeignKeys_SetsDealKnownPlayerSuitVoidForeignKeys()
    {
        var game = CreateGameWithLeaves();
        var deal = game.Deals.First();
        deal.DealId = 77;
        var cache = new LeafCollectionCache([game]);

        cache.PopulateForeignKeys();

        cache.DealKnownPlayerSuitVoids.Should().AllSatisfy(v => v.DealId.Should().Be(77));
    }

    [Fact]
    public void PopulateForeignKeys_SetsDealPlayerStartingHandCardForeignKeys()
    {
        var game = CreateGameWithLeaves();
        var dealPlayer = game.Deals.First().DealPlayers.First();
        dealPlayer.DealPlayerId = 55;
        var cache = new LeafCollectionCache([game]);

        cache.PopulateForeignKeys();

        cache.DealPlayerStartingHandCards.Should().AllSatisfy(c => c.DealPlayerId.Should().Be(55));
    }

    [Fact]
    public void PopulateForeignKeys_SetsTrickCardPlayedForeignKeys()
    {
        var game = CreateGameWithLeaves();
        var trick = game.Deals.First().Tricks.First();
        trick.TrickId = 33;
        var cache = new LeafCollectionCache([game]);

        cache.PopulateForeignKeys();

        cache.TrickCardsPlayed.Should().AllSatisfy(tc => tc.TrickId.Should().Be(33));
    }

    [Fact]
    public void PopulateForeignKeys_SetsCallTrumpDecisionForeignKeys()
    {
        var game = CreateGameWithLeaves();
        var ctDecision = game.Deals.First().CallTrumpDecisions.First();
        ctDecision.CallTrumpDecisionId = 11;
        var cache = new LeafCollectionCache([game]);

        cache.PopulateForeignKeys();

        cache.CallTrumpCardsInHand.Should().AllSatisfy(c => c.CallTrumpDecisionId.Should().Be(11));
        cache.CallTrumpValidDecisions.Should().AllSatisfy(c => c.CallTrumpDecisionId.Should().Be(11));
        cache.CallTrumpPredictedPoints.Should().AllSatisfy(c => c.CallTrumpDecisionId.Should().Be(11));
    }

    [Fact]
    public void PopulateForeignKeys_SetsDiscardCardDecisionForeignKeys()
    {
        var game = CreateGameWithLeaves();
        var dcDecision = game.Deals.First().DiscardCardDecisions.First();
        dcDecision.DiscardCardDecisionId = 22;
        var cache = new LeafCollectionCache([game]);

        cache.PopulateForeignKeys();

        cache.DiscardCardsInHand.Should().AllSatisfy(c => c.DiscardCardDecisionId.Should().Be(22));
        cache.DiscardPredictedPoints.Should().AllSatisfy(c => c.DiscardCardDecisionId.Should().Be(22));
    }

    [Fact]
    public void PopulateForeignKeys_SetsPlayCardDecisionForeignKeys()
    {
        var game = CreateGameWithLeaves();
        var pcDecision = game.Deals.First().Tricks.First().PlayCardDecisions[0];
        pcDecision.PlayCardDecisionId = 88;
        var cache = new LeafCollectionCache([game]);

        cache.PopulateForeignKeys();

        cache.PlayCardCardsInHand.Should().AllSatisfy(c => c.PlayCardDecisionId.Should().Be(88));
        cache.PlayCardPlayedCards.Should().AllSatisfy(c => c.PlayCardDecisionId.Should().Be(88));
        cache.PlayCardValidCards.Should().AllSatisfy(c => c.PlayCardDecisionId.Should().Be(88));
        cache.PlayCardKnownVoids.Should().AllSatisfy(c => c.PlayCardDecisionId.Should().Be(88));
        cache.PlayCardAccountedForCards.Should().AllSatisfy(c => c.PlayCardDecisionId.Should().Be(88));
        cache.PlayCardPredictedPoints.Should().AllSatisfy(c => c.PlayCardDecisionId.Should().Be(88));
    }

    [Fact]
    public void Constructor_WithMultipleGames_ExtractsFromAll()
    {
        var game1 = CreateGameWithLeaves();
        var game2 = CreateGameWithLeaves();
        var cache = new LeafCollectionCache([game1, game2]);

        cache.GamePlayers.Should().HaveCount(4);
        cache.DealDeckCards.Should().HaveCount(6);
        cache.LeafCount.Should().Be(42);
    }

    [Fact]
    public void PopulateForeignKeys_WithMultipleGames_SetsCorrectParentIds()
    {
        var game1 = CreateGameWithLeaves();
        game1.GameId = 100;
        var game2 = CreateGameWithLeaves();
        game2.GameId = 200;
        var cache = new LeafCollectionCache([game1, game2]);

        cache.PopulateForeignKeys();

        cache.GamePlayers.Take(2).Should().AllSatisfy(gp => gp.GameId.Should().Be(100));
        cache.GamePlayers.Skip(2).Should().AllSatisfy(gp => gp.GameId.Should().Be(200));
    }

    private static GameEntity CreateGameWithLeaves()
    {
        var playCardDecision = new PlayCardDecisionEntity
        {
            DealId = 1,
            TrickId = 1,
            CardsInHand = { new PlayCardDecisionCardsInHand { RelativeCardId = 101, SortOrder = 0 } },
            PlayedCards = { new PlayCardDecisionPlayedCard { RelativePlayerPositionId = 1, RelativeCardId = 102 } },
            ValidCards = { new PlayCardDecisionValidCard { RelativeCardId = 101 } },
            KnownVoids = { new PlayCardDecisionKnownVoid { RelativePlayerPositionId = 2, RelativeSuitId = 1 } },
            CardsAccountedFor = { new PlayCardDecisionAccountedForCard { RelativeCardId = 103 } },
            PredictedPoints = { new PlayCardDecisionPredictedPoints { RelativeCardId = 101, PredictedPoints = 1.5f } },
        };

        var trick = new TrickEntity
        {
            DealId = 1,
            TrickNumber = 1,
            LeadPlayerPositionId = 1,
            TrickCardsPlayed =
            {
                new TrickCardPlayed { PlayerPositionId = 1, CardId = 101, PlayOrder = 0 },
                new TrickCardPlayed { PlayerPositionId = 2, CardId = 102, PlayOrder = 1 },
            },
            PlayCardDecisions = { playCardDecision },
        };

        var callTrumpDecision = new CallTrumpDecisionEntity
        {
            DealId = 1,
            DealerRelativePositionId = 1,
            UpCardId = 101,
            CardsInHand = { new CallTrumpDecisionCardsInHand { CardId = 101, SortOrder = 0 } },
            ValidDecisions = { new CallTrumpDecisionValidDecision { CallTrumpDecisionValueId = 1 } },
            PredictedPoints = { new CallTrumpDecisionPredictedPoints { CallTrumpDecisionValueId = 1, PredictedPoints = 2.0f } },
        };

        var discardDecision = new DiscardCardDecisionEntity
        {
            DealId = 1,
            CallingRelativePlayerPositionId = 1,
            CardsInHand = { new DiscardCardDecisionCardsInHand { RelativeCardId = 101, SortOrder = 0 } },
            PredictedPoints = { new DiscardCardDecisionPredictedPoints { RelativeCardId = 101, PredictedPoints = 0.5f } },
        };

        var dealPlayer = new DealPlayerEntity
        {
            DealId = 1,
            PlayerPositionId = 1,
            StartingHandCards =
            {
                new DealPlayerStartingHandCard { CardId = 101, SortOrder = 0 },
                new DealPlayerStartingHandCard { CardId = 102, SortOrder = 1 },
            },
        };

        var deal = new DealEntity
        {
            GameId = 1,
            DealNumber = 1,
            DealStatusId = 1,
            DealDeckCards =
            {
                new DealDeckCard { CardId = 101, SortOrder = 0 },
                new DealDeckCard { CardId = 102, SortOrder = 1 },
                new DealDeckCard { CardId = 103, SortOrder = 2 },
            },
            DealKnownPlayerSuitVoids =
            {
                new DealKnownPlayerSuitVoid { PlayerPositionId = 1, SuitId = 2 },
            },
            DealPlayers = { dealPlayer },
            Tricks = { trick },
            CallTrumpDecisions = { callTrumpDecision },
            DiscardCardDecisions = { discardDecision },
        };

        return new GameEntity
        {
            GameStatusId = 1,
            GamePlayers =
            {
                new GamePlayer { PlayerPositionId = 1 },
                new GamePlayer { PlayerPositionId = 2 },
            },
            Deals = { deal },
        };
    }
}
