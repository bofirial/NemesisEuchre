using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.Foundation;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Repositories;

public interface IGameRepository
{
    Task SaveCompletedGameAsync(Game game, CancellationToken cancellationToken = default);
}

public class GameRepository(NemesisEuchreDbContext context, ILogger<GameRepository> logger, IGameToEntityMapper mapper) : IGameRepository
{
    public async Task SaveCompletedGameAsync(Game game, CancellationToken cancellationToken = default)
    {
        try
        {
            LoggerMessages.LogPersistingCompletedGame(logger, game.GameStatus);

            var gameEntity = mapper.Map(game);

            context.Games!.Add(gameEntity);

            await context.SaveChangesAsync(cancellationToken);

            LoggerMessages.LogGamePersistedSuccessfully(logger, gameEntity.GameId);
        }
        catch (Exception ex)
        {
            LoggerMessages.LogGamePersistenceFailed(logger, ex);
        }
    }
}
