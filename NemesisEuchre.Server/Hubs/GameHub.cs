using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.Server.Models;

namespace NemesisEuchre.Server.Hubs;

[Authorize]
public class GameHub : Hub
{
    public Task SendMessageAsync(string message)
    {
        return Clients.Caller.SendAsync("ReceiveMessage", Context.UserIdentifier, message);
    }

    public Task<PlayerGameState> JoinGameAsync(string gameName)
    {
        return Task.FromResult(new PlayerGameState
        {
            GameName = gameName,
            GameStatus = GameStatusViewModel.Lobby,
            MyPosition = PlayerPosition.South,
            Players = new Dictionary<PlayerPosition, PlayerInfo>(),
            Team1Score = 0,
            Team2Score = 0,
            CurrentDeal = null,
        });
    }
}
