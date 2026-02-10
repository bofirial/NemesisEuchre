using System.Data.Common;

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
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "GamePlayers", cache.GamePlayers, CreateGamePlayersReader, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "DealDeckCards", cache.DealDeckCards, CreateDealDeckCardsReader, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "DealKnownPlayerSuitVoids", cache.DealKnownPlayerSuitVoids, CreateDealKnownPlayerSuitVoidsReader, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "DealPlayerStartingHandCards", cache.DealPlayerStartingHandCards, CreateDealPlayerStartingHandCardsReader, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "TrickCardsPlayed", cache.TrickCardsPlayed, CreateTrickCardsPlayedReader, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "CallTrumpDecisionCardsInHand", cache.CallTrumpCardsInHand, CreateCallTrumpCardsInHandReader, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "CallTrumpValidDecisions", cache.CallTrumpValidDecisions, CreateCallTrumpValidDecisionsReader, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "CallTrumpDecisionPredictedPoints", cache.CallTrumpPredictedPoints, CreateCallTrumpPredictedPointsReader, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "DiscardCardDecisionCardsInHand", cache.DiscardCardsInHand, CreateDiscardCardsInHandReader, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "DiscardCardDecisionPredictedPoints", cache.DiscardPredictedPoints, CreateDiscardPredictedPointsReader, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionCardsInHand", cache.PlayCardCardsInHand, CreatePlayCardCardsInHandReader, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionPlayedCards", cache.PlayCardPlayedCards, CreatePlayCardPlayedCardsReader, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionValidCards", cache.PlayCardValidCards, CreatePlayCardValidCardsReader, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionKnownVoids", cache.PlayCardKnownVoids, CreatePlayCardKnownVoidsReader, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionCardsAccountedFor", cache.PlayCardAccountedForCards, CreatePlayCardAccountedForCardsReader, cancellationToken).ConfigureAwait(false);
        await BulkInsertAsync(connection, transaction, bulkCopyTimeout, "PlayCardDecisionPredictedPoints", cache.PlayCardPredictedPoints, CreatePlayCardPredictedPointsReader, cancellationToken).ConfigureAwait(false);
    }

    private static async Task BulkInsertAsync<T>(
        SqlConnection connection,
        SqlTransaction transaction,
        int bulkCopyTimeout,
        string tableName,
        IReadOnlyList<T> entities,
        Func<IReadOnlyList<T>, DbDataReader> createReader,
        CancellationToken cancellationToken)
    {
        if (entities.Count == 0)
        {
            return;
        }

        await using var reader = createReader(entities);

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

    private static EntityListDataReader<GamePlayer> CreateGamePlayersReader(IReadOnlyList<GamePlayer> entities)
    {
        return new(
            entities,
            [
                ("GameId", typeof(int), e => e.GameId),
                ("PlayerPositionId", typeof(int), e => e.PlayerPositionId),
                ("ActorTypeId", typeof(int), e => (object?)e.ActorTypeId ?? DBNull.Value),
            ]);
    }

    private static EntityListDataReader<DealDeckCard> CreateDealDeckCardsReader(IReadOnlyList<DealDeckCard> entities)
    {
        return new(
            entities,
            [
                ("DealId", typeof(int), e => e.DealId),
                ("CardId", typeof(int), e => e.CardId),
                ("SortOrder", typeof(int), e => e.SortOrder),
            ]);
    }

    private static EntityListDataReader<DealKnownPlayerSuitVoid> CreateDealKnownPlayerSuitVoidsReader(IReadOnlyList<DealKnownPlayerSuitVoid> entities)
    {
        return new(
            entities,
            [
                ("DealId", typeof(int), e => e.DealId),
                ("PlayerPositionId", typeof(int), e => e.PlayerPositionId),
                ("SuitId", typeof(int), e => e.SuitId),
            ]);
    }

    private static EntityListDataReader<DealPlayerStartingHandCard> CreateDealPlayerStartingHandCardsReader(IReadOnlyList<DealPlayerStartingHandCard> entities)
    {
        return new(
            entities,
            [
                ("DealPlayerId", typeof(int), e => e.DealPlayerId),
                ("CardId", typeof(int), e => e.CardId),
                ("SortOrder", typeof(int), e => e.SortOrder),
            ]);
    }

    private static EntityListDataReader<TrickCardPlayed> CreateTrickCardsPlayedReader(IReadOnlyList<TrickCardPlayed> entities)
    {
        return new(
            entities,
            [
                ("TrickId", typeof(int), e => e.TrickId),
                ("PlayerPositionId", typeof(int), e => e.PlayerPositionId),
                ("CardId", typeof(int), e => e.CardId),
                ("PlayOrder", typeof(int), e => e.PlayOrder),
            ]);
    }

    private static EntityListDataReader<CallTrumpDecisionCardsInHand> CreateCallTrumpCardsInHandReader(IReadOnlyList<CallTrumpDecisionCardsInHand> entities)
    {
        return new(
            entities,
            [
                ("CallTrumpDecisionId", typeof(int), e => e.CallTrumpDecisionId),
                ("CardId", typeof(int), e => e.CardId),
                ("SortOrder", typeof(int), e => e.SortOrder),
            ]);
    }

    private static EntityListDataReader<CallTrumpDecisionValidDecision> CreateCallTrumpValidDecisionsReader(IReadOnlyList<CallTrumpDecisionValidDecision> entities)
    {
        return new(
            entities,
            [
                ("CallTrumpDecisionId", typeof(int), e => e.CallTrumpDecisionId),
                ("CallTrumpDecisionValueId", typeof(int), e => e.CallTrumpDecisionValueId),
            ]);
    }

    private static EntityListDataReader<CallTrumpDecisionPredictedPoints> CreateCallTrumpPredictedPointsReader(IReadOnlyList<CallTrumpDecisionPredictedPoints> entities)
    {
        return new(
            entities,
            [
                ("CallTrumpDecisionId", typeof(int), e => e.CallTrumpDecisionId),
                ("CallTrumpDecisionValueId", typeof(int), e => e.CallTrumpDecisionValueId),
                ("PredictedPoints", typeof(float), e => e.PredictedPoints),
            ]);
    }

    private static EntityListDataReader<DiscardCardDecisionCardsInHand> CreateDiscardCardsInHandReader(IReadOnlyList<DiscardCardDecisionCardsInHand> entities)
    {
        return new(
            entities,
            [
                ("DiscardCardDecisionId", typeof(int), e => e.DiscardCardDecisionId),
                ("RelativeCardId", typeof(int), e => e.RelativeCardId),
                ("SortOrder", typeof(int), e => e.SortOrder),
            ]);
    }

    private static EntityListDataReader<DiscardCardDecisionPredictedPoints> CreateDiscardPredictedPointsReader(IReadOnlyList<DiscardCardDecisionPredictedPoints> entities)
    {
        return new(
            entities,
            [
                ("DiscardCardDecisionId", typeof(int), e => e.DiscardCardDecisionId),
                ("RelativeCardId", typeof(int), e => e.RelativeCardId),
                ("PredictedPoints", typeof(float), e => e.PredictedPoints),
            ]);
    }

    private static EntityListDataReader<PlayCardDecisionCardsInHand> CreatePlayCardCardsInHandReader(IReadOnlyList<PlayCardDecisionCardsInHand> entities)
    {
        return new(
            entities,
            [
                ("PlayCardDecisionId", typeof(int), e => e.PlayCardDecisionId),
                ("RelativeCardId", typeof(int), e => e.RelativeCardId),
                ("SortOrder", typeof(int), e => e.SortOrder),
            ]);
    }

    private static EntityListDataReader<PlayCardDecisionPlayedCard> CreatePlayCardPlayedCardsReader(IReadOnlyList<PlayCardDecisionPlayedCard> entities)
    {
        return new(
            entities,
            [
                ("PlayCardDecisionId", typeof(int), e => e.PlayCardDecisionId),
                ("RelativePlayerPositionId", typeof(int), e => e.RelativePlayerPositionId),
                ("RelativeCardId", typeof(int), e => e.RelativeCardId),
            ]);
    }

    private static EntityListDataReader<PlayCardDecisionValidCard> CreatePlayCardValidCardsReader(IReadOnlyList<PlayCardDecisionValidCard> entities)
    {
        return new(
            entities,
            [
                ("PlayCardDecisionId", typeof(int), e => e.PlayCardDecisionId),
                ("RelativeCardId", typeof(int), e => e.RelativeCardId),
            ]);
    }

    private static EntityListDataReader<PlayCardDecisionKnownVoid> CreatePlayCardKnownVoidsReader(IReadOnlyList<PlayCardDecisionKnownVoid> entities)
    {
        return new(
            entities,
            [
                ("PlayCardDecisionId", typeof(int), e => e.PlayCardDecisionId),
                ("RelativePlayerPositionId", typeof(int), e => e.RelativePlayerPositionId),
                ("RelativeSuitId", typeof(int), e => e.RelativeSuitId),
            ]);
    }

    private static EntityListDataReader<PlayCardDecisionAccountedForCard> CreatePlayCardAccountedForCardsReader(IReadOnlyList<PlayCardDecisionAccountedForCard> entities)
    {
        return new(
            entities,
            [
                ("PlayCardDecisionId", typeof(int), e => e.PlayCardDecisionId),
                ("RelativeCardId", typeof(int), e => e.RelativeCardId),
            ]);
    }

    private static EntityListDataReader<PlayCardDecisionPredictedPoints> CreatePlayCardPredictedPointsReader(IReadOnlyList<PlayCardDecisionPredictedPoints> entities)
    {
        return new(
            entities,
            [
                ("PlayCardDecisionId", typeof(int), e => e.PlayCardDecisionId),
                ("RelativeCardId", typeof(int), e => e.RelativeCardId),
                ("PredictedPoints", typeof(float), e => e.PredictedPoints),
            ]);
    }
}
