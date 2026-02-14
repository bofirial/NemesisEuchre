using FluentAssertions;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Services;

namespace NemesisEuchre.DataAccess.Tests.Services;

public class BulkInsertServiceTests
{
    [Fact]
    public async Task BulkInsertLeafEntitiesAsync_WithEmptyCache_DoesNotThrow()
    {
        var cache = new LeafCollectionCache([]);
        var service = new BulkInsertService(CreateConfiguredFactory());

        var act = () => service.BulkInsertLeafEntitiesAsync(
            cache,
            null!,
            null!,
            30,
            TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
        cache.LeafCount.Should().Be(0);
    }

    [Fact]
    public void BulkInsertService_ImplementsInterface()
    {
        var service = new BulkInsertService(CreateConfiguredFactory());
        service.Should().BeAssignableTo<IBulkInsertService>();
    }

    [Fact]
    public async Task BulkInsertLeafEntitiesAsync_WithEmptyLeafCollections_DoesNotAttemptConnection()
    {
        var game = new GameEntity { GameStatusId = 1 };
        var cache = new LeafCollectionCache([game]);
        var service = new BulkInsertService(CreateConfiguredFactory());

        cache.LeafCount.Should().Be(0);

        var act = () => service.BulkInsertLeafEntitiesAsync(
            cache,
            null!,
            null!,
            30,
            TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
        cache.LeafCount.Should().Be(0);
    }

    private static EntityReaderFactory CreateConfiguredFactory()
    {
        var factory = new EntityReaderFactory();
        EntityReaderConfiguration.ConfigureReaders(factory);
        return factory;
    }
}
