using Microsoft.EntityFrameworkCore;

using NemesisEuchre.DataAccess;
using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.Server.Services;

public interface IGameSessionService
{
    Task<UserEntity> UpsertUserAsync(string githubId, string login, string? email, CancellationToken ct = default);

    Task<UserEntity?> FindUserByGitHubIdAsync(string githubId, CancellationToken ct = default);

    Task<GameSessionEntity> GetOrCreateSessionAsync(string sessionName, CancellationToken ct = default);

    Task AddUserToSessionAsync(int sessionId, int userId, CancellationToken ct = default);
}

public class GameSessionService(NemesisEuchreDbContext db) : IGameSessionService
{
    public async Task<UserEntity> UpsertUserAsync(string githubId, string login, string? email, CancellationToken ct = default)
    {
        var existing = await db.Users!.FirstOrDefaultAsync(u => u.GitHubId == githubId, ct);
        if (existing is not null)
        {
            existing.LastSeenAt = DateTime.UtcNow;
            existing.Email = email;
            await db.SaveChangesAsync(ct);
            return existing;
        }

        var user = new UserEntity
        {
            GitHubId = githubId,
            GitHubLogin = login,
            Email = email,
        };
        db.Users!.Add(user);
        await db.SaveChangesAsync(ct);
        return user;
    }

    public Task<UserEntity?> FindUserByGitHubIdAsync(string githubId, CancellationToken ct = default)
    {
        return db.Users!.FirstOrDefaultAsync(u => u.GitHubId == githubId, ct);
    }

    public async Task<GameSessionEntity> GetOrCreateSessionAsync(string sessionName, CancellationToken ct = default)
    {
        var existing = await db.GameSessions!.FirstOrDefaultAsync(s => s.SessionName == sessionName, ct);
        if (existing is not null)
        {
            return existing;
        }

        var session = new GameSessionEntity { SessionName = sessionName };
        db.GameSessions!.Add(session);
        await db.SaveChangesAsync(ct);
        return session;
    }

    public async Task AddUserToSessionAsync(int sessionId, int userId, CancellationToken ct = default)
    {
        var alreadyJoined = await db.GameSessionUsers!.AnyAsync(
            gsu => gsu.GameSessionId == sessionId && gsu.UserId == userId, ct);

        if (alreadyJoined)
        {
            return;
        }

        db.GameSessionUsers!.Add(new GameSessionUserEntity
        {
            GameSessionId = sessionId,
            UserId = userId,
        });
        await db.SaveChangesAsync(ct);
    }
}
