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
        ("advancedCallTrumpZip", "_advancedcalltrump.zip"),
        ("advancedCallTrumpJson", "_advancedcalltrump.json"),
        ("discardCardZip", "_discardcard.zip"),
        ("discardCardJson", "_discardcard.json"),
        ("advancedDiscardCardZip", "_advanceddiscardcard.zip"),
        ("advancedDiscardCardJson", "_advanceddiscardcard.json"),
        ("playCardZip", "_playcard.zip"),
        ("playCardJson", "_playcard.json"),
        ("simplePlayCardZip", "_simpleplaycard.zip"),
        ("simplePlayCardJson", "_simpleplaycard.json"),
        ("advancedPlayCardZip", "_advancedplaycard.zip"),
        ("advancedPlayCardJson", "_advancedplaycard.json"),
    ];

    private static readonly (string name, string[] variants)[] BotFileCategories =
    [
        ("Call Trump", ["callTrump", "advancedCallTrump"]),
        ("Discard Card", ["discardCard", "advancedDiscardCard"]),
        ("Play Card", ["playCard", "simplePlayCard", "advancedPlayCard"]),
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

        var files = CollectAndValidateFiles(GetFileProviders(request), requireOnePerCategory: true, errors);

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

        var files = CollectAndValidateFiles(GetFileProviders(request), requireOnePerCategory: false, errors);

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
            (field: "advancedCallTrumpZip", file: request.AdvancedCallTrumpZip),
            (field: "advancedCallTrumpJson", file: request.AdvancedCallTrumpJson),
            (field: "discardCardZip", file: request.DiscardCardZip),
            (field: "discardCardJson", file: request.DiscardCardJson),
            (field: "advancedDiscardCardZip", file: request.AdvancedDiscardCardZip),
            (field: "advancedDiscardCardJson", file: request.AdvancedDiscardCardJson),
            (field: "playCardZip", file: request.PlayCardZip),
            (field: "playCardJson", file: request.PlayCardJson),
            (field: "simplePlayCardZip", file: request.SimplePlayCardZip),
            (field: "simplePlayCardJson", file: request.SimplePlayCardJson),
            (field: "advancedPlayCardZip", file: request.AdvancedPlayCardZip),
            (field: "advancedPlayCardJson", file: request.AdvancedPlayCardJson),
        ];
    }

    private static List<IFormFile> CollectAndValidateFiles(
        (string field, IFormFile? file)[] provided,
        bool requireOnePerCategory,
        List<string> errors)
    {
        var files = new List<IFormFile>();
        var providedByField = provided.ToDictionary(p => p.field, p => p.file);

        foreach (var ((field, requiredSuffix), (_, file)) in BotFileSpecs.Zip(provided))
        {
            if (file is null || file.Length == 0)
            {
                continue;
            }

            if (!file.FileName.EndsWith(requiredSuffix, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"{field} filename must end with '{requiredSuffix}'.");
                continue;
            }

            files.Add(file);
        }

        if (requireOnePerCategory)
        {
            foreach (var (categoryName, variants) in BotFileCategories)
            {
                var variantsWithFiles = variants
                    .Where(v => HasFile(providedByField, v + "Zip") || HasFile(providedByField, v + "Json"))
                    .ToList();

                if (variantsWithFiles.Count == 0)
                {
                    errors.Add($"{categoryName}: one variant is required ({string.Join(" or ", variants)}).");
                }
                else if (variantsWithFiles.Count > 1)
                {
                    errors.Add($"{categoryName}: provide only one variant, not multiple ({string.Join(", ", variantsWithFiles)}).");
                }
                else
                {
                    var variant = variantsWithFiles[0];
                    if (!HasFile(providedByField, variant + "Zip"))
                    {
                        errors.Add($"{variant}Zip is required.");
                    }

                    if (!HasFile(providedByField, variant + "Json"))
                    {
                        errors.Add($"{variant}Json is required.");
                    }
                }
            }
        }

        return files;
    }

    private static bool HasFile(Dictionary<string, IFormFile?> provided, string field)
    {
        return provided.TryGetValue(field, out var file) && file?.Length > 0;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9 _-]{1,50}$")]
    private static partial Regex BotNameRegex();
}
