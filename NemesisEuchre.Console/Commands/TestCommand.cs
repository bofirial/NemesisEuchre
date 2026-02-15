using DotMake.CommandLine;

using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Services;
using NemesisEuchre.Console.Services.BehavioralTests;

using Spectre.Console;

namespace NemesisEuchre.Console.Commands;

[CliCommand(
    Description = "Run behavioral tests against a trained ML model",
    Parent = typeof(DefaultCommand))]
public class TestCommand(
    IAnsiConsole ansiConsole,
    IModelBehavioralTestRunner testRunner,
    ITestResultsRenderer resultsRenderer,
    ITestResultsExporter testResultsExporter,
    ILogger<TestCommand> logger) : ICliRunAsyncWithReturn
{
    [CliOption(
        Description = "Name of the trained model to test (e.g. Gen2)",
        Alias = "m")]
    public required string ModelName { get; set; }

    [CliOption(
        Description = "Export test results to JSON file for automation and analysis",
        Required = false,
        Alias = "json")]
    public string? OutputJson { get; set; }

    public async Task<int> RunAsync()
    {
        ansiConsole.WriteLine();
        ansiConsole.MarkupLine($"[dim]Running behavioral tests for model: {ModelName}[/]");

        var suiteResult = await Task.Run(() => testRunner.RunTests(ModelName));

        resultsRenderer.RenderResults(suiteResult);

        if (!string.IsNullOrWhiteSpace(OutputJson))
        {
            try
            {
                testResultsExporter.ExportToJson(suiteResult, OutputJson);
                var exportedPath = Path.GetFullPath(OutputJson);
                ansiConsole.MarkupLine($"[green]✓ Test results exported to: {exportedPath}[/]");
            }
            catch (Exception ex)
            {
                Foundation.LoggerMessages.LogResultsExportFailed(logger, OutputJson, ex);
                ansiConsole.MarkupLine($"[yellow]⚠ Warning: Failed to export results - {ex.Message}[/]");
            }
        }

        return suiteResult.Results.All(r => r.Passed) ? 0 : 1;
    }
}
