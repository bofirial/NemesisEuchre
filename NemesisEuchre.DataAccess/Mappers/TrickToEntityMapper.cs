using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Extensions;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Mappers;

public interface ITrickToEntityMapper
{
    TrickEntity Map(Trick trick, int trickNumber, Dictionary<PlayerPosition, Player> gamePlayers, bool didTeam1WinGame, bool didTeam2WinGame, Team? dealWinningTeam, DealResult? dealResult);
}

public class TrickToEntityMapper : ITrickToEntityMapper
{
    public TrickEntity Map(Trick trick, int trickNumber, Dictionary<PlayerPosition, Player> gamePlayers, bool didTeam1WinGame, bool didTeam2WinGame, Team? dealWinningTeam, DealResult? dealResult)
    {
        var didTeam1WinDeal = dealWinningTeam == Team.Team1;
        var didTeam2WinDeal = dealWinningTeam == Team.Team2;

        return new TrickEntity
        {
            TrickNumber = trickNumber,
            LeadPlayerPositionId = (int)trick.LeadPosition,
            LeadSuitId = trick.LeadSuit.HasValue ? (int)trick.LeadSuit.Value : null,
            WinningPlayerPositionId = trick.WinningPosition.HasValue ? (int)trick.WinningPosition.Value : null,
            WinningTeamId = trick.WinningTeam.HasValue ? (int)trick.WinningTeam.Value : null,
            TrickCardsPlayed = [.. trick.CardsPlayed.Select((played, index) => new TrickCardPlayed
            {
                PlayerPositionId = (int)played.PlayerPosition,
                CardId = CardIdHelper.ToCardId(played.Card),
                PlayOrder = index,
            })],
            PlayCardDecisions = [.. trick.PlayCardDecisions.Select(decision =>
            {
                var actorType = gamePlayers[decision.PlayerPosition].ActorType;
                var playerTeam = decision.PlayerPosition.GetTeam();
                var didTeamWinTrick = trick.WinningTeam == playerTeam;
                var didTeamWinDeal = playerTeam == Team.Team1 ? didTeam1WinDeal : didTeam2WinDeal;
                var didTeamWinGame = playerTeam == Team.Team1 ? didTeam1WinGame : didTeam2WinGame;

                return new PlayCardDecisionEntity
                {
                    LeadRelativePlayerPositionId = (int)decision.LeadPlayer.ToRelativePosition(decision.PlayerPosition),
                    LeadRelativeSuitId = decision.LeadSuit.HasValue ? (int)decision.LeadSuit.Value.ToRelativeSuit(decision.TrumpSuit) : null,
                    WinningTrickRelativePlayerPositionId = decision.WinningTrickPlayer.HasValue ? (int)decision.WinningTrickPlayer.Value.ToRelativePosition(decision.PlayerPosition) : null,
                    TrickNumber = decision.TrickNumber,
                    TeamScore = decision.TeamScore,
                    OpponentScore = decision.OpponentScore,
                    CallingRelativePlayerPositionId = (int)decision.CallingPlayer.ToRelativePosition(decision.PlayerPosition),
                    CallingPlayerGoingAlone = decision.CallingPlayerGoingAlone,
                    DealerRelativePlayerPositionId = (int)decision.Dealer.ToRelativePosition(decision.PlayerPosition),
                    DealerPickedUpRelativeCardId = decision.DealerPickedUpCard != null
                        ? CardIdHelper.ToRelativeCardId(decision.DealerPickedUpCard.ToRelative(decision.TrumpSuit))
                        : null,
                    ChosenRelativeCardId = CardIdHelper.ToRelativeCardId(decision.ChosenCard.ToRelative(decision.TrumpSuit)),
                    ActorTypeId = actorType.HasValue ? (int)actorType.Value : null,
                    DidTeamWinTrick = didTeamWinTrick,
                    DidTeamWinDeal = didTeamWinDeal,
                    RelativeDealPoints = dealResult.CalculateRelativeDealPoints(decision.PlayerPosition, dealWinningTeam),
                    DidTeamWinGame = didTeamWinGame,
                    CardsInHand = [.. decision.CardsInHand.SortByTrump(decision.TrumpSuit).Select((card, index) => new PlayCardDecisionCardsInHand
                    {
                        RelativeCardId = CardIdHelper.ToRelativeCardId(card.ToRelative(decision.TrumpSuit)),
                        SortOrder = index,
                    })],
                    PlayedCards = [.. decision.PlayedCards.Select(kvp => new PlayCardDecisionPlayedCard
                    {
                        RelativePlayerPositionId = (int)kvp.Key.ToRelativePosition(decision.PlayerPosition),
                        RelativeCardId = CardIdHelper.ToRelativeCardId(kvp.Value.ToRelative(decision.TrumpSuit)),
                    })],
                    ValidCards = [.. decision.ValidCardsToPlay.SortByTrump(decision.TrumpSuit).Select(card => new PlayCardDecisionValidCard
                    {
                        RelativeCardId = CardIdHelper.ToRelativeCardId(card.ToRelative(decision.TrumpSuit)),
                    })],
                    KnownVoids = [.. decision.KnownPlayerSuitVoids
                        .Where(v => v.PlayerPosition != decision.PlayerPosition)
                        .Select(v => new PlayCardDecisionKnownVoid
                        {
                            RelativePlayerPositionId = (int)v.PlayerPosition.ToRelativePosition(decision.PlayerPosition),
                            RelativeSuitId = (int)v.Suit.ToRelativeSuit(decision.TrumpSuit),
                        })],
                    CardsAccountedFor = [.. decision.CardsAccountedFor.SortByTrump(decision.TrumpSuit).Select(card => new PlayCardDecisionAccountedForCard
                    {
                        RelativeCardId = CardIdHelper.ToRelativeCardId(card.ToRelative(decision.TrumpSuit)),
                    })],
                    PredictedPoints = [.. decision.DecisionPredictedPoints.Select(kvp => new PlayCardDecisionPredictedPoints
                    {
                        RelativeCardId = CardIdHelper.ToRelativeCardId(kvp.Key.ToRelative(decision.TrumpSuit)),
                        PredictedPoints = kvp.Value,
                    })],
                };
            })],
        };
    }
}
