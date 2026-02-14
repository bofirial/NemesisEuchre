using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.DataAccess.Services;

public static class EntityReaderConfiguration
{
    public static void ConfigureReaders(EntityReaderFactory factory)
    {
        // GamePlayers
        factory.Register<GamePlayer>(entities =>
            new EntityListDataReader<GamePlayer>(
                entities,
                [
                    ("GameId", typeof(int), e => e.GameId),
                    ("PlayerPositionId", typeof(int), e => e.PlayerPositionId),
                    ("ActorTypeId", typeof(int), e => (object?)e.ActorTypeId ?? DBNull.Value),
                ]));

        // DealDeckCards
        factory.Register<DealDeckCard>(entities =>
            new EntityListDataReader<DealDeckCard>(
                entities,
                [
                    ("DealId", typeof(int), e => e.DealId),
                    ("CardId", typeof(int), e => e.CardId),
                    ("SortOrder", typeof(int), e => e.SortOrder),
                ]));

        // DealKnownPlayerSuitVoids
        factory.Register<DealKnownPlayerSuitVoid>(entities =>
            new EntityListDataReader<DealKnownPlayerSuitVoid>(
                entities,
                [
                    ("DealId", typeof(int), e => e.DealId),
                    ("PlayerPositionId", typeof(int), e => e.PlayerPositionId),
                    ("SuitId", typeof(int), e => e.SuitId),
                ]));

        // DealPlayerStartingHandCards
        factory.Register<DealPlayerStartingHandCard>(entities =>
            new EntityListDataReader<DealPlayerStartingHandCard>(
                entities,
                [
                    ("DealPlayerId", typeof(int), e => e.DealPlayerId),
                    ("CardId", typeof(int), e => e.CardId),
                    ("SortOrder", typeof(int), e => e.SortOrder),
                ]));

        // TrickCardsPlayed
        factory.Register<TrickCardPlayed>(entities =>
            new EntityListDataReader<TrickCardPlayed>(
                entities,
                [
                    ("TrickId", typeof(int), e => e.TrickId),
                    ("PlayerPositionId", typeof(int), e => e.PlayerPositionId),
                    ("CardId", typeof(int), e => e.CardId),
                    ("PlayOrder", typeof(int), e => e.PlayOrder),
                ]));

        // CallTrumpDecisionCardsInHand
        factory.Register<CallTrumpDecisionCardsInHand>(entities =>
            new EntityListDataReader<CallTrumpDecisionCardsInHand>(
                entities,
                [
                    ("CallTrumpDecisionId", typeof(int), e => e.CallTrumpDecisionId),
                    ("CardId", typeof(int), e => e.CardId),
                    ("SortOrder", typeof(int), e => e.SortOrder),
                ]));

        // CallTrumpDecisionValidDecisions
        factory.Register<CallTrumpDecisionValidDecision>(entities =>
            new EntityListDataReader<CallTrumpDecisionValidDecision>(
                entities,
                [
                    ("CallTrumpDecisionId", typeof(int), e => e.CallTrumpDecisionId),
                    ("CallTrumpDecisionValueId", typeof(int), e => e.CallTrumpDecisionValueId),
                ]));

        // CallTrumpDecisionPredictedPoints
        factory.Register<CallTrumpDecisionPredictedPoints>(entities =>
            new EntityListDataReader<CallTrumpDecisionPredictedPoints>(
                entities,
                [
                    ("CallTrumpDecisionId", typeof(int), e => e.CallTrumpDecisionId),
                    ("CallTrumpDecisionValueId", typeof(int), e => e.CallTrumpDecisionValueId),
                    ("PredictedPoints", typeof(float), e => e.PredictedPoints),
                ]));

        // DiscardCardDecisionCardsInHand
        factory.Register<DiscardCardDecisionCardsInHand>(entities =>
            new EntityListDataReader<DiscardCardDecisionCardsInHand>(
                entities,
                [
                    ("DiscardCardDecisionId", typeof(int), e => e.DiscardCardDecisionId),
                    ("RelativeCardId", typeof(int), e => e.RelativeCardId),
                    ("SortOrder", typeof(int), e => e.SortOrder),
                ]));

        // DiscardCardDecisionPredictedPoints
        factory.Register<DiscardCardDecisionPredictedPoints>(entities =>
            new EntityListDataReader<DiscardCardDecisionPredictedPoints>(
                entities,
                [
                    ("DiscardCardDecisionId", typeof(int), e => e.DiscardCardDecisionId),
                    ("RelativeCardId", typeof(int), e => e.RelativeCardId),
                    ("PredictedPoints", typeof(float), e => e.PredictedPoints),
                ]));

        // PlayCardDecisionCardsInHand
        factory.Register<PlayCardDecisionCardsInHand>(entities =>
            new EntityListDataReader<PlayCardDecisionCardsInHand>(
                entities,
                [
                    ("PlayCardDecisionId", typeof(int), e => e.PlayCardDecisionId),
                    ("RelativeCardId", typeof(int), e => e.RelativeCardId),
                    ("SortOrder", typeof(int), e => e.SortOrder),
                ]));

        // PlayCardDecisionPlayedCards
        factory.Register<PlayCardDecisionPlayedCard>(entities =>
            new EntityListDataReader<PlayCardDecisionPlayedCard>(
                entities,
                [
                    ("PlayCardDecisionId", typeof(int), e => e.PlayCardDecisionId),
                    ("RelativePlayerPositionId", typeof(int), e => e.RelativePlayerPositionId),
                    ("RelativeCardId", typeof(int), e => e.RelativeCardId),
                ]));

        // PlayCardDecisionValidCards
        factory.Register<PlayCardDecisionValidCard>(entities =>
            new EntityListDataReader<PlayCardDecisionValidCard>(
                entities,
                [
                    ("PlayCardDecisionId", typeof(int), e => e.PlayCardDecisionId),
                    ("RelativeCardId", typeof(int), e => e.RelativeCardId),
                ]));

        // PlayCardDecisionKnownVoids
        factory.Register<PlayCardDecisionKnownVoid>(entities =>
            new EntityListDataReader<PlayCardDecisionKnownVoid>(
                entities,
                [
                    ("PlayCardDecisionId", typeof(int), e => e.PlayCardDecisionId),
                    ("RelativePlayerPositionId", typeof(int), e => e.RelativePlayerPositionId),
                    ("RelativeSuitId", typeof(int), e => e.RelativeSuitId),
                ]));

        // PlayCardDecisionAccountedForCards
        factory.Register<PlayCardDecisionAccountedForCard>(entities =>
            new EntityListDataReader<PlayCardDecisionAccountedForCard>(
                entities,
                [
                    ("PlayCardDecisionId", typeof(int), e => e.PlayCardDecisionId),
                    ("RelativeCardId", typeof(int), e => e.RelativeCardId),
                ]));

        // PlayCardDecisionPredictedPoints
        factory.Register<PlayCardDecisionPredictedPoints>(entities =>
            new EntityListDataReader<PlayCardDecisionPredictedPoints>(
                entities,
                [
                    ("PlayCardDecisionId", typeof(int), e => e.PlayCardDecisionId),
                    ("RelativeCardId", typeof(int), e => e.RelativeCardId),
                    ("PredictedPoints", typeof(float), e => e.PredictedPoints),
                ]));
    }
}
