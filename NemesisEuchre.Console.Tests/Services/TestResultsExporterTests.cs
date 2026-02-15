using System.Text.Json;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Models.BehavioralTests;
using NemesisEuchre.Console.Services;
using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Tests.Services;

public class TestResultsExporterTests : IDisposable
{
    private readonly Mock<ILogger<TestResultsExporter>> _mockLogger;
    private readonly TestResultsExporter _exporter;
    private readonly string _tempDirectory;
    private readonly List<string> _filesToCleanup;

    public TestResultsExporterTests()
    {
        _mockLogger = new Mock<ILogger<TestResultsExporter>>();
        _exporter = new TestResultsExporter(_mockLogger.Object);
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"TestResultsExporterTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
        _filesToCleanup = [];
    }

    public void Dispose()
    {
        foreach (var file in _filesToCleanup)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ExportToJson_WithValidResults_WritesJsonFile()
    {
        var suiteResult = CreateSampleSuiteResult();
        var outputPath = Path.Combine(_tempDirectory, "test-results.json");
        _filesToCleanup.Add(outputPath);

        _exporter.ExportToJson(suiteResult, outputPath);

        File.Exists(outputPath).Should().BeTrue();
        var jsonContent = File.ReadAllText(outputPath);
        jsonContent.Should().Contain("\"ModelName\": \"gen2\"");
        jsonContent.Should().Contain("\"TotalTests\": 3");
    }

    [Fact]
    public void ExportToJson_WithRelativePath_ResolvesToAbsolutePath()
    {
        var suiteResult = CreateSampleSuiteResult();
        const string relativePath = "relative-test-results.json";
        var expectedPath = Path.GetFullPath(relativePath);
        _filesToCleanup.Add(expectedPath);

        _exporter.ExportToJson(suiteResult, relativePath);

        File.Exists(expectedPath).Should().BeTrue();
    }

    [Fact]
    public void ExportToJson_WithMissingDirectory_CreatesDirectory()
    {
        var suiteResult = CreateSampleSuiteResult();
        var nestedPath = Path.Combine(_tempDirectory, "nested", "output", "results.json");
        _filesToCleanup.Add(nestedPath);

        _exporter.ExportToJson(suiteResult, nestedPath);

        File.Exists(nestedPath).Should().BeTrue();
        Directory.Exists(Path.GetDirectoryName(nestedPath)).Should().BeTrue();
    }

    [Fact]
    public void ExportToJson_WithMissingExtension_AppendsJsonExtension()
    {
        var suiteResult = CreateSampleSuiteResult();
        var pathWithoutExtension = Path.Combine(_tempDirectory, "results-no-ext");
        var expectedPath = pathWithoutExtension + ".json";
        _filesToCleanup.Add(expectedPath);

        _exporter.ExportToJson(suiteResult, pathWithoutExtension);

        File.Exists(expectedPath).Should().BeTrue();
    }

    [Fact]
    public void ExportToJson_WithPassedTests_IncludesSummaryStatistics()
    {
        var results = new List<BehavioralTestResult>
        {
            CreateTestResult("Test1", DecisionType.Play, passed: true),
            CreateTestResult("Test2", DecisionType.CallTrump, passed: true),
            CreateTestResult("Test3", DecisionType.Discard, passed: true),
        };
        var suiteResult = new BehavioralTestSuiteResult("gen2", results, TimeSpan.FromSeconds(5));
        var outputPath = Path.Combine(_tempDirectory, "passed-results.json");
        _filesToCleanup.Add(outputPath);

        _exporter.ExportToJson(suiteResult, outputPath);

        var jsonContent = File.ReadAllText(outputPath);
        jsonContent.Should().Contain("\"TotalTests\": 3");
        jsonContent.Should().Contain("\"PassedTests\": 3");
        jsonContent.Should().Contain("\"FailedTests\": 0");
        jsonContent.Should().Contain("\"PassRate\": 1");
    }

    [Fact]
    public void ExportToJson_WithFailedTests_IncludesFailureReasons()
    {
        var results = new List<BehavioralTestResult>
        {
            CreateTestResult("Test1", DecisionType.Play, passed: false, failureReason: "Expected trump card"),
            CreateTestResult("Test2", DecisionType.CallTrump, passed: true),
        };
        var suiteResult = new BehavioralTestSuiteResult("gen2", results, TimeSpan.FromSeconds(5));
        var outputPath = Path.Combine(_tempDirectory, "failed-results.json");
        _filesToCleanup.Add(outputPath);

        _exporter.ExportToJson(suiteResult, outputPath);

        var jsonContent = File.ReadAllText(outputPath);
        jsonContent.Should().Contain("\"FailureReason\": \"Expected trump card\"");
    }

    [Fact]
    public void ExportToJson_WithMixedResults_ComputesCorrectPassRate()
    {
        var results = new List<BehavioralTestResult>
        {
            CreateTestResult("Test1", DecisionType.Play, passed: true),
            CreateTestResult("Test2", DecisionType.Play, passed: false),
            CreateTestResult("Test3", DecisionType.CallTrump, passed: true),
            CreateTestResult("Test4", DecisionType.Discard, passed: true),
        };
        var suiteResult = new BehavioralTestSuiteResult("gen2", results, TimeSpan.FromSeconds(5));
        var outputPath = Path.Combine(_tempDirectory, "mixed-results.json");
        _filesToCleanup.Add(outputPath);

        _exporter.ExportToJson(suiteResult, outputPath);

        var jsonContent = File.ReadAllText(outputPath);
        var exportData = JsonSerializer.Deserialize<TestResultsExport>(jsonContent, JsonSerializationOptions.WithNaNHandling);
        exportData.Should().NotBeNull();
        exportData!.PassRate.Should().BeApproximately(0.75, 0.001);
    }

    [Fact]
    public void ExportToJson_WithAllResults_IncludesOptionScores()
    {
        var optionScores = new Dictionary<string, float>
        {
            ["9♣"] = 0.8542f,
            ["J♥"] = 0.1234f,
            ["A♠"] = 0.0224f,
        };
        var results = new List<BehavioralTestResult>
        {
            new(
                "Test1",
                DecisionType.Play,
                true,
                "9♣",
                "Should play lowest",
                optionScores,
                null),
        };
        var suiteResult = new BehavioralTestSuiteResult("gen2", results, TimeSpan.FromSeconds(5));
        var outputPath = Path.Combine(_tempDirectory, "scores-results.json");
        _filesToCleanup.Add(outputPath);

        _exporter.ExportToJson(suiteResult, outputPath);

        var jsonContent = File.ReadAllText(outputPath);
        jsonContent.Should().Contain("\"OptionScores\"");

        var exportData = JsonSerializer.Deserialize<TestResultsExport>(jsonContent, JsonSerializationOptions.WithNaNHandling);
        exportData.Should().NotBeNull();
        exportData!.TestResults[0].OptionScores.Should().ContainKey("9♣");
        exportData.TestResults[0].OptionScores["9♣"].Should().BeApproximately(0.8542f, 0.0001f);
        exportData.TestResults[0].OptionScores["J♥"].Should().BeApproximately(0.1234f, 0.0001f);
        exportData.TestResults[0].OptionScores["A♠"].Should().BeApproximately(0.0224f, 0.0001f);
    }

    [Fact]
    public void ExportToJson_SerializedJson_CanBeDeserialized()
    {
        var suiteResult = CreateSampleSuiteResult();
        var outputPath = Path.Combine(_tempDirectory, "deserialization-test.json");
        _filesToCleanup.Add(outputPath);

        _exporter.ExportToJson(suiteResult, outputPath);

        var jsonContent = File.ReadAllText(outputPath);
        var deserialized = JsonSerializer.Deserialize<TestResultsExport>(jsonContent, JsonSerializationOptions.WithNaNHandling);

        deserialized.Should().NotBeNull();
        deserialized!.ModelName.Should().Be("gen2");
        deserialized.TotalTests.Should().Be(3);
        deserialized.PassedTests.Should().Be(2);
        deserialized.FailedTests.Should().Be(1);
        deserialized.PassRate.Should().BeApproximately(0.6666, 0.001);
        deserialized.TestResults.Should().HaveCount(3);
        deserialized.TestsByDecisionType.Should().ContainKeys(
            DecisionType.Play,
            DecisionType.CallTrump,
            DecisionType.Discard);
        deserialized.GeneratedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ExportToJson_WithNullOutputPath_ThrowsArgumentException()
    {
        var suiteResult = CreateSampleSuiteResult();

        var act = () => _exporter.ExportToJson(suiteResult, null!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Output path cannot be null or empty*");
    }

    [Fact]
    public void ExportToJson_WithEmptyOutputPath_ThrowsArgumentException()
    {
        var suiteResult = CreateSampleSuiteResult();

        var act = () => _exporter.ExportToJson(suiteResult, string.Empty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Output path cannot be null or empty*");
    }

    [Fact]
    public void ExportToJson_GroupsByDecisionType_WithPerCategoryStatistics()
    {
        var results = new List<BehavioralTestResult>
        {
            CreateTestResult("Test1", DecisionType.Play, passed: true),
            CreateTestResult("Test2", DecisionType.Play, passed: false),
            CreateTestResult("Test3", DecisionType.CallTrump, passed: true),
            CreateTestResult("Test4", DecisionType.CallTrump, passed: true),
            CreateTestResult("Test5", DecisionType.Discard, passed: true),
        };
        var suiteResult = new BehavioralTestSuiteResult("gen2", results, TimeSpan.FromSeconds(5));
        var outputPath = Path.Combine(_tempDirectory, "grouped-results.json");
        _filesToCleanup.Add(outputPath);

        _exporter.ExportToJson(suiteResult, outputPath);

        var jsonContent = File.ReadAllText(outputPath);
        var exportData = JsonSerializer.Deserialize<TestResultsExport>(jsonContent, JsonSerializationOptions.WithNaNHandling);

        exportData.Should().NotBeNull();
        exportData!.TestsByDecisionType.Should().ContainKey(DecisionType.Play);
        exportData.TestsByDecisionType[DecisionType.Play].Total.Should().Be(2);
        exportData.TestsByDecisionType[DecisionType.Play].Passed.Should().Be(1);
        exportData.TestsByDecisionType[DecisionType.Play].Failed.Should().Be(1);
        exportData.TestsByDecisionType[DecisionType.Play].PassRate.Should().BeApproximately(0.5, 0.001);

        exportData.TestsByDecisionType[DecisionType.CallTrump].Total.Should().Be(2);
        exportData.TestsByDecisionType[DecisionType.CallTrump].Passed.Should().Be(2);
        exportData.TestsByDecisionType[DecisionType.CallTrump].PassRate.Should().Be(1.0);

        exportData.TestsByDecisionType[DecisionType.Discard].Total.Should().Be(1);
        exportData.TestsByDecisionType[DecisionType.Discard].Passed.Should().Be(1);
        exportData.TestsByDecisionType[DecisionType.Discard].PassRate.Should().Be(1.0);
    }

    private static BehavioralTestSuiteResult CreateSampleSuiteResult()
    {
        var results = new List<BehavioralTestResult>
        {
            CreateTestResult("Test1", DecisionType.Play, passed: true),
            CreateTestResult("Test2", DecisionType.CallTrump, passed: true),
            CreateTestResult("Test3", DecisionType.Discard, passed: false, failureReason: "Wrong choice"),
        };
        return new BehavioralTestSuiteResult("gen2", results, TimeSpan.FromSeconds(5));
    }

    private static BehavioralTestResult CreateTestResult(
        string testName,
        DecisionType decisionType,
        bool passed,
        string? failureReason = null)
    {
        var optionScores = new Dictionary<string, float>
        {
            ["Option1"] = 0.7f,
            ["Option2"] = 0.3f,
        };

        return new BehavioralTestResult(
            testName,
            decisionType,
            passed,
            "Option1",
            "Expected behavior",
            optionScores,
            failureReason);
    }
}
