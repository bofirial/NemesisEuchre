using DotMake.CommandLine;

using NemesisEuchre.Console.Services.BehavioralTests;

using Spectre.Console;

namespace NemesisEuchre.Console.Commands;

[CliCommand(
    Description = "Run behavioral tests against a trained ML model",
    Parent = typeof(DefaultCommand))]
public class TestCommand(
    IAnsiConsole ansiConsole,
    IModelBehavioralTestRunner testRunner,
    ITestResultsRenderer resultsRenderer) : ICliRunAsyncWithReturn
{
    [CliOption(
        Description = "Name of the trained model to test (e.g. Gen2)",
        Alias = "m")]
    public required string ModelName { get; set; }

    public async Task<int> RunAsync()
    {
        ansiConsole.WriteLine();
        ansiConsole.MarkupLine($"[dim]Running behavioral tests for model: {ModelName}[/]");

        var suiteResult = await Task.Run(() => testRunner.RunTests(ModelName));

        resultsRenderer.RenderResults(suiteResult);

        return suiteResult.Results.All(r => r.Passed) ? 0 : 1;
    }
}
