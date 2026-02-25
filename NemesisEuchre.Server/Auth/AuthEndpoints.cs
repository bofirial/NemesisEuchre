using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;

using NemesisEuchre.Server.Services;

namespace NemesisEuchre.Server.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapGet("/api/auth/login", context => context.ChallengeAsync("GitHub"));

        app.MapGet("/api/auth/user", async (ClaimsPrincipal user, IGameSessionService sessionService) =>
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var name = user.FindFirstValue(ClaimTypes.Name);
            var email = user.FindFirstValue(ClaimTypes.Email);
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            if (id is not null && name is not null)
            {
                await sessionService.UpsertUserAsync(id, name, email);
            }

            return Results.Ok(new { name, login = name, id, email, roles });
        }).RequireAuthorization();

        app.MapGet("/api/auth/logout", () => Results.Ok());
    }
}
