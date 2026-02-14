using Microsoft.Data.SqlClient;

namespace NemesisEuchre.DataAccess.Services;

public interface IBulkInsertService
{
    Task BulkInsertLeafEntitiesAsync(
        LeafCollectionCache cache,
        SqlConnection connection,
        SqlTransaction transaction,
        int bulkCopyTimeout,
        CancellationToken cancellationToken = default);
}

public class BulkInsertService(IEntityReaderFactory readerFactory) : IBulkInsertService
{
    public async Task BulkInsertLeafEntitiesAsync(
        LeafCollectionCache cache,
        SqlConnection connection,
        SqlTransaction transaction,
        int bulkCopyTimeout,
        CancellationToken cancellationToken = default)
    {
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "GamePlayers", cache.GamePlayers, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "DealDeckCards", cache.DealDeckCards, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "DealKnownPlayerSuitVoids", cache.DealKnownPlayerSuitVoids, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "DealPlayerStartingHandCards", cache.DealPlayerStartingHandCards, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "TrickCardsPlayed", cache.TrickCardsPlayed, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "CallTrumpDecisionCardsInHand", cache.CallTrumpCardsInHand, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "CallTrumpValidDecisions", cache.CallTrumpValidDecisions, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "CallTrumpDecisionPredictedPoints", cache.CallTrumpPredictedPoints, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "DiscardCardDecisionCardsInHand", cache.DiscardCardsInHand, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "DiscardCardDecisionPredictedPoints", cache.DiscardPredictedPoints, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionCardsInHand", cache.PlayCardCardsInHand, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionPlayedCards", cache.PlayCardPlayedCards, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionValidCards", cache.PlayCardValidCards, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionKnownVoids", cache.PlayCardKnownVoids, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionCardsAccountedFor", cache.PlayCardAccountedForCards, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionPredictedPoints", cache.PlayCardPredictedPoints, cancellationToken).ConfigureAwait(false);
    }

    private async Task BulkInsertAsync<T>(
        SqlConnection connection,
        SqlTransaction transaction,
        int bulkCopyTimeout,
        string tableName,
        IReadOnlyList<T> entities,
        CancellationToken cancellationToken)
    {
        if (entities.Count == 0)
        {
            return;
        }

        await using var reader = readerFactory.CreateReader(entities);

        using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
        {
            DestinationTableName = tableName,
            BulkCopyTimeout = bulkCopyTimeout,
        };

        for (int i = 0; i < reader.FieldCount; i++)
        {
            bulkCopy.ColumnMappings.Add(reader.GetName(i), reader.GetName(i));
        }

        await bulkCopy.WriteToServerAsync(reader, cancellationToken).ConfigureAwait(false);
    }
}
