using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.Server.Models;
using NemesisEuchre.Server.Services;

namespace NemesisEuchre.Server.Hubs;

[Authorize]
public class GameHub(IGameSessionService sessionService, IPlayerStateProjector stateProjector) : Hub
{
    public async Task<PlayerGameState> JoinGameAsync(string sessionName)
    {
        var githubId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await sessionService.FindUserByGitHubIdAsync(githubId);
        var session = await sessionService.GetOrCreateSessionAsync(sessionName);

        await Groups.AddToGroupAsync(Context.ConnectionId, session.SessionName);

        if (user is not null)
        {
            await sessionService.JoinSessionAsync(session.GameSessionId, user.UserId, Context.ConnectionId);
        }

        var context = await sessionService.GetSessionContextAsync(session.GameSessionId, session.SessionName);

        await BroadcastGameStateAsync(context, Context.ConnectionId);

        var currentMember = context.Members.FirstOrDefault(m => m.Membership.UserId == user?.UserId);
        return currentMember is not null
            ? stateProjector.Project(context, currentMember)
            : stateProjector.Project(context, new ActiveSessionMember
            {
                Membership = new() { GameSessionId = session.GameSessionId, UserId = 0 },
                ConnectionIds = [],
            });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var context = await sessionService.DisconnectAsync(Context.ConnectionId);

        if (context is not null)
        {
            await BroadcastGameStateAsync(context);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task RemoveUserFromSessionAsync(string targetLogin)
    {
        var result = await sessionService.RemoveUserFromSessionAsync(Context.ConnectionId, targetLogin);
        if (result is null)
        {
            return;
        }

        var (context, removedConnectionIds) = result.Value;
        foreach (var connId in removedConnectionIds)
        {
            await Groups.RemoveFromGroupAsync(connId, context.SessionName);
            await Clients.Client(connId).SendAsync("KickedFromSession");
        }

        await BroadcastGameStateAsync(context);
    }

    public async Task PromoteToLeaderAsync(string targetLogin)
    {
        var context = await sessionService.PromoteToLeaderAsync(Context.ConnectionId, targetLogin);
        if (context is null)
        {
            return;
        }

        await BroadcastGameStateAsync(context);
    }

    public async Task ClaimSeatAsync(PlayerPosition position)
    {
        var context = await sessionService.ClaimSeatAsync(Context.ConnectionId, position);
        if (context is null)
        {
            return;
        }

        await BroadcastGameStateAsync(context);
    }

    public async Task VacateSeatAsync()
    {
        var context = await sessionService.VacateSeatAsync(Context.ConnectionId);
        if (context is null)
        {
            return;
        }

        await BroadcastGameStateAsync(context);
    }

    public async Task AddBotToSeatAsync(PlayerPosition position, ActorType actorType, string? modelName)
    {
        var context = await sessionService.AddBotToSeatAsync(Context.ConnectionId, position, actorType, modelName);
        if (context is null)
        {
            return;
        }

        await BroadcastGameStateAsync(context);
    }

    public async Task RemoveBotFromSeatAsync(PlayerPosition position)
    {
        var context = await sessionService.RemoveBotFromSeatAsync(Context.ConnectionId, position);
        if (context is null)
        {
            return;
        }

        await BroadcastGameStateAsync(context);
    }

    private async Task BroadcastGameStateAsync(GameContext context, string? excludeConnectionId = null)
    {
        foreach (var member in context.Members)
        {
            foreach (var connId in member.ConnectionIds.Where(id => id != excludeConnectionId))
            {
                await Clients.Client(connId).SendAsync("ReceiveGameState", stateProjector.Project(context, member));
            }
        }
    }
}
