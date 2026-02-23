using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NemesisEuchre.Server.Hubs;

[Authorize]
public class GameHub : Hub
{
    public Task SendMessageAsync(string message)
    {
        return Clients.Caller.SendAsync("ReceiveMessage", Context.UserIdentifier, message);
    }
}
