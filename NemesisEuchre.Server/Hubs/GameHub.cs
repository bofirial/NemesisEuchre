using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

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

        IReadOnlyList<ActiveSessionMember> members = [];
        if (user is not null)
        {
            members = await sessionService.JoinSessionAsync(
                session.GameSessionId, user.UserId, Context.ConnectionId);
        }

        var context = new GameContext { SessionName = session.SessionName, Members = members };

        await BroadcastGameStateAsync(context, Context.ConnectionId);

        var currentMember = members.FirstOrDefault(m => m.Membership.UserId == user?.UserId);
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
