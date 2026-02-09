using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Mappers;

public interface IGameToEntityMapper
{
    GameEntity Map(Game game);
}

public class GameToEntityMapper(IDealToEntityMapper dealMapper) : IGameToEntityMapper
{
    public GameEntity Map(Game game)
    {
        var didTeam1WinGame = game.WinningTeam == Team.Team1;
        var didTeam2WinGame = game.WinningTeam == Team.Team2;

        return new GameEntity
        {
            GameStatusId = (int)game.GameStatus,
            Team1Score = game.Team1Score,
            Team2Score = game.Team2Score,
            WinningTeamId = game.WinningTeam.HasValue ? (int)game.WinningTeam.Value : null,
            CreatedAt = DateTime.UtcNow,
            GamePlayers = [.. game.Players.Select(kvp => new GamePlayer
            {
                PlayerPositionId = (int)kvp.Key,
                ActorTypeId = kvp.Value.ActorType.HasValue ? (int)kvp.Value.ActorType.Value : null,
            })],
            Deals = [.. game.CompletedDeals.Select((deal, index) => dealMapper.Map(deal, index + 1, game.Players, didTeam1WinGame, didTeam2WinGame))],
        };
    }
}
