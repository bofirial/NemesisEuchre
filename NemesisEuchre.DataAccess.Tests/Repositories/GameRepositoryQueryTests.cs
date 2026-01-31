using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.DataAccess.Tests.Repositories;

public class GameRepositoryQueryTests
{
    [Fact]
    public async Task GetCallTrumpTrainingDataAsync_WithMatchingActorType_ReturnsFilteredDecisions()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        var chaosDecision = CreateCallTrumpDecision(ActorType.Chaos, true);
        chaosDecision.DealId = deal.DealId;
        var chadDecision = CreateCallTrumpDecision(ActorType.Chad, true);
        chadDecision.DealId = deal.DealId;

        await context.CallTrumpDecisions!.AddRangeAsync(chaosDecision, chadDecision);
        await context.SaveChangesAsync();

        var results = new List<CallTrumpDecisionEntity>();
        await foreach (var decision in repository.GetCallTrumpTrainingDataAsync(ActorType.Chaos))
        {
            results.Add(decision);
        }

        results.Should().HaveCount(1);
        results[0].ActorType.Should().Be(ActorType.Chaos);
    }

    [Fact]
    public async Task GetCallTrumpTrainingDataAsync_WithWinningTeamOnlyTrue_ReturnsOnlyWinningDecisions()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        var winningDecision = CreateCallTrumpDecision(ActorType.Chaos, true);
        winningDecision.DealId = deal.DealId;
        var losingDecision = CreateCallTrumpDecision(ActorType.Chaos, false);
        losingDecision.DealId = deal.DealId;

        await context.CallTrumpDecisions!.AddRangeAsync(winningDecision, losingDecision);
        await context.SaveChangesAsync();

        var results = new List<CallTrumpDecisionEntity>();
        await foreach (var decision in repository.GetCallTrumpTrainingDataAsync(ActorType.Chaos, winningTeamOnly: true))
        {
            results.Add(decision);
        }

        results.Should().HaveCount(1);
        results[0].DidTeamWinGame.Should().BeTrue();
    }

    [Fact]
    public async Task GetCallTrumpTrainingDataAsync_WithLimit_ReturnsLimitedResults()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            var decision = CreateCallTrumpDecision(ActorType.Chaos, true);
            decision.DealId = deal.DealId;
            context.CallTrumpDecisions!.Add(decision);
        }

        await context.SaveChangesAsync();

        var results = new List<CallTrumpDecisionEntity>();
        await foreach (var decision in repository.GetCallTrumpTrainingDataAsync(ActorType.Chaos, limit: 3))
        {
            results.Add(decision);
        }

        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetCallTrumpTrainingDataAsync_WithLimitZero_ReturnsAllResults()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            var decision = CreateCallTrumpDecision(ActorType.Chaos, true);
            decision.DealId = deal.DealId;
            context.CallTrumpDecisions!.Add(decision);
        }

        await context.SaveChangesAsync();

        var results = new List<CallTrumpDecisionEntity>();
        await foreach (var decision in repository.GetCallTrumpTrainingDataAsync(ActorType.Chaos, limit: 0))
        {
            results.Add(decision);
        }

        results.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetCallTrumpTrainingDataAsync_WithNoMatchingData_ReturnsEmptyEnumerable()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var results = new List<CallTrumpDecisionEntity>();
        await foreach (var decision in repository.GetCallTrumpTrainingDataAsync(ActorType.Chaos))
        {
            results.Add(decision);
        }

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCallTrumpTrainingDataAsync_WithCancellationToken_StopsEnumeration()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        for (int i = 0; i < 10; i++)
        {
            var decision = CreateCallTrumpDecision(ActorType.Chaos, true);
            decision.DealId = deal.DealId;
            context.CallTrumpDecisions!.Add(decision);
        }

        await context.SaveChangesAsync();

        using var cts = new CancellationTokenSource();
        var results = new List<CallTrumpDecisionEntity>();

        try
        {
            await foreach (var decision in repository.GetCallTrumpTrainingDataAsync(ActorType.Chaos, cancellationToken: cts.Token))
            {
                results.Add(decision);
                if (results.Count == 3)
                {
#pragma warning disable CA1849
                    await cts.CancelAsync();
#pragma warning restore CA1849
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected - test verifies cancellation stops enumeration
        }

        results.Should().HaveCountLessThan(10);
    }

    [Fact]
    public async Task GetCallTrumpTrainingDataAsync_WithAllFilters_ReturnsCorrectResults()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            var winningDecision = CreateCallTrumpDecision(ActorType.Chaos, true);
            winningDecision.DealId = deal.DealId;
            context.CallTrumpDecisions!.Add(winningDecision);
        }

        for (int i = 0; i < 5; i++)
        {
            var losingDecision = CreateCallTrumpDecision(ActorType.Chaos, false);
            losingDecision.DealId = deal.DealId;
            context.CallTrumpDecisions!.Add(losingDecision);
        }

        for (int i = 0; i < 5; i++)
        {
            var otherActorDecision = CreateCallTrumpDecision(ActorType.Chad, true);
            otherActorDecision.DealId = deal.DealId;
            context.CallTrumpDecisions!.Add(otherActorDecision);
        }

        await context.SaveChangesAsync();

        var results = new List<CallTrumpDecisionEntity>();
        await foreach (var decision in repository.GetCallTrumpTrainingDataAsync(ActorType.Chaos, limit: 3, winningTeamOnly: true))
        {
            results.Add(decision);
        }

        results.Should().HaveCount(3);
        results.Should().OnlyContain(d => d.ActorType == ActorType.Chaos);
        results.Should().OnlyContain(d => d.DidTeamWinGame == true);
    }

    [Fact]
    public async Task GetDiscardCardTrainingDataAsync_WithMatchingActorType_ReturnsFilteredDecisions()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        var chaosDecision = CreateDiscardCardDecision(ActorType.Chaos, true);
        chaosDecision.DealId = deal.DealId;
        var chadDecision = CreateDiscardCardDecision(ActorType.Chad, true);
        chadDecision.DealId = deal.DealId;

        await context.DiscardCardDecisions!.AddRangeAsync(chaosDecision, chadDecision);
        await context.SaveChangesAsync();

        var results = new List<DiscardCardDecisionEntity>();
        await foreach (var decision in repository.GetDiscardCardTrainingDataAsync(ActorType.Chaos))
        {
            results.Add(decision);
        }

        results.Should().HaveCount(1);
        results[0].ActorType.Should().Be(ActorType.Chaos);
    }

    [Fact]
    public async Task GetDiscardCardTrainingDataAsync_WithWinningTeamOnlyTrue_ReturnsOnlyWinningDecisions()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        var winningDecision = CreateDiscardCardDecision(ActorType.Chaos, true);
        winningDecision.DealId = deal.DealId;
        var losingDecision = CreateDiscardCardDecision(ActorType.Chaos, false);
        losingDecision.DealId = deal.DealId;

        await context.DiscardCardDecisions!.AddRangeAsync(winningDecision, losingDecision);
        await context.SaveChangesAsync();

        var results = new List<DiscardCardDecisionEntity>();
        await foreach (var decision in repository.GetDiscardCardTrainingDataAsync(ActorType.Chaos, winningTeamOnly: true))
        {
            results.Add(decision);
        }

        results.Should().HaveCount(1);
        results[0].DidTeamWinGame.Should().BeTrue();
    }

    [Fact]
    public async Task GetDiscardCardTrainingDataAsync_WithLimit_ReturnsLimitedResults()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            var decision = CreateDiscardCardDecision(ActorType.Chaos, true);
            decision.DealId = deal.DealId;
            context.DiscardCardDecisions!.Add(decision);
        }

        await context.SaveChangesAsync();

        var results = new List<DiscardCardDecisionEntity>();
        await foreach (var decision in repository.GetDiscardCardTrainingDataAsync(ActorType.Chaos, limit: 3))
        {
            results.Add(decision);
        }

        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetDiscardCardTrainingDataAsync_WithLimitZero_ReturnsAllResults()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            var decision = CreateDiscardCardDecision(ActorType.Chaos, true);
            decision.DealId = deal.DealId;
            context.DiscardCardDecisions!.Add(decision);
        }

        await context.SaveChangesAsync();

        var results = new List<DiscardCardDecisionEntity>();
        await foreach (var decision in repository.GetDiscardCardTrainingDataAsync(ActorType.Chaos, limit: 0))
        {
            results.Add(decision);
        }

        results.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetDiscardCardTrainingDataAsync_WithNoMatchingData_ReturnsEmptyEnumerable()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var results = new List<DiscardCardDecisionEntity>();
        await foreach (var decision in repository.GetDiscardCardTrainingDataAsync(ActorType.Chaos))
        {
            results.Add(decision);
        }

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDiscardCardTrainingDataAsync_WithCancellationToken_StopsEnumeration()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        for (int i = 0; i < 10; i++)
        {
            var decision = CreateDiscardCardDecision(ActorType.Chaos, true);
            decision.DealId = deal.DealId;
            context.DiscardCardDecisions!.Add(decision);
        }

        await context.SaveChangesAsync();

        using var cts = new CancellationTokenSource();
        var results = new List<DiscardCardDecisionEntity>();

        try
        {
            await foreach (var decision in repository.GetDiscardCardTrainingDataAsync(ActorType.Chaos, cancellationToken: cts.Token))
            {
                results.Add(decision);
                if (results.Count == 3)
                {
#pragma warning disable CA1849
                    await cts.CancelAsync();
#pragma warning restore CA1849
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected - test verifies cancellation stops enumeration
        }

        results.Should().HaveCountLessThan(10);
    }

    [Fact]
    public async Task GetDiscardCardTrainingDataAsync_WithAllFilters_ReturnsCorrectResults()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            var winningDecision = CreateDiscardCardDecision(ActorType.Chaos, true);
            winningDecision.DealId = deal.DealId;
            context.DiscardCardDecisions!.Add(winningDecision);
        }

        for (int i = 0; i < 5; i++)
        {
            var losingDecision = CreateDiscardCardDecision(ActorType.Chaos, false);
            losingDecision.DealId = deal.DealId;
            context.DiscardCardDecisions!.Add(losingDecision);
        }

        for (int i = 0; i < 5; i++)
        {
            var otherActorDecision = CreateDiscardCardDecision(ActorType.Chad, true);
            otherActorDecision.DealId = deal.DealId;
            context.DiscardCardDecisions!.Add(otherActorDecision);
        }

        await context.SaveChangesAsync();

        var results = new List<DiscardCardDecisionEntity>();
        await foreach (var decision in repository.GetDiscardCardTrainingDataAsync(ActorType.Chaos, limit: 3, winningTeamOnly: true))
        {
            results.Add(decision);
        }

        results.Should().HaveCount(3);
        results.Should().OnlyContain(d => d.ActorType == ActorType.Chaos);
        results.Should().OnlyContain(d => d.DidTeamWinGame == true);
    }

    [Fact]
    public async Task GetPlayCardTrainingDataAsync_WithMatchingActorType_ReturnsFilteredDecisions()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        var trick = new TrickEntity
        {
            DealId = deal.DealId,
            TrickNumber = 1,
            LeadPosition = PlayerPosition.North,
            CardsPlayedJson = "[]",
        };
        context.Tricks!.Add(trick);
        await context.SaveChangesAsync();

        var chaosDecision = CreatePlayCardDecision(ActorType.Chaos, true);
        chaosDecision.DealId = deal.DealId;
        chaosDecision.TrickId = trick.TrickId;
        var chadDecision = CreatePlayCardDecision(ActorType.Chad, true);
        chadDecision.DealId = deal.DealId;
        chadDecision.TrickId = trick.TrickId;

        await context.PlayCardDecisions!.AddRangeAsync(chaosDecision, chadDecision);
        await context.SaveChangesAsync();

        var results = new List<PlayCardDecisionEntity>();
        await foreach (var decision in repository.GetPlayCardTrainingDataAsync(ActorType.Chaos))
        {
            results.Add(decision);
        }

        results.Should().HaveCount(1);
        results[0].ActorType.Should().Be(ActorType.Chaos);
    }

    [Fact]
    public async Task GetPlayCardTrainingDataAsync_WithWinningTeamOnlyTrue_ReturnsOnlyWinningDecisions()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        var trick = new TrickEntity
        {
            DealId = deal.DealId,
            TrickNumber = 1,
            LeadPosition = PlayerPosition.North,
            CardsPlayedJson = "[]",
        };
        context.Tricks!.Add(trick);
        await context.SaveChangesAsync();

        var winningDecision = CreatePlayCardDecision(ActorType.Chaos, true);
        winningDecision.DealId = deal.DealId;
        winningDecision.TrickId = trick.TrickId;
        var losingDecision = CreatePlayCardDecision(ActorType.Chaos, false);
        losingDecision.DealId = deal.DealId;
        losingDecision.TrickId = trick.TrickId;

        await context.PlayCardDecisions!.AddRangeAsync(winningDecision, losingDecision);
        await context.SaveChangesAsync();

        var results = new List<PlayCardDecisionEntity>();
        await foreach (var decision in repository.GetPlayCardTrainingDataAsync(ActorType.Chaos, winningTeamOnly: true))
        {
            results.Add(decision);
        }

        results.Should().HaveCount(1);
        results[0].DidTeamWinGame.Should().BeTrue();
    }

    [Fact]
    public async Task GetPlayCardTrainingDataAsync_WithLimit_ReturnsLimitedResults()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        var trick = new TrickEntity
        {
            DealId = deal.DealId,
            TrickNumber = 1,
            LeadPosition = PlayerPosition.North,
            CardsPlayedJson = "[]",
        };
        context.Tricks!.Add(trick);
        await context.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            var decision = CreatePlayCardDecision(ActorType.Chaos, true);
            decision.DealId = deal.DealId;
            decision.TrickId = trick.TrickId;
            context.PlayCardDecisions!.Add(decision);
        }

        await context.SaveChangesAsync();

        var results = new List<PlayCardDecisionEntity>();
        await foreach (var decision in repository.GetPlayCardTrainingDataAsync(ActorType.Chaos, limit: 3))
        {
            results.Add(decision);
        }

        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetPlayCardTrainingDataAsync_WithLimitZero_ReturnsAllResults()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        var trick = new TrickEntity
        {
            DealId = deal.DealId,
            TrickNumber = 1,
            LeadPosition = PlayerPosition.North,
            CardsPlayedJson = "[]",
        };
        context.Tricks!.Add(trick);
        await context.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            var decision = CreatePlayCardDecision(ActorType.Chaos, true);
            decision.DealId = deal.DealId;
            decision.TrickId = trick.TrickId;
            context.PlayCardDecisions!.Add(decision);
        }

        await context.SaveChangesAsync();

        var results = new List<PlayCardDecisionEntity>();
        await foreach (var decision in repository.GetPlayCardTrainingDataAsync(ActorType.Chaos, limit: 0))
        {
            results.Add(decision);
        }

        results.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetPlayCardTrainingDataAsync_WithNoMatchingData_ReturnsEmptyEnumerable()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var results = new List<PlayCardDecisionEntity>();
        await foreach (var decision in repository.GetPlayCardTrainingDataAsync(ActorType.Chaos))
        {
            results.Add(decision);
        }

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPlayCardTrainingDataAsync_WithCancellationToken_StopsEnumeration()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        var trick = new TrickEntity
        {
            DealId = deal.DealId,
            TrickNumber = 1,
            LeadPosition = PlayerPosition.North,
            CardsPlayedJson = "[]",
        };
        context.Tricks!.Add(trick);
        await context.SaveChangesAsync();

        for (int i = 0; i < 10; i++)
        {
            var decision = CreatePlayCardDecision(ActorType.Chaos, true);
            decision.DealId = deal.DealId;
            decision.TrickId = trick.TrickId;
            context.PlayCardDecisions!.Add(decision);
        }

        await context.SaveChangesAsync();

        using var cts = new CancellationTokenSource();
        var results = new List<PlayCardDecisionEntity>();

        try
        {
            await foreach (var decision in repository.GetPlayCardTrainingDataAsync(ActorType.Chaos, cancellationToken: cts.Token))
            {
                results.Add(decision);
                if (results.Count == 3)
                {
#pragma warning disable CA1849
                    await cts.CancelAsync();
#pragma warning restore CA1849
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected - test verifies cancellation stops enumeration
        }

        results.Should().HaveCountLessThan(10);
    }

    [Fact]
    public async Task GetPlayCardTrainingDataAsync_WithAllFilters_ReturnsCorrectResults()
    {
        await using var context = CreateInMemoryContext();
        var repository = CreateRepository(context);

        var deal = CreateDealEntity();
        context.Deals!.Add(deal);
        await context.SaveChangesAsync();

        var trick = new TrickEntity
        {
            DealId = deal.DealId,
            TrickNumber = 1,
            LeadPosition = PlayerPosition.North,
            CardsPlayedJson = "[]",
        };
        context.Tricks!.Add(trick);
        await context.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            var winningDecision = CreatePlayCardDecision(ActorType.Chaos, true);
            winningDecision.DealId = deal.DealId;
            winningDecision.TrickId = trick.TrickId;
            context.PlayCardDecisions!.Add(winningDecision);
        }

        for (int i = 0; i < 5; i++)
        {
            var losingDecision = CreatePlayCardDecision(ActorType.Chaos, false);
            losingDecision.DealId = deal.DealId;
            losingDecision.TrickId = trick.TrickId;
            context.PlayCardDecisions!.Add(losingDecision);
        }

        for (int i = 0; i < 5; i++)
        {
            var otherActorDecision = CreatePlayCardDecision(ActorType.Chad, true);
            otherActorDecision.DealId = deal.DealId;
            otherActorDecision.TrickId = trick.TrickId;
            context.PlayCardDecisions!.Add(otherActorDecision);
        }

        await context.SaveChangesAsync();

        var results = new List<PlayCardDecisionEntity>();
        await foreach (var decision in repository.GetPlayCardTrainingDataAsync(ActorType.Chaos, limit: 3, winningTeamOnly: true))
        {
            results.Add(decision);
        }

        results.Should().HaveCount(3);
        results.Should().OnlyContain(d => d.ActorType == ActorType.Chaos);
        results.Should().OnlyContain(d => d.DidTeamWinGame == true);
    }

    private static NemesisEuchreDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<NemesisEuchreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new NemesisEuchreDbContext(options);
    }

    private static GameRepository CreateRepository(NemesisEuchreDbContext context)
    {
        var mockLogger = new Mock<ILogger<GameRepository>>();
        var mockMapper = new Mock<IGameToEntityMapper>();
        var mockOptions = new Mock<IOptions<PersistenceOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new PersistenceOptions());
        return new GameRepository(context, mockLogger.Object, mockMapper.Object, mockOptions.Object);
    }

    private static DealEntity CreateDealEntity()
    {
        return new DealEntity
        {
            GameId = 1,
            DealNumber = 1,
            DealStatus = DealStatus.Complete,
            DealerPosition = PlayerPosition.North,
            DeckJson = "[]",
            PlayersJson = "[]",
            Team1Score = 0,
            Team2Score = 0,
        };
    }

    private static CallTrumpDecisionEntity CreateCallTrumpDecision(ActorType actorType, bool? didTeamWinGame)
    {
        return new CallTrumpDecisionEntity
        {
            ActorType = actorType,
            DidTeamWinGame = didTeamWinGame,
            DidTeamWinDeal = didTeamWinGame,
            CardsInHandJson = /*lang=json,strict*/ "[{\"Suit\":\"Hearts\",\"Rank\":\"Nine\"}]",
            TeamScore = 0,
            OpponentScore = 0,
            DealerPosition = RelativePlayerPosition.Self,
            UpCardJson = /*lang=json,strict*/ "{\"Suit\":\"Hearts\",\"Rank\":\"Jack\"}",
            ValidDecisionsJson = "[\"Pass\",\"OrderItUp\"]",
            ChosenDecisionJson = "\"OrderItUp\"",
            DecisionOrder = 1,
        };
    }

    private static DiscardCardDecisionEntity CreateDiscardCardDecision(ActorType actorType, bool? didTeamWinGame)
    {
        return new DiscardCardDecisionEntity
        {
            ActorType = actorType,
            DidTeamWinGame = didTeamWinGame,
            DidTeamWinDeal = didTeamWinGame,
            CardsInHandJson = /*lang=json,strict*/ "[{\"Suit\":\"Hearts\",\"Rank\":\"Nine\"}]",
            TeamScore = 0,
            OpponentScore = 0,
            CallingPlayer = RelativePlayerPosition.Self,
            CallingPlayerGoingAlone = false,
            ChosenCardJson = /*lang=json,strict*/ "{\"Suit\":\"Hearts\",\"Rank\":\"Nine\"}",
        };
    }

    private static PlayCardDecisionEntity CreatePlayCardDecision(ActorType actorType, bool? didTeamWinGame)
    {
        return new PlayCardDecisionEntity
        {
            ActorType = actorType,
            DidTeamWinGame = didTeamWinGame,
            DidTeamWinDeal = didTeamWinGame,
            DidTeamWinTrick = didTeamWinGame,
            CardsInHandJson = /*lang=json,strict*/ "[{\"Suit\":\"Hearts\",\"Rank\":\"Nine\"}]",
            TeamScore = 0,
            OpponentScore = 0,
            LeadPlayer = RelativePlayerPosition.Self,
            LeadSuit = RelativeSuit.Trump,
            PlayedCardsJson = "[]",
            WinningTrickPlayer = RelativePlayerPosition.Self,
            ValidCardsToPlayJson = /*lang=json,strict*/ "[{\"Suit\":\"Hearts\",\"Rank\":\"Nine\"}]",
            CallingPlayer = RelativePlayerPosition.Self,
            CallingPlayerGoingAlone = false,
            ChosenCardJson = /*lang=json,strict*/ "{\"Suit\":\"Hearts\",\"Rank\":\"Nine\"}",
        };
    }
}
