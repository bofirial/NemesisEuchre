using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NemesisEuchre.Server.Services;

namespace NemesisEuchre.Server.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IGameSessionService sessionService) : ControllerBase
{
    [HttpGet("login")]
    public IActionResult Login()
    {
        return Challenge(new AuthenticationProperties(), "GitHub");
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetUserAsync(CancellationToken cancellationToken)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = User.FindFirstValue(ClaimTypes.Name);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

        if (id is not null && name is not null)
        {
            await sessionService.UpsertUserAsync(id, name, email, cancellationToken);
        }

        return Ok(new { name, login = name, id, email, roles });
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        return Ok();
    }
}
