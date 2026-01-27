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
                    CardsInHandJson = JsonSerializer.Serialize(
                        decision.CardsInHand.Select(c => c.ToRelative(decision.TrumpSuit))),
                    LeadPlayer = decision.LeadPlayer.ToRelativePosition(decision.PlayerPosition),
                    LeadSuit = decision.LeadSuit?.ToRelativeSuit(decision.TrumpSuit),
                    PlayedCardsJson = JsonSerializer.Serialize(
                        decision.PlayedCards.ToDictionary(
                            kvp => kvp.Key.ToRelativePosition(decision.PlayerPosition),
                            kvp => kvp.Value.ToRelative(decision.TrumpSuit))),
                    WinningTrickPlayer = decision.WinningTrickPlayer?.ToRelativePosition(decision.PlayerPosition),
                    TeamScore = decision.TeamScore,
                    OpponentScore = decision.OpponentScore,
                    ValidCardsToPlayJson = JsonSerializer.Serialize(
                        decision.ValidCardsToPlay.Select(c => c.ToRelative(decision.TrumpSuit))),
                    ChosenCardJson = JsonSerializer.Serialize(
                        decision.ChosenCard.ToRelative(decision.TrumpSuit)),
                    ActorType = actorType,
                    DidTeamWinTrick = didTeamWinTrick,
                    DidTeamWinDeal = didTeamWinDeal,
                    DidTeamWinGame = didTeamWinGame,
                };
            })],
        };
    }
}
