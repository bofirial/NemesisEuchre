using Microsoft.EntityFrameworkCore;

using NemesisEuchre.DataAccess;
using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Server.Models;

namespace NemesisEuchre.Server.Services;

public interface IGameSessionService
{
    Task<UserEntity> UpsertUserAsync(string githubId, string login, string? email, CancellationToken ct = default);

    Task<UserEntity?> FindUserByGitHubIdAsync(string githubId, CancellationToken ct = default);

    Task<GameSessionEntity> GetOrCreateSessionAsync(string sessionName, CancellationToken ct = default);

    Task<IReadOnlyList<ActiveSessionMember>> JoinSessionAsync(
        int sessionId, int userId, string connectionId, CancellationToken ct = default);

    Task<GameContext?> DisconnectAsync(string connectionId, CancellationToken ct = default);

    Task<(GameContext context, IReadOnlyList<string> removedConnectionIds)?> RemoveUserFromSessionAsync(
        string callerConnectionId, string targetLogin, CancellationToken ct = default);

    Task<GameContext?> PromoteToLeaderAsync(
        string callerConnectionId, string targetLogin, CancellationToken ct = default);
}

public class GameSessionService(NemesisEuchreDbContext db) : IGameSessionService
{
    public async Task<UserEntity> UpsertUserAsync(string githubId, string login, string? email, CancellationToken ct = default)
    {
        var existing = await db.Users!.FirstOrDefaultAsync(u => u.GitHubId == githubId, ct);
        if (existing is not null)
        {
            existing.LastSeenDate = DateTime.UtcNow;
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
        var active = await db.GameSessions!.FirstOrDefaultAsync(
            s => s.SessionName == sessionName && s.AllUsersDisconnectedDate == null,
            ct);

        if (active is not null)
        {
            return active;
        }

        var session = new GameSessionEntity { SessionName = sessionName };
        db.GameSessions!.Add(session);
        await db.SaveChangesAsync(ct);
        return session;
    }

    public async Task<IReadOnlyList<ActiveSessionMember>> JoinSessionAsync(
        int sessionId, int userId, string connectionId, CancellationToken ct = default)
    {
        var membership = await db.GameSessionUsers!.FirstOrDefaultAsync(
            gsu => gsu.GameSessionId == sessionId && gsu.UserId == userId, ct);

        if (membership is null)
        {
            var isFirst = !await db.GameSessionUsers!.AnyAsync(gsu => gsu.GameSessionId == sessionId, ct);
            db.GameSessionUsers!.Add(new GameSessionUserEntity
            {
                GameSessionId = sessionId,
                UserId = userId,
                IsSessionLeader = isFirst,
            });
        }

        db.GameSessionConnections!.Add(new GameSessionConnectionEntity
        {
            ConnectionId = connectionId,
            GameSessionId = sessionId,
            UserId = userId,
        });

        await db.SaveChangesAsync(ct);

        return await GetActiveSessionMembersAsync(sessionId, ct);
    }

    public async Task<GameContext?> DisconnectAsync(string connectionId, CancellationToken ct = default)
    {
        var connection = await db.GameSessionConnections!
            .Include(gsc => gsc.GameSession)
            .FirstOrDefaultAsync(gsc => gsc.ConnectionId == connectionId, ct);

        if (connection is null)
        {
            return null;
        }

        connection.DisconnectedDate = DateTime.UtcNow;

        var hasActiveConnections = await db.GameSessionConnections!.AnyAsync(
            gsc => gsc.GameSessionId == connection.GameSessionId
                && gsc.ConnectionId != connectionId
                && gsc.DisconnectedDate == null,
            ct);

        if (!hasActiveConnections)
        {
            connection.GameSession!.AllUsersDisconnectedDate = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);

        var members = await GetActiveSessionMembersAsync(connection.GameSessionId, ct);

        return new GameContext
        {
            SessionName = connection.GameSession!.SessionName,
            Members = members,
        };
    }

    public async Task<(GameContext context, IReadOnlyList<string> removedConnectionIds)?> RemoveUserFromSessionAsync(
        string callerConnectionId, string targetLogin, CancellationToken ct = default)
    {
        var callerConn = await db.GameSessionConnections!
            .Include(c => c.GameSession)
            .FirstOrDefaultAsync(c => c.ConnectionId == callerConnectionId && c.DisconnectedDate == null, ct);
        if (callerConn is null)
        {
            return null;
        }

        var callerMembership = await db.GameSessionUsers!
            .FirstOrDefaultAsync(m => m.GameSessionId == callerConn.GameSessionId && m.UserId == callerConn.UserId, ct);
        if (callerMembership?.IsSessionLeader != true)
        {
            return null;
        }

        var targetUser = await db.Users!.FirstOrDefaultAsync(u => u.GitHubLogin == targetLogin, ct);
        if (targetUser is null)
        {
            return null;
        }

        var targetConnections = await db.GameSessionConnections!
            .Where(c => c.GameSessionId == callerConn.GameSessionId
                && c.UserId == targetUser.UserId
                && c.DisconnectedDate == null)
            .ToListAsync(ct);

        var removedConnectionIds = targetConnections.ConvertAll(c => c.ConnectionId);
        foreach (var conn in targetConnections)
        {
            conn.DisconnectedDate = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);

        var members = await GetActiveSessionMembersAsync(callerConn.GameSessionId, ct);
        return (context: new GameContext { SessionName = callerConn.GameSession!.SessionName, Members = members }, removedConnectionIds);
    }

    public async Task<GameContext?> PromoteToLeaderAsync(
        string callerConnectionId, string targetLogin, CancellationToken ct = default)
    {
        var callerConn = await db.GameSessionConnections!
            .Include(c => c.GameSession)
            .FirstOrDefaultAsync(c => c.ConnectionId == callerConnectionId && c.DisconnectedDate == null, ct);
        if (callerConn is null)
        {
            return null;
        }

        var callerMembership = await db.GameSessionUsers!
            .FirstOrDefaultAsync(m => m.GameSessionId == callerConn.GameSessionId && m.UserId == callerConn.UserId, ct);
        if (callerMembership?.IsSessionLeader != true)
        {
            return null;
        }

        var targetUser = await db.Users!.FirstOrDefaultAsync(u => u.GitHubLogin == targetLogin, ct);
        if (targetUser is null)
        {
            return null;
        }

        var targetMembership = await db.GameSessionUsers!
            .FirstOrDefaultAsync(m => m.GameSessionId == callerConn.GameSessionId && m.UserId == targetUser.UserId, ct);
        if (targetMembership is null)
        {
            return null;
        }

        targetMembership.IsSessionLeader = true;
        await db.SaveChangesAsync(ct);

        return new GameContext
        {
            SessionName = callerConn.GameSession!.SessionName,
            Members = await GetActiveSessionMembersAsync(callerConn.GameSessionId, ct),
        };
    }

    private async Task<IReadOnlyList<ActiveSessionMember>> GetActiveSessionMembersAsync(
        int sessionId, CancellationToken ct)
    {
        var memberships = await db.GameSessionUsers!
            .Include(gsu => gsu.User)
            .Where(gsu => gsu.GameSessionId == sessionId
                && db.GameSessionConnections!.Any(
                    gsc => gsc.GameSessionId == sessionId
                        && gsc.UserId == gsu.UserId
                        && gsc.DisconnectedDate == null))
            .ToListAsync(ct);

        var activeConnections = await db.GameSessionConnections!
            .Where(gsc => gsc.GameSessionId == sessionId && gsc.DisconnectedDate == null)
            .ToListAsync(ct);

        return memberships.ConvertAll(m => new ActiveSessionMember
        {
            Membership = m,
            ConnectionIds = [.. activeConnections
                .Where(c => c.UserId == m.UserId)
                .Select(c => c.ConnectionId)],
        });
    }
}
