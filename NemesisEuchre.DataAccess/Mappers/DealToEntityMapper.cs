using System.Text.Json;

using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Mappers;

public interface IDealToEntityMapper
{
    DealEntity Map(Deal deal, int dealNumber, Dictionary<PlayerPosition, Player> gamePlayers, bool didTeam1WinGame, bool didTeam2WinGame);
}

public class DealToEntityMapper(ITrickToEntityMapper trickMapper) : IDealToEntityMapper
{
    public DealEntity Map(Deal deal, int dealNumber, Dictionary<PlayerPosition, Player> gamePlayers, bool didTeam1WinGame, bool didTeam2WinGame)
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
            DealStatus = deal.DealStatus,
            DealerPosition = deal.DealerPosition,
            DeckJson = JsonSerializer.Serialize(deal.Deck, JsonSerializationOptions.Default),
            UpCardJson = deal.UpCard != null ? JsonSerializer.Serialize(deal.UpCard, JsonSerializationOptions.Default) : null,
            Trump = deal.Trump,
            CallingPlayer = deal.CallingPlayer,
            CallingPlayerIsGoingAlone = deal.CallingPlayerIsGoingAlone,
            DealResult = deal.DealResult,
            WinningTeam = deal.WinningTeam,
            Team1Score = deal.Team1Score,
            Team2Score = deal.Team2Score,
            PlayersJson = JsonSerializer.Serialize(deal.Players, JsonSerializationOptions.Default),
            Tricks = [.. deal.CompletedTricks.Select((trick, index) => trickMapper.Map(trick, index + 1, gamePlayers, didTeam1WinGame, didTeam2WinGame, deal.WinningTeam))],
        };

        MapCallTrumpDecisions(deal, dealEntity, gamePlayers, didTeam1WinGame, didTeam2WinGame);
        MapDiscardCardDecisions(deal, dealEntity, gamePlayers, didTeam1WinGame, didTeam2WinGame);

        dealEntity.PlayCardDecisions = [.. dealEntity.Tricks.SelectMany(t => t.PlayCardDecisions)];

        return dealEntity;
    }

    private static void MapCallTrumpDecisions(Deal deal, DealEntity dealEntity, Dictionary<PlayerPosition, Player> gamePlayers, bool didTeam1WinGame, bool didTeam2WinGame)
    {
        var didTeam1WinDeal = deal.WinningTeam == Team.Team1;
        var didTeam2WinDeal = deal.WinningTeam == Team.Team2;

        dealEntity.CallTrumpDecisions = [.. deal.CallTrumpDecisions.Select(decision =>
        {
            var actorType = gamePlayers[decision.PlayerPosition].ActorType;
            var playerTeam = decision.PlayerPosition.GetTeam();
            var didTeamWinDeal = playerTeam == Team.Team1 ? didTeam1WinDeal : didTeam2WinDeal;
            var didTeamWinGame = playerTeam == Team.Team1 ? didTeam1WinGame : didTeam2WinGame;

            return new CallTrumpDecisionEntity
            {
                CardsInHandJson = JsonSerializer.Serialize(decision.CardsInHand, JsonSerializationOptions.Default),
                UpCardJson = JsonSerializer.Serialize(decision.UpCard, JsonSerializationOptions.Default),
                DealerPosition = decision.DealerPosition,
                DecidingPlayerPosition = decision.PlayerPosition,
                TeamScore = decision.TeamScore,
                OpponentScore = decision.OpponentScore,
                ValidDecisionsJson = JsonSerializer.Serialize(decision.ValidCallTrumpDecisions, JsonSerializationOptions.Default),
                ChosenDecisionJson = JsonSerializer.Serialize(decision.ChosenDecision, JsonSerializationOptions.Default),
                DecisionOrder = (byte)decision.DecisionOrder,
                ActorType = actorType,
                DidTeamWinDeal = didTeamWinDeal,
                DidTeamWinGame = didTeamWinGame,
            };
        })];
    }

    private static void MapDiscardCardDecisions(Deal deal, DealEntity dealEntity, Dictionary<PlayerPosition, Player> gamePlayers, bool didTeam1WinGame, bool didTeam2WinGame)
    {
        var didTeam1WinDeal = deal.WinningTeam == Team.Team1;
        var didTeam2WinDeal = deal.WinningTeam == Team.Team2;

        dealEntity.DiscardCardDecisions = [.. deal.DiscardCardDecisions.Select(decision =>
        {
            var actorType = gamePlayers[decision.PlayerPosition].ActorType;
            var playerTeam = decision.PlayerPosition.GetTeam();
            var didTeamWinDeal = playerTeam == Team.Team1 ? didTeam1WinDeal : didTeam2WinDeal;
            var didTeamWinGame = playerTeam == Team.Team1 ? didTeam1WinGame : didTeam2WinGame;

            return new DiscardCardDecisionEntity
            {
                CardsInHandJson = JsonSerializer.Serialize(decision.CardsInHand.Select(c => c.ToRelative(deal.Trump!.Value)), JsonSerializationOptions.Default),
                CallingPlayer = deal.CallingPlayer!.Value.ToRelativePosition(decision.PlayerPosition),
                TeamScore = decision.TeamScore,
                OpponentScore = decision.OpponentScore,
                ValidCardsToDiscardJson = JsonSerializer.Serialize(decision.ValidCardsToDiscard.Select(c => c.ToRelative(deal.Trump!.Value)), JsonSerializationOptions.Default),
                ChosenCardJson = JsonSerializer.Serialize(decision.ChosenCard.ToRelative(deal.Trump!.Value), JsonSerializationOptions.Default),
                ActorType = actorType,
                DidTeamWinDeal = didTeamWinDeal,
                DidTeamWinGame = didTeamWinGame,
            };
        })];
    }
}
