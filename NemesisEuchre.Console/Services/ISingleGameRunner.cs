using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Services;

/// <summary>
/// Service responsible for orchestrating single game execution including
/// game play, persistence, and result rendering.
/// </summary>
public interface ISingleGameRunner
{
    /// <summary>
    /// Executes a single game from start to finish, handling orchestration,
    /// persistence, and rendering.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The completed game result.</returns>
    Task<Game> RunAsync(CancellationToken cancellationToken = default);
}
