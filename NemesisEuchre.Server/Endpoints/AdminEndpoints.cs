using System.Text.RegularExpressions;

using NemesisEuchre.Server.Services;

namespace NemesisEuchre.Server.Endpoints;

public static partial class AdminEndpoints
{
    private static readonly (string field, string requiredSuffix)[] ModelFileSpecs =
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
        app.MapPost("/api/admin/models", async (
            HttpRequest request,
            IModelStorageService storageService,
            CancellationToken cancellationToken) =>
        {
            var form = await request.ReadFormAsync(cancellationToken);

            var errors = new List<string>();

            var modelName = form["modelName"].FirstOrDefault() ?? string.Empty;
            if (!ModelNameRegex().IsMatch(modelName))
            {
                errors.Add("modelName must be 1–50 characters: letters, digits, hyphens, underscores.");
            }

            var files = new List<IFormFile>();
            foreach (var (field, requiredSuffix) in ModelFileSpecs)
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
                await storageService.UploadModelAsync(modelName, files, cancellationToken);
                return Results.Ok();
            }
            catch (InvalidOperationException)
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_-]{1,50}$")]
    private static partial Regex ModelNameRegex();
}
