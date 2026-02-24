using System.Text.RegularExpressions;

using NemesisEuchre.Server.Services;

namespace NemesisEuchre.Server.Endpoints;

public static partial class AdminEndpoints
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

    public static void MapAdminEndpoints(this WebApplication app)
    {
        var bots = app.MapGroup("/api/admin/bots")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        bots.MapGet("/", async (
            IBotStorageService storageService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var botNames = await storageService.ListBotNamesAsync(cancellationToken);
                return Results.Ok(new { bots = botNames });
            }
            catch (InvalidOperationException)
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        });

        bots.MapGet("/{botName}", async (
            string botName,
            IBotStorageService storageService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var files = await storageService.GetBotFilesAsync(botName, cancellationToken);
                return Results.Ok(new { files });
            }
            catch (InvalidOperationException)
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        });

        bots.MapPost("/", async (
            HttpRequest request,
            IBotStorageService storageService,
            CancellationToken cancellationToken) =>
        {
            var form = await request.ReadFormAsync(cancellationToken);
            var errors = new List<string>();

            var botName = form["botName"].FirstOrDefault() ?? string.Empty;
            if (!BotNameRegex().IsMatch(botName))
            {
                errors.Add("botName must be 1–50 characters: letters, digits, hyphens, underscores.");
            }

            var files = new List<IFormFile>();
            foreach (var (field, requiredSuffix) in BotFileSpecs)
            {
                var file = form.Files[field];
                if (file is null || file.Length == 0)
                {
                    errors.Add($"{field} is required.");
                    continue;
                }

                if (!file.FileName.EndsWith(requiredSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"{field} filename must end with '{requiredSuffix}'.");
                    continue;
                }

                files.Add(file);
            }

            if (errors.Count > 0)
            {
                return Results.BadRequest(new { errors });
            }

            try
            {
                await storageService.UploadBotAsync(botName, files, cancellationToken);
                return Results.Ok();
            }
            catch (InvalidOperationException)
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        });

        bots.MapPut("/{botName}", async (
            string botName,
            HttpRequest request,
            IBotStorageService storageService,
            CancellationToken cancellationToken) =>
        {
            var form = await request.ReadFormAsync(cancellationToken);
            var errors = new List<string>();

            var newBotNameRaw = form["newBotName"].FirstOrDefault();
            string? newBotName = null;
            if (newBotNameRaw is not null)
            {
                if (!BotNameRegex().IsMatch(newBotNameRaw))
                {
                    errors.Add("newBotName must be 1–50 characters: letters, digits, hyphens, underscores.");
                }
                else
                {
                    newBotName = newBotNameRaw;
                }
            }

            var files = new List<IFormFile>();
            foreach (var (field, requiredSuffix) in BotFileSpecs)
            {
                var file = form.Files[field];
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

            if (errors.Count > 0)
            {
                return Results.BadRequest(new { errors });
            }

            try
            {
                await storageService.UpdateBotAsync(botName, newBotName, files, cancellationToken);
                return Results.Ok();
            }
            catch (InvalidOperationException)
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        });

        bots.MapDelete("/{botName}", async (
            string botName,
            IBotStorageService storageService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await storageService.DeleteBotAsync(botName, cancellationToken);
                return Results.Ok();
            }
            catch (InvalidOperationException)
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        });
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_-]{1,50}$")]
    private static partial Regex BotNameRegex();
}
