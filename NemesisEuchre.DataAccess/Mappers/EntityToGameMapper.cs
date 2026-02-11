using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Mappers;

public interface IEntityToGameMapper
{
    Game Map(GameEntity entity, bool includeDecisions);
}

public class EntityToGameMapper(IEntityToDealMapper dealMapper) : IEntityToGameMapper
{
    public Game Map(GameEntity entity, bool includeDecisions)
    {
        var game = new Game
        {
            GameStatus = (GameStatus)entity.GameStatusId,
            Team1Score = entity.Team1Score,
            Team2Score = entity.Team2Score,
            WinningTeam = entity.WinningTeamId.HasValue ? (Team)entity.WinningTeamId.Value : null,
        };

        foreach (var gp in entity.GamePlayers)
        {
            var position = (PlayerPosition)gp.PlayerPositionId;
            game.Players[position] = new Player
            {
                Position = position,
                Actor = new Actor((ActorType)gp.ActorTypeId, null),
            };
        }

        foreach (var dealEntity in entity.Deals.OrderBy(d => d.DealNumber))
        {
            game.CompletedDeals.Add(dealMapper.Map(dealEntity, game.Players, includeDecisions));
        }

        return game;
    }
}
