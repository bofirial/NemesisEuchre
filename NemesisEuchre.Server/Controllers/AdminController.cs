using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NemesisEuchre.Server.Models;
using NemesisEuchre.Server.Services;

namespace NemesisEuchre.Server.Controllers;

[ApiController]
[Route("api/admin/bots")]
[Authorize(Roles = "Admin")]
public partial class AdminController(IBotStorageService storageService) : ControllerBase
{
    private static readonly (string field, string requiredSuffix)[] BotFileSpecs =
    [
        ("callTrumpZip", "_calltrump.zip"),
        ("callTrumpJson", "_calltrump.json"),
        ("discardCardZip", "_discardcard.zip"),
        ("discardCardJson", "_discardcard.json"),
        ("playCardZip", "_playcard.zip"),
        ("playCardJson", "_playcard.json"),
    ];

    [HttpGet]
    public async Task<IActionResult> ListBotsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var botNames = await storageService.ListBotNamesAsync(cancellationToken);
            return Ok(new { bots = botNames });
        }
        catch (InvalidOperationException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }

    [HttpGet("{botName}")]
    public async Task<IActionResult> GetBotAsync(string botName, CancellationToken cancellationToken)
    {
        try
        {
            var files = await storageService.GetBotFilesAsync(botName, cancellationToken);
            return Ok(new { files });
        }
        catch (InvalidOperationException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateBotAsync([FromForm] BotUploadRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        var trimmedBotName = request.BotName.Trim();
        if (!BotNameRegex().IsMatch(trimmedBotName))
        {
            errors.Add("botName must be 1–50 characters: letters, digits, spaces, hyphens, underscores.");
        }

        var files = CollectAndValidateFiles(GetFileProviders(request), requireAll: true, errors);

        if (errors.Count > 0)
        {
            return BadRequest(new { errors });
        }

        try
        {
            await storageService.UploadBotAsync(trimmedBotName, files, cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }

    [HttpPut("{botName}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateBotAsync(string botName, [FromForm] BotUpdateRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        string? newBotName = null;
        if (request.NewBotName is not null)
        {
            var trimmedNewBotName = request.NewBotName.Trim();
            if (!BotNameRegex().IsMatch(trimmedNewBotName))
            {
                errors.Add("newBotName must be 1–50 characters: letters, digits, spaces, hyphens, underscores.");
            }
            else
            {
                newBotName = trimmedNewBotName;
            }
        }

        var files = CollectAndValidateFiles(GetFileProviders(request), requireAll: false, errors);

        if (errors.Count > 0)
        {
            return BadRequest(new { errors });
        }

        try
        {
            await storageService.UpdateBotAsync(botName, newBotName, files, cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }

    [HttpDelete("{botName}")]
    public async Task<IActionResult> DeleteBotAsync(string botName, CancellationToken cancellationToken)
    {
        try
        {
            await storageService.DeleteBotAsync(botName, cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static (string field, IFormFile? file)[] GetFileProviders(IBotFileRequest request)
    {
        return
        [
            (field: "callTrumpZip", file: request.CallTrumpZip),
            (field: "callTrumpJson", file: request.CallTrumpJson),
            (field: "discardCardZip", file: request.DiscardCardZip),
            (field: "discardCardJson", file: request.DiscardCardJson),
            (field: "playCardZip", file: request.PlayCardZip),
            (field: "playCardJson", file: request.PlayCardJson),
        ];
    }

    private static List<IFormFile> CollectAndValidateFiles(
        (string field, IFormFile? file)[] provided,
        bool requireAll,
        List<string> errors)
    {
        var files = new List<IFormFile>();
        foreach (var ((field, requiredSuffix), (_, file)) in BotFileSpecs.Zip(provided))
        {
            if (file is null || file.Length == 0)
            {
                if (requireAll)
                {
                    errors.Add($"{field} is required.");
                }

                continue;
            }

            if (!file.FileName.EndsWith(requiredSuffix, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"{field} filename must end with '{requiredSuffix}'.");
                continue;
            }

            files.Add(file);
        }

        return files;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9 _-]{1,50}$")]
    private static partial Regex BotNameRegex();
}
