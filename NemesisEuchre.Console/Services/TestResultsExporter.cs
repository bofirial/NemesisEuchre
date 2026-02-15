using System.Text.Json;

using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Models.BehavioralTests;
using NemesisEuchre.DataAccess.Configuration;

namespace NemesisEuchre.Console.Services;

public interface ITestResultsExporter
{
    void ExportToJson(BehavioralTestSuiteResult suiteResult, string outputPath);
}

public class TestResultsExporter(ILogger<TestResultsExporter> logger) : ITestResultsExporter
{
    public void ExportToJson(BehavioralTestSuiteResult suiteResult, string outputPath)
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

        var exportData = TestResultsExport.FromSuiteResult(suiteResult);

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
