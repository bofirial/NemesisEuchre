using System.Text.Json;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Mappers;

public interface ITrickToEntityMapper
{
    TrickEntity Map(Trick trick, int trickNumber, Dictionary<PlayerPosition, Player> gamePlayers, bool didTeam1WinGame, bool didTeam2WinGame, Team? dealWinningTeam);
}

public class TrickToEntityMapper : ITrickToEntityMapper
{
    public TrickEntity Map(Trick trick, int trickNumber, Dictionary<PlayerPosition, Player> gamePlayers, bool didTeam1WinGame, bool didTeam2WinGame, Team? dealWinningTeam)
    {
        var didTeam1WinDeal = dealWinningTeam == Team.Team1;
        var didTeam2WinDeal = dealWinningTeam == Team.Team2;

        return new TrickEntity
        {
            TrickNumber = trickNumber,
            LeadPosition = trick.LeadPosition,
            CardsPlayedJson = JsonSerializer.Serialize(trick.CardsPlayed),
            LeadSuit = trick.LeadSuit,
            WinningPosition = trick.WinningPosition,
            WinningTeam = trick.WinningTeam,
            PlayCardDecisions = [.. trick.PlayCardDecisions.Select(decision =>
            {
                var actorType = gamePlayers[decision.PlayerPosition].ActorType;
                var playerTeam = decision.PlayerPosition.GetTeam();
                var didTeamWinTrick = trick.WinningTeam == playerTeam;
                var didTeamWinDeal = playerTeam == Team.Team1 ? didTeam1WinDeal : didTeam2WinDeal;
                var didTeamWinGame = playerTeam == Team.Team1 ? didTeam1WinGame : didTeam2WinGame;

                return new PlayCardDecisionEntity
                {
                    TrickNumber = trick.TrickNumber,
                    HandJson = JsonSerializer.Serialize(decision.CardsInHand),
                    DecidingPlayerPosition = decision.PlayerPosition,
                    TrumpSuit = decision.TrumpSuit,
                    LeadPlayer = decision.LeadPlayer,
                    LeadSuit = decision.LeadSuit,
                    PlayedCardsJson = JsonSerializer.Serialize(decision.PlayedCards),
                    WinningTrickPlayer = decision.WinningTrickPlayer,
                    TeamScore = decision.TeamScore,
                    OpponentScore = decision.OpponentScore,
                    ValidCardsToPlayJson = JsonSerializer.Serialize(decision.ValidCardsToPlay),
                    ChosenCardJson = JsonSerializer.Serialize(decision.ChosenCard),
                    ActorType = actorType,
                    DidTeamWinTrick = didTeamWinTrick,
                    DidTeamWinDeal = didTeamWinDeal,
                    DidTeamWinGame = didTeamWinGame,
                };
            })],
        };
    }
}
