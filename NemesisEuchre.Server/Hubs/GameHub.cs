using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.Server.Models;
using NemesisEuchre.Server.Services;

namespace NemesisEuchre.Server.Hubs;

[Authorize]
public class GameHub(IGameSessionService sessionService) : Hub
{
    public async Task<PlayerGameState> JoinGameAsync(string gameName)
    {
        var githubId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await sessionService.FindUserByGitHubIdAsync(githubId);
        var session = await sessionService.GetOrCreateSessionAsync(gameName);
        if (user is not null)
        {
            await sessionService.AddUserToSessionAsync(session.GameSessionId, user.UserId);
        }

        return new PlayerGameState
        {
            SessionName = session.SessionName,
            GameStatus = GameStatusViewModel.Lobby,
            MyPosition = PlayerPosition.South,
            Players = new Dictionary<PlayerPosition, PlayerInfo>(),
            Team1Score = 0,
            Team2Score = 0,
        };
    }
}
