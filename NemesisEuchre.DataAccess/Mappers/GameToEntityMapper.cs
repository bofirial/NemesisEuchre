using System.Text.Json;

using NemesisEuchre.DataAccess.Configuration;
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
            GameStatus = game.GameStatus,
            PlayersJson = JsonSerializer.Serialize(game.Players, JsonSerializationOptions.Default),
            Team1Score = game.Team1Score,
            Team2Score = game.Team2Score,
            WinningTeam = game.WinningTeam,
            CreatedAt = DateTime.UtcNow,
            Deals = [.. game.CompletedDeals.Select((deal, index) => dealMapper.Map(deal, index + 1, game.Players, didTeam1WinGame, didTeam2WinGame))],
        };
    }
}
