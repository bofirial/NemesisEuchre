using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.Server.Models;
using NemesisEuchre.Server.Services;

namespace NemesisEuchre.Server.Hubs;

[Authorize]
public class GameHub(
    IGameSessionService sessionService,
    IPlayerStateProjector stateProjector,
    IActiveGameService activeGameService,
    IInteractiveTrumpService interactiveTrumpService) : Hub
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

    public async Task StartGameAsync()
    {
        var context = await sessionService.StartGameAsync(Context.ConnectionId);
        if (context is null)
        {
            return;
        }

        var game = activeGameService.GetGame(context.SessionId);
        if (game?.CurrentDeal is not null)
        {
            var sessionLock = activeGameService.GetOrCreateLock(context.SessionId);
            await sessionLock.WaitAsync();
            try
            {
                await interactiveTrumpService.ProcessBotTrumpDecisionsAsync(game.CurrentDeal);
            }
            finally
            {
                sessionLock.Release();
            }

            context = await sessionService.GetSessionContextAsync(context.SessionId, context.SessionName);
        }

        await BroadcastGameStateAsync(context with { Status = GameStatusViewModel.Playing });
    }

    public async Task<string?> MakeTrumpDecisionAsync(CallTrumpDecision decision)
    {
        var info = await sessionService.GetConnectionInfoAsync(Context.ConnectionId);
        if (info is null)
        {
            return "Not in a session";
        }

        var (sessionId, sessionName, myPosition) = info.Value;
        if (myPosition is null)
        {
            return "Not seated";
        }

        var game = activeGameService.GetGame(sessionId);
        if (game?.CurrentDeal is null)
        {
            return "No active game";
        }

        var sessionLock = activeGameService.GetOrCreateLock(sessionId);
        if (!await sessionLock.WaitAsync(0))
        {
            return "Another decision is in progress";
        }

        try
        {
            await interactiveTrumpService.ApplyHumanTrumpDecisionAsync(
                game.CurrentDeal,
                myPosition.Value,
                decision);
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }
        finally
        {
            sessionLock.Release();
        }

        var context = await sessionService.GetSessionContextAsync(sessionId, sessionName);
        await BroadcastGameStateAsync(context with { Status = GameStatusViewModel.Playing });
        return null;
    }

    public async Task<string?> MakeDealerDiscardAsync(Card card)
    {
        var info = await sessionService.GetConnectionInfoAsync(Context.ConnectionId);
        if (info is null)
        {
            return "Not in a session";
        }

        var (sessionId, sessionName, myPosition) = info.Value;
        if (myPosition is null)
        {
            return "Not seated";
        }

        var game = activeGameService.GetGame(sessionId);
        if (game?.CurrentDeal is null)
        {
            return "No active game";
        }

        var sessionLock = activeGameService.GetOrCreateLock(sessionId);
        if (!await sessionLock.WaitAsync(0))
        {
            return "Another decision is in progress";
        }

        try
        {
            await interactiveTrumpService.ApplyHumanDealerDiscardAsync(
                game.CurrentDeal,
                myPosition.Value,
                card);
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }
        finally
        {
            sessionLock.Release();
        }

        var context = await sessionService.GetSessionContextAsync(sessionId, sessionName);
        await BroadcastGameStateAsync(context with { Status = GameStatusViewModel.Playing });
        return null;
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
