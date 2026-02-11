using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.DataAccess.Mappers;

public interface IEntityToDealMapper
{
    Deal Map(DealEntity entity, Dictionary<PlayerPosition, Player> gamePlayers, bool includeDecisions);
}

public class EntityToDealMapper(IEntityToTrickMapper trickMapper) : IEntityToDealMapper
{
    public Deal Map(DealEntity entity, Dictionary<PlayerPosition, Player> gamePlayers, bool includeDecisions)
    {
        var dealerPosition = entity.DealerPositionId.HasValue ? (PlayerPosition)entity.DealerPositionId.Value : (PlayerPosition?)null;
        var trump = entity.TrumpSuitId.HasValue ? (Suit)entity.TrumpSuitId.Value : (Suit?)null;
        var callingPlayer = entity.CallingPlayerPositionId.HasValue ? (PlayerPosition)entity.CallingPlayerPositionId.Value : (PlayerPosition?)null;

        var deal = new Deal
        {
            DealNumber = (short)entity.DealNumber,
            DealStatus = (DealStatus)entity.DealStatusId,
            DealerPosition = dealerPosition,
            UpCard = entity.UpCardId.HasValue ? CardIdHelper.ToCard(entity.UpCardId.Value) : null,
            DiscardedCard = entity.DiscardedCardId.HasValue ? CardIdHelper.ToCard(entity.DiscardedCardId.Value) : null,
            Trump = trump,
            CallingPlayer = callingPlayer,
            CallingPlayerIsGoingAlone = entity.CallingPlayerIsGoingAlone,
            ChosenDecision = entity.ChosenCallTrumpDecisionId.HasValue ? (CallTrumpDecision)entity.ChosenCallTrumpDecisionId.Value : null,
            DealResult = entity.DealResultId.HasValue ? (DealResult)entity.DealResultId.Value : null,
            WinningTeam = entity.WinningTeamId.HasValue ? (Team)entity.WinningTeamId.Value : null,
            Team1Score = entity.Team1Score,
            Team2Score = entity.Team2Score,
            Deck = [.. entity.DealDeckCards.OrderBy(c => c.SortOrder).Select(c => CardIdHelper.ToCard(c.CardId))],
            KnownPlayerSuitVoids = [.. entity.DealKnownPlayerSuitVoids.Select(v =>
                new PlayerSuitVoid((PlayerPosition)v.PlayerPositionId, (Suit)v.SuitId))],
            Players = entity.DealPlayers.ToDictionary(
                dp => (PlayerPosition)dp.PlayerPositionId,
                dp => new DealPlayer
                {
                    Position = (PlayerPosition)dp.PlayerPositionId,
                    Actor = new Actor((ActorType)dp.ActorTypeId, null),
                    StartingHand = [.. dp.StartingHandCards.OrderBy(c => c.SortOrder).Select(c => CardIdHelper.ToCard(c.CardId))],
                }),
            CompletedTricks = [.. entity.Tricks
                .OrderBy(t => t.TrickNumber)
                .Select(t => trickMapper.Map(t, trump ?? default, dealerPosition ?? default, includeDecisions))],
        };

        if (includeDecisions)
        {
            MapCallTrumpDecisions(entity, deal, dealerPosition);
            MapDiscardCardDecisions(entity, deal, trump, callingPlayer);
        }

        return deal;
    }

    private static void MapCallTrumpDecisions(DealEntity entity, Deal deal, PlayerPosition? dealerPosition)
    {
        deal.CallTrumpDecisions = [.. entity.CallTrumpDecisions
            .OrderBy(d => d.DecisionOrder)
            .Select(d =>
            {
                var selfPosition = PlayerPositionExtensions.DeriveAbsolutePosition(
                    dealerPosition ?? default,
                    (RelativePlayerPosition)d.DealerRelativePositionId);

                return new CallTrumpDecisionRecord
                {
                    PlayerPosition = selfPosition,
                    DealerPosition = dealerPosition ?? default,
                    UpCard = CardIdHelper.ToCard(d.UpCardId),
                    TeamScore = d.TeamScore,
                    OpponentScore = d.OpponentScore,
                    ChosenDecision = (CallTrumpDecision)d.ChosenDecisionValueId,
                    DecisionOrder = d.DecisionOrder,
                    CardsInHand = [.. d.CardsInHand.OrderBy(c => c.SortOrder).Select(c => CardIdHelper.ToCard(c.CardId))],
                    ValidCallTrumpDecisions = [.. d.ValidDecisions.Select(v => (CallTrumpDecision)v.CallTrumpDecisionValueId)],
                    DecisionPredictedPoints = d.PredictedPoints.ToDictionary(
                        p => (CallTrumpDecision)p.CallTrumpDecisionValueId,
                        p => p.PredictedPoints),
                };
            })];
    }

    private static void MapDiscardCardDecisions(DealEntity entity, Deal deal, Suit? trump, PlayerPosition? callingPlayer)
    {
        deal.DiscardCardDecisions = [.. entity.DiscardCardDecisions.Select(d =>
        {
            var selfPosition = PlayerPositionExtensions.DeriveAbsolutePosition(
                callingPlayer ?? default,
                (RelativePlayerPosition)d.CallingRelativePlayerPositionId);

            var trumpValue = trump ?? default;

            return new DiscardCardDecisionRecord
            {
                PlayerPosition = selfPosition,
                TrumpSuit = trumpValue,
                CallingPlayer = callingPlayer ?? default,
                CallingPlayerGoingAlone = d.CallingPlayerGoingAlone,
                TeamScore = d.TeamScore,
                OpponentScore = d.OpponentScore,
                ChosenCard = CardIdHelper.ToRelativeCard(d.ChosenRelativeCardId).ToAbsolute(trumpValue),
                CardsInHand = [.. d.CardsInHand.OrderBy(c => c.SortOrder).Select(c => CardIdHelper.ToRelativeCard(c.RelativeCardId).ToAbsolute(trumpValue))],
                DecisionPredictedPoints = d.PredictedPoints.ToDictionary(
                    p => CardIdHelper.ToRelativeCard(p.RelativeCardId).ToAbsolute(trumpValue),
                    p => p.PredictedPoints),
            };
        })];
    }
}
