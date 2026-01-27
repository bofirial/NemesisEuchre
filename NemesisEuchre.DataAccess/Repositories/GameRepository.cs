using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Repositories;

public interface IGameRepository
{
    Task<int> SaveCompletedGameAsync(Game game, CancellationToken cancellationToken = default);
}

public class GameRepository(NemesisEuchreDbContext context, IGameToEntityMapper mapper) : IGameRepository
{
    public async Task<int> SaveCompletedGameAsync(Game game, CancellationToken cancellationToken = default)
    {
        var gameEntity = mapper.Map(game);

        context.Games!.Add(gameEntity);

        await context.SaveChangesAsync(cancellationToken);

        return gameEntity.GameId;
    }
}
