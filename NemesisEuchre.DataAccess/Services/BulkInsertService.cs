using System.Data;

using Microsoft.Data.SqlClient;

using NemesisEuchre.DataAccess.Entities;

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

public class BulkInsertService : IBulkInsertService
{
    public async Task BulkInsertLeafEntitiesAsync(
        LeafCollectionCache cache,
        SqlConnection connection,
        SqlTransaction transaction,
        int bulkCopyTimeout,
        CancellationToken cancellationToken = default)
    {
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "GamePlayers", cache.GamePlayers, BuildGamePlayersTable, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "DealDeckCards", cache.DealDeckCards, BuildDealDeckCardsTable, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "DealKnownPlayerSuitVoids", cache.DealKnownPlayerSuitVoids, BuildDealKnownPlayerSuitVoidsTable, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "DealPlayerStartingHandCards", cache.DealPlayerStartingHandCards, BuildDealPlayerStartingHandCardsTable, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "TrickCardsPlayed", cache.TrickCardsPlayed, BuildTrickCardsPlayedTable, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "CallTrumpDecisionCardsInHand", cache.CallTrumpCardsInHand, BuildCallTrumpCardsInHandTable, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "CallTrumpValidDecisions", cache.CallTrumpValidDecisions, BuildCallTrumpValidDecisionsTable, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "CallTrumpDecisionPredictedPoints", cache.CallTrumpPredictedPoints, BuildCallTrumpPredictedPointsTable, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "DiscardCardDecisionCardsInHand", cache.DiscardCardsInHand, BuildDiscardCardsInHandTable, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "DiscardCardDecisionPredictedPoints", cache.DiscardPredictedPoints, BuildDiscardPredictedPointsTable, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionCardsInHand", cache.PlayCardCardsInHand, BuildPlayCardCardsInHandTable, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionPlayedCards", cache.PlayCardPlayedCards, BuildPlayCardPlayedCardsTable, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionValidCards", cache.PlayCardValidCards, BuildPlayCardValidCardsTable, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionKnownVoids", cache.PlayCardKnownVoids, BuildPlayCardKnownVoidsTable, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionCardsAccountedFor", cache.PlayCardAccountedForCards, BuildPlayCardAccountedForCardsTable, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionPredictedPoints", cache.PlayCardPredictedPoints, BuildPlayCardPredictedPointsTable, cancellationToken).ConfigureAwait(false);
    }

    private static async Task BulkInsertAsync<T>(
        SqlConnection connection,
        SqlTransaction transaction,
        int bulkCopyTimeout,
        string tableName,
        IReadOnlyList<T> entities,
        Func<IReadOnlyList<T>, DataTable> buildTable,
        CancellationToken cancellationToken)
    {
        if (entities.Count == 0)
        {
            return;
        }

        var dataTable = buildTable(entities);

        using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
        {
            DestinationTableName = tableName,
            BulkCopyTimeout = bulkCopyTimeout,
        };

        foreach (DataColumn column in dataTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }

        await bulkCopy.WriteToServerAsync(dataTable, cancellationToken).ConfigureAwait(false);
    }

    private static DataTable BuildGamePlayersTable(IReadOnlyList<GamePlayer> entities)
    {
        var table = new DataTable();
        table.Columns.Add("GameId", typeof(int));
        table.Columns.Add("PlayerPositionId", typeof(int));
        table.Columns.Add("ActorTypeId", typeof(int));

        foreach (var e in entities)
        {
            table.Rows.Add(e.GameId, e.PlayerPositionId, (object?)e.ActorTypeId ?? DBNull.Value);
        }

        return table;
    }

    private static DataTable BuildDealDeckCardsTable(IReadOnlyList<DealDeckCard> entities)
    {
        var table = new DataTable();
        table.Columns.Add("DealId", typeof(int));
        table.Columns.Add("CardId", typeof(int));
        table.Columns.Add("SortOrder", typeof(int));

        foreach (var e in entities)
        {
            table.Rows.Add(e.DealId, e.CardId, e.SortOrder);
        }

        return table;
    }

    private static DataTable BuildDealKnownPlayerSuitVoidsTable(IReadOnlyList<DealKnownPlayerSuitVoid> entities)
    {
        var table = new DataTable();
        table.Columns.Add("DealId", typeof(int));
        table.Columns.Add("PlayerPositionId", typeof(int));
        table.Columns.Add("SuitId", typeof(int));

        foreach (var e in entities)
        {
            table.Rows.Add(e.DealId, e.PlayerPositionId, e.SuitId);
        }

        return table;
    }

    private static DataTable BuildDealPlayerStartingHandCardsTable(IReadOnlyList<DealPlayerStartingHandCard> entities)
    {
        var table = new DataTable();
        table.Columns.Add("DealPlayerId", typeof(int));
        table.Columns.Add("CardId", typeof(int));
        table.Columns.Add("SortOrder", typeof(int));

        foreach (var e in entities)
        {
            table.Rows.Add(e.DealPlayerId, e.CardId, e.SortOrder);
        }

        return table;
    }

    private static DataTable BuildTrickCardsPlayedTable(IReadOnlyList<TrickCardPlayed> entities)
    {
        var table = new DataTable();
        table.Columns.Add("TrickId", typeof(int));
        table.Columns.Add("PlayerPositionId", typeof(int));
        table.Columns.Add("CardId", typeof(int));
        table.Columns.Add("PlayOrder", typeof(int));

        foreach (var e in entities)
        {
            table.Rows.Add(e.TrickId, e.PlayerPositionId, e.CardId, e.PlayOrder);
        }

        return table;
    }

    private static DataTable BuildCallTrumpCardsInHandTable(IReadOnlyList<CallTrumpDecisionCardsInHand> entities)
    {
        var table = new DataTable();
        table.Columns.Add("CallTrumpDecisionId", typeof(int));
        table.Columns.Add("CardId", typeof(int));
        table.Columns.Add("SortOrder", typeof(int));

        foreach (var e in entities)
        {
            table.Rows.Add(e.CallTrumpDecisionId, e.CardId, e.SortOrder);
        }

        return table;
    }

    private static DataTable BuildCallTrumpValidDecisionsTable(IReadOnlyList<CallTrumpDecisionValidDecision> entities)
    {
        var table = new DataTable();
        table.Columns.Add("CallTrumpDecisionId", typeof(int));
        table.Columns.Add("CallTrumpDecisionValueId", typeof(int));

        foreach (var e in entities)
        {
            table.Rows.Add(e.CallTrumpDecisionId, e.CallTrumpDecisionValueId);
        }

        return table;
    }

    private static DataTable BuildCallTrumpPredictedPointsTable(IReadOnlyList<CallTrumpDecisionPredictedPoints> entities)
    {
        var table = new DataTable();
        table.Columns.Add("CallTrumpDecisionId", typeof(int));
        table.Columns.Add("CallTrumpDecisionValueId", typeof(int));
        table.Columns.Add("PredictedPoints", typeof(float));

        foreach (var e in entities)
        {
            table.Rows.Add(e.CallTrumpDecisionId, e.CallTrumpDecisionValueId, e.PredictedPoints);
        }

        return table;
    }

    private static DataTable BuildDiscardCardsInHandTable(IReadOnlyList<DiscardCardDecisionCardsInHand> entities)
    {
        var table = new DataTable();
        table.Columns.Add("DiscardCardDecisionId", typeof(int));
        table.Columns.Add("RelativeCardId", typeof(int));
        table.Columns.Add("SortOrder", typeof(int));

        foreach (var e in entities)
        {
            table.Rows.Add(e.DiscardCardDecisionId, e.RelativeCardId, e.SortOrder);
        }

        return table;
    }

    private static DataTable BuildDiscardPredictedPointsTable(IReadOnlyList<DiscardCardDecisionPredictedPoints> entities)
    {
        var table = new DataTable();
        table.Columns.Add("DiscardCardDecisionId", typeof(int));
        table.Columns.Add("RelativeCardId", typeof(int));
        table.Columns.Add("PredictedPoints", typeof(float));

        foreach (var e in entities)
        {
            table.Rows.Add(e.DiscardCardDecisionId, e.RelativeCardId, e.PredictedPoints);
        }

        return table;
    }

    private static DataTable BuildPlayCardCardsInHandTable(IReadOnlyList<PlayCardDecisionCardsInHand> entities)
    {
        var table = new DataTable();
        table.Columns.Add("PlayCardDecisionId", typeof(int));
        table.Columns.Add("RelativeCardId", typeof(int));
        table.Columns.Add("SortOrder", typeof(int));

        foreach (var e in entities)
        {
            table.Rows.Add(e.PlayCardDecisionId, e.RelativeCardId, e.SortOrder);
        }

        return table;
    }

    private static DataTable BuildPlayCardPlayedCardsTable(IReadOnlyList<PlayCardDecisionPlayedCard> entities)
    {
        var table = new DataTable();
        table.Columns.Add("PlayCardDecisionId", typeof(int));
        table.Columns.Add("RelativePlayerPositionId", typeof(int));
        table.Columns.Add("RelativeCardId", typeof(int));

        foreach (var e in entities)
        {
            table.Rows.Add(e.PlayCardDecisionId, e.RelativePlayerPositionId, e.RelativeCardId);
        }

        return table;
    }

    private static DataTable BuildPlayCardValidCardsTable(IReadOnlyList<PlayCardDecisionValidCard> entities)
    {
        var table = new DataTable();
        table.Columns.Add("PlayCardDecisionId", typeof(int));
        table.Columns.Add("RelativeCardId", typeof(int));

        foreach (var e in entities)
        {
            table.Rows.Add(e.PlayCardDecisionId, e.RelativeCardId);
        }

        return table;
    }

    private static DataTable BuildPlayCardKnownVoidsTable(IReadOnlyList<PlayCardDecisionKnownVoid> entities)
    {
        var table = new DataTable();
        table.Columns.Add("PlayCardDecisionId", typeof(int));
        table.Columns.Add("RelativePlayerPositionId", typeof(int));
        table.Columns.Add("RelativeSuitId", typeof(int));

        foreach (var e in entities)
        {
            table.Rows.Add(e.PlayCardDecisionId, e.RelativePlayerPositionId, e.RelativeSuitId);
        }

        return table;
    }

    private static DataTable BuildPlayCardAccountedForCardsTable(IReadOnlyList<PlayCardDecisionAccountedForCard> entities)
    {
        var table = new DataTable();
        table.Columns.Add("PlayCardDecisionId", typeof(int));
        table.Columns.Add("RelativeCardId", typeof(int));

        foreach (var e in entities)
        {
            table.Rows.Add(e.PlayCardDecisionId, e.RelativeCardId);
        }

        return table;
    }

    private static DataTable BuildPlayCardPredictedPointsTable(IReadOnlyList<PlayCardDecisionPredictedPoints> entities)
    {
        var table = new DataTable();
        table.Columns.Add("PlayCardDecisionId", typeof(int));
        table.Columns.Add("RelativeCardId", typeof(int));
        table.Columns.Add("PredictedPoints", typeof(float));

        foreach (var e in entities)
        {
            table.Rows.Add(e.PlayCardDecisionId, e.RelativeCardId, e.PredictedPoints);
        }

        return table;
    }
}
