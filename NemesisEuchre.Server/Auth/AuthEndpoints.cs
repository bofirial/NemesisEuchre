using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;

namespace NemesisEuchre.Server.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapGet("/api/auth/login", context => context.ChallengeAsync("GitHub"));

        app.MapGet("/api/auth/user", (ClaimsPrincipal user) =>
        {
            var name = user.FindFirstValue(ClaimTypes.Name);
            var email = user.FindFirstValue(ClaimTypes.Email);
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            return Results.Ok(new { name, login = name, id, email, roles });
        }).RequireAuthorization();

        app.MapGet("/api/auth/logout", () => Results.Ok());
    }
}
