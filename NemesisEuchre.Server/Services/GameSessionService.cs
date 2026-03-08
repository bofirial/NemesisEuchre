using Microsoft.EntityFrameworkCore;

using NemesisEuchre.DataAccess;
using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.Server.Models;

namespace NemesisEuchre.Server.Services;

public interface IGameSessionService
{
    Task<UserEntity> UpsertUserAsync(string githubId, string login, string? email, CancellationToken ct = default);

    Task<UserEntity?> FindUserByGitHubIdAsync(string githubId, CancellationToken ct = default);

    Task<GameSessionEntity> GetOrCreateSessionAsync(string sessionName, CancellationToken ct = default);

    Task<IReadOnlyList<ActiveSessionMember>> JoinSessionAsync(
        int sessionId, int userId, string connectionId, CancellationToken ct = default);

    Task<GameContext> GetSessionContextAsync(int sessionId, string sessionName, CancellationToken ct = default);

    Task<GameContext?> ClaimSeatAsync(string connectionId, PlayerPosition position, CancellationToken ct = default);

    Task<GameContext?> DisconnectAsync(string connectionId, CancellationToken ct = default);

    Task<(GameContext context, IReadOnlyList<string> removedConnectionIds)?> RemoveUserFromSessionAsync(
        string callerConnectionId, string targetLogin, CancellationToken ct = default);

    Task<GameContext?> PromoteToLeaderAsync(
        string callerConnectionId, string targetLogin, CancellationToken ct = default);

    Task<GameContext?> VacateSeatAsync(string connectionId, CancellationToken ct = default);

    Task<GameContext?> AddBotToSeatAsync(
        string connectionId,
        PlayerPosition position,
        ActorType botActorType,
        string? modelName = null,
        CancellationToken ct = default);

    Task<GameContext?> RemoveBotFromSeatAsync(
        string connectionId,
        PlayerPosition position,
        CancellationToken ct = default);

    Task<GameContext?> StartGameAsync(string connectionId, CancellationToken ct = default);
}

public class GameSessionService(NemesisEuchreDbContext db, IActiveGameService activeGameService, IDealFactory dealFactory) : IGameSessionService
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

    public Task<GameContext> GetSessionContextAsync(int sessionId, string sessionName, CancellationToken ct = default)
    {
        return BuildGameContextAsync(sessionId, sessionName, ct);
    }

    public async Task<GameContext?> ClaimSeatAsync(string connectionId, PlayerPosition position, CancellationToken ct = default)
    {
        var connection = await db.GameSessionConnections!
            .Include(c => c.GameSession)
            .FirstOrDefaultAsync(c => c.ConnectionId == connectionId && c.DisconnectedDate == null, ct);
        if (connection is null)
        {
            return null;
        }

        var sessionId = connection.GameSessionId;
        var userId = connection.UserId;

        var existingSeat = await db.GameSessionSeats!
            .FirstOrDefaultAsync(s => s.GameSessionId == sessionId && s.UserId == userId, ct);
        if (existingSeat is not null)
        {
            db.GameSessionSeats!.Remove(existingSeat);
        }

        var targetSeat = await db.GameSessionSeats!
            .FirstOrDefaultAsync(s => s.GameSessionId == sessionId && s.Position == position, ct);
        if (targetSeat is not null)
        {
            targetSeat.UserId = userId;
        }
        else
        {
            db.GameSessionSeats!.Add(new GameSessionSeatEntity
            {
                GameSessionId = sessionId,
                Position = position,
                UserId = userId,
            });
        }

        await db.SaveChangesAsync(ct);

        return await BuildGameContextAsync(sessionId, connection.GameSession!.SessionName, ct);
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

        return await BuildGameContextAsync(connection.GameSessionId, connection.GameSession!.SessionName, ct);
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

        var context = await BuildGameContextAsync(callerConn.GameSessionId, callerConn.GameSession!.SessionName, ct);
        return (context, removedConnectionIds);
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

        return await BuildGameContextAsync(callerConn.GameSessionId, callerConn.GameSession!.SessionName, ct);
    }

    public async Task<GameContext?> VacateSeatAsync(string connectionId, CancellationToken ct = default)
    {
        var connection = await db.GameSessionConnections!
            .Include(c => c.GameSession)
            .FirstOrDefaultAsync(c => c.ConnectionId == connectionId && c.DisconnectedDate == null, ct);
        if (connection is null)
        {
            return null;
        }

        var seat = await db.GameSessionSeats!
            .FirstOrDefaultAsync(s => s.GameSessionId == connection.GameSessionId && s.UserId == connection.UserId, ct);
        if (seat is null)
        {
            return null;
        }

        db.GameSessionSeats!.Remove(seat);
        await db.SaveChangesAsync(ct);

        return await BuildGameContextAsync(connection.GameSessionId, connection.GameSession!.SessionName, ct);
    }

    public async Task<GameContext?> AddBotToSeatAsync(
        string connectionId,
        PlayerPosition position,
        ActorType botActorType,
        string? modelName = null,
        CancellationToken ct = default)
    {
        var connection = await db.GameSessionConnections!
            .Include(c => c.GameSession)
            .FirstOrDefaultAsync(c => c.ConnectionId == connectionId && c.DisconnectedDate == null, ct);
        if (connection is null)
        {
            return null;
        }

        var membership = await db.GameSessionUsers!
            .FirstOrDefaultAsync(m => m.GameSessionId == connection.GameSessionId && m.UserId == connection.UserId, ct);
        if (membership?.IsSessionLeader != true)
        {
            return null;
        }

        var targetSeat = await db.GameSessionSeats!
            .FirstOrDefaultAsync(s => s.GameSessionId == connection.GameSessionId && s.Position == position, ct);
        if (targetSeat is not null)
        {
            return null;
        }

        db.GameSessionSeats!.Add(new GameSessionSeatEntity
        {
            GameSessionId = connection.GameSessionId,
            Position = position,
            UserId = null,
            BotActorType = botActorType,
            BotModelName = modelName,
        });
        await db.SaveChangesAsync(ct);

        return await BuildGameContextAsync(connection.GameSessionId, connection.GameSession!.SessionName, ct);
    }

    public async Task<GameContext?> RemoveBotFromSeatAsync(
        string connectionId,
        PlayerPosition position,
        CancellationToken ct = default)
    {
        var connection = await db.GameSessionConnections!
            .Include(c => c.GameSession)
            .FirstOrDefaultAsync(c => c.ConnectionId == connectionId && c.DisconnectedDate == null, ct);
        if (connection is null)
        {
            return null;
        }

        var membership = await db.GameSessionUsers!
            .FirstOrDefaultAsync(m => m.GameSessionId == connection.GameSessionId && m.UserId == connection.UserId, ct);
        if (membership?.IsSessionLeader != true)
        {
            return null;
        }

        var seat = await db.GameSessionSeats!
            .FirstOrDefaultAsync(s => s.GameSessionId == connection.GameSessionId && s.Position == position && s.UserId == null, ct);
        if (seat is null)
        {
            return null;
        }

        db.GameSessionSeats!.Remove(seat);
        await db.SaveChangesAsync(ct);

        return await BuildGameContextAsync(connection.GameSessionId, connection.GameSession!.SessionName, ct);
    }

    public async Task<GameContext?> StartGameAsync(string connectionId, CancellationToken ct = default)
    {
        var connection = await db.GameSessionConnections!
            .Include(c => c.GameSession)
            .FirstOrDefaultAsync(c => c.ConnectionId == connectionId && c.DisconnectedDate == null, ct);
        if (connection is null)
        {
            return null;
        }

        var membership = await db.GameSessionUsers!
            .FirstOrDefaultAsync(m => m.GameSessionId == connection.GameSessionId && m.UserId == connection.UserId, ct);
        if (membership?.IsSessionLeader != true)
        {
            return null;
        }

        var seats = await db.GameSessionSeats!
            .Include(s => s.User)
            .Where(s => s.GameSessionId == connection.GameSessionId)
            .ToListAsync(ct);

        if (seats.Count < 4)
        {
            return null;
        }

        var actorByPosition = seats.ToDictionary(
            s => s.Position,
            s => s.UserId is not null
                ? new Actor(ActorType.User)
                : new Actor(
                    s.BotActorType!.Value,
                    s.BotModelName is not null ? new Dictionary<string, string> { ["default"] = s.BotModelName } : null));

        var game = new Game
        {
            Players =
            {
                [PlayerPosition.North] = new Player { Position = PlayerPosition.North, Actor = actorByPosition[PlayerPosition.North] },
                [PlayerPosition.East] = new Player { Position = PlayerPosition.East,  Actor = actorByPosition[PlayerPosition.East] },
                [PlayerPosition.South] = new Player { Position = PlayerPosition.South, Actor = actorByPosition[PlayerPosition.South] },
                [PlayerPosition.West] = new Player { Position = PlayerPosition.West,  Actor = actorByPosition[PlayerPosition.West] },
            },
        };

        activeGameService.StoreGame(connection.GameSessionId, game);

        game.CurrentDeal = await dealFactory.CreateDealAsync(game);
        game.CurrentDeal.DealStatus = DealStatus.SelectingTrumpPhase1;

        var context = await BuildGameContextAsync(connection.GameSessionId, connection.GameSession!.SessionName, ct);
        return context with { Status = GameStatusViewModel.Playing };
    }

    private async Task<GameContext> BuildGameContextAsync(int sessionId, string sessionName, CancellationToken ct)
    {
        var members = await GetActiveSessionMembersAsync(sessionId, ct);

        var seats = await db.GameSessionSeats!
            .Include(s => s.User)
            .Where(s => s.GameSessionId == sessionId)
            .ToListAsync(ct);

        var seatInfos = seats.ConvertAll(s => new SeatInfo
        {
            Position = s.Position,
            GitHubLogin = s.User?.GitHubLogin,
            BotActorType = s.BotActorType,
            BotModelName = s.BotModelName,
        });

        return new GameContext
        {
            SessionName = sessionName,
            Members = members,
            Seats = seatInfos,
            ActiveGame = activeGameService.GetGame(sessionId),
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
