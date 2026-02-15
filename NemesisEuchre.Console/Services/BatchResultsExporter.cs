using System.Text.Json;

using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Models;
using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Services;

public interface IBatchResultsExporter
{
    void ExportToJson(BatchGameResults results, string outputPath, Actor[]? team1Actors, Actor[]? team2Actors);
}

public class BatchResultsExporter(ILogger<BatchResultsExporter> logger) : IBatchResultsExporter
{
    public void ExportToJson(BatchGameResults results, string outputPath, Actor[]? team1Actors, Actor[]? team2Actors)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));
        }

        var normalizedPath = Path.GetFullPath(outputPath);

        if (!normalizedPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            normalizedPath += ".json";
        }

        var directory = Path.GetDirectoryName(normalizedPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var exportData = BatchGameResultsExport.FromBatchResults(results, team1Actors, team2Actors);

        try
        {
            var json = JsonSerializer.Serialize(exportData, JsonSerializationOptions.WithNaNHandling);
            File.WriteAllText(normalizedPath, json);

            var fileName = Path.GetFileName(normalizedPath);
            Foundation.LoggerMessages.LogResultsExported(logger, fileName);
        }
        catch (Exception ex)
        {
            var fileName = Path.GetFileName(normalizedPath);
            Foundation.LoggerMessages.LogResultsExportFailed(logger, fileName, ex);
            throw new InvalidOperationException($"Failed to export results to {normalizedPath}", ex);
        }
    }
}
