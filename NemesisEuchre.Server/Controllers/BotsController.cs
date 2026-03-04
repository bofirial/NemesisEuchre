using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NemesisEuchre.Server.Services;

namespace NemesisEuchre.Server.Controllers;

[ApiController]
[Route("api/bots")]
[Authorize]
public class BotsController(IBotStorageService botStorageService) : ControllerBase
{
    private static readonly string[] BuiltinBots = ["Chaos", "Beta", "Chad"];

    [HttpGet]
    public async Task<IActionResult> ListBotsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var azureBots = await botStorageService.ListBotNamesAsync(cancellationToken);
            return Ok(new { builtinBots = BuiltinBots, azureBots });
        }
        catch (InvalidOperationException)
        {
            return Ok(new { builtinBots = BuiltinBots, azureBots = Array.Empty<string>() });
        }
    }
}
