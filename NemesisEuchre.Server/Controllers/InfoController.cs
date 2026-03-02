using Microsoft.AspNetCore.Mvc;

namespace NemesisEuchre.Server.Controllers;

[ApiController]
[Route("api")]
public class InfoController : ControllerBase
{
    [HttpGet("version")]
    public IActionResult GetVersion()
    {
        return Ok(new { version = ThisAssembly.AssemblyInformationalVersion });
    }
}
