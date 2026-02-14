using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Extensions;
using NemesisEuchre.DataAccess.Models;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Mappers;

public interface IDealToEntityMapper
{
    DealEntity Map(Deal deal, int dealNumber, Dictionary<PlayerPosition, Player> gamePlayers, GameOutcomeContext gameOutcome);
}

public class DealToEntityMapper(ITrickToEntityMapper trickMapper) : IDealToEntityMapper
{
    public DealEntity Map(Deal deal, int dealNumber, Dictionary<PlayerPosition, Player> gamePlayers, GameOutcomeContext gameOutcome)
    {
        if (deal.DealNumber == 0)
        {
            throw new InvalidOperationException("DealNumber must be set before mapping");
        }

        foreach (var trick in deal.CompletedTricks)
        {
            if (trick.TrickNumber == 0)
            {
                throw new InvalidOperationException("TrickNumber must be set for all tricks before mapping");
            }
        }

        var dealEntity = new DealEntity
        {
            DealNumber = dealNumber,
            DealStatusId = (int)deal.DealStatus,
            DealerPositionId = deal.DealerPosition.HasValue ? (int)deal.DealerPosition.Value : null,
            UpCardId = deal.UpCard != null ? CardIdHelper.ToCardId(deal.UpCard) : null,
            DiscardedCardId = deal.DiscardedCard != null ? CardIdHelper.ToCardId(deal.DiscardedCard) : null,
            TrumpSuitId = deal.Trump.HasValue ? (int)deal.Trump.Value : null,
            CallingPlayerPositionId = deal.CallingPlayer.HasValue ? (int)deal.CallingPlayer.Value : null,
            CallingPlayerIsGoingAlone = deal.CallingPlayerIsGoingAlone,
            ChosenCallTrumpDecisionId = deal.ChosenDecision.HasValue ? (int)deal.ChosenDecision.Value : null,
            DealResultId = deal.DealResult.HasValue ? (int)deal.DealResult.Value : null,
            WinningTeamId = deal.WinningTeam.HasValue ? (int)deal.WinningTeam.Value : null,
            Team1Score = deal.Team1Score,
            Team2Score = deal.Team2Score,
            DealDeckCards = [.. deal.Deck.SortByTrump(deal.Trump).Select((card, index) => new DealDeckCard
            {
                CardId = CardIdHelper.ToCardId(card),
                SortOrder = index,
            })],
            DealPlayers = [.. deal.Players.Select(kvp => new DealPlayerEntity
            {
                PlayerPositionId = (int)kvp.Key,
                ActorTypeId = (int)kvp.Value.Actor.ActorType,
                StartingHandCards = [.. kvp.Value.StartingHand.Select((card, index) => new DealPlayerStartingHandCard
                {
                    CardId = CardIdHelper.ToCardId(card),
                    SortOrder = index,
                })],
            })],
            DealKnownPlayerSuitVoids = [.. deal.KnownPlayerSuitVoids.Select(v => new DealKnownPlayerSuitVoid
            {
                PlayerPositionId = (int)v.PlayerPosition,
                SuitId = (int)v.Suit,
            })],
            Tricks = [.. deal.CompletedTricks.Select((trick, index) => trickMapper.Map(trick, index + 1, gamePlayers, gameOutcome, deal.WinningTeam, deal.DealResult))],
        };

        MapCallTrumpDecisions(deal, dealEntity, gamePlayers, gameOutcome);
        MapDiscardCardDecisions(deal, dealEntity, gamePlayers, gameOutcome);

        dealEntity.PlayCardDecisions = [.. dealEntity.Tricks.SelectMany(t => t.PlayCardDecisions)];

        return dealEntity;
    }

    private static void MapCallTrumpDecisions(Deal deal, DealEntity dealEntity, Dictionary<PlayerPosition, Player> gamePlayers, GameOutcomeContext gameOutcome)
    {
        var didTeam1WinDeal = deal.WinningTeam == Team.Team1;
        var didTeam2WinDeal = deal.WinningTeam == Team.Team2;

        dealEntity.CallTrumpDecisions = [.. deal.CallTrumpDecisions.Select(decision =>
        {
            var actorType = gamePlayers[decision.PlayerPosition].Actor.ActorType;
            var playerTeam = decision.PlayerPosition.GetTeam();
            var didTeamWinDeal = playerTeam == Team.Team1 ? didTeam1WinDeal : didTeam2WinDeal;
            var didTeamWinGame = playerTeam == Team.Team1 ? gameOutcome.DidTeam1WinGame : gameOutcome.DidTeam2WinGame;

            return new CallTrumpDecisionEntity
            {
                DealerRelativePositionId = (int)decision.DealerPosition.ToRelativePosition(decision.PlayerPosition),
                UpCardId = CardIdHelper.ToCardId(decision.UpCard!),
                TeamScore = decision.TeamScore,
                OpponentScore = decision.OpponentScore,
                ChosenDecisionValueId = (int)decision.ChosenDecision,
                DecisionOrder = (byte)decision.DecisionOrder,
                ActorTypeId = (int)actorType,
                DidTeamWinDeal = didTeamWinDeal,
                RelativeDealPoints = deal.DealResult.CalculateRelativeDealPoints(decision.PlayerPosition, deal.WinningTeam),
                DidTeamWinGame = didTeamWinGame,
                CardsInHand = [.. decision.CardsInHand.SortByTrump(null).Select((card, index) => new CallTrumpDecisionCardsInHand
                {
                    CardId = CardIdHelper.ToCardId(card),
                    SortOrder = index,
                })],
                ValidDecisions = [.. decision.ValidCallTrumpDecisions.Select(d => new CallTrumpDecisionValidDecision
                {
                    CallTrumpDecisionValueId = (int)d,
                })],
                PredictedPoints = [.. decision.DecisionPredictedPoints.Select(kvp => new CallTrumpDecisionPredictedPoints
                {
                    CallTrumpDecisionValueId = (int)kvp.Key,
                    PredictedPoints = kvp.Value,
                })],
            };
        })];
    }

    private static void MapDiscardCardDecisions(Deal deal, DealEntity dealEntity, Dictionary<PlayerPosition, Player> gamePlayers, GameOutcomeContext gameOutcome)
    {
        var didTeam1WinDeal = deal.WinningTeam == Team.Team1;
        var didTeam2WinDeal = deal.WinningTeam == Team.Team2;

        dealEntity.DiscardCardDecisions = [.. deal.DiscardCardDecisions.Select(decision =>
        {
            var actorType = gamePlayers[decision.PlayerPosition].Actor.ActorType;
            var playerTeam = decision.PlayerPosition.GetTeam();
            var didTeamWinDeal = playerTeam == Team.Team1 ? didTeam1WinDeal : didTeam2WinDeal;
            var didTeamWinGame = playerTeam == Team.Team1 ? gameOutcome.DidTeam1WinGame : gameOutcome.DidTeam2WinGame;

            return new DiscardCardDecisionEntity
            {
                CallingRelativePlayerPositionId = (int)deal.CallingPlayer!.Value.ToRelativePosition(decision.PlayerPosition),
                TeamScore = decision.TeamScore,
                OpponentScore = decision.OpponentScore,
                ChosenRelativeCardId = CardIdHelper.ToRelativeCardId(decision.ChosenCard.ToRelative(deal.Trump!.Value)),
                CallingPlayerGoingAlone = deal.CallingPlayerIsGoingAlone,
                ActorTypeId = (int)actorType,
                DidTeamWinDeal = didTeamWinDeal,
                RelativeDealPoints = deal.DealResult.CalculateRelativeDealPoints(decision.PlayerPosition, deal.WinningTeam),
                DidTeamWinGame = didTeamWinGame,
                CardsInHand = [.. decision.CardsInHand.SortByTrump(deal.Trump!.Value).Select((card, index) => new DiscardCardDecisionCardsInHand
                {
                    RelativeCardId = CardIdHelper.ToRelativeCardId(card.ToRelative(deal.Trump!.Value)),
                    SortOrder = index,
                })],
                PredictedPoints = [.. decision.DecisionPredictedPoints.Select(kvp => new DiscardCardDecisionPredictedPoints
                {
                    RelativeCardId = CardIdHelper.ToRelativeCardId(kvp.Key.ToRelative(deal.Trump!.Value)),
                    PredictedPoints = kvp.Value,
                })],
            };
        })];
    }
}
