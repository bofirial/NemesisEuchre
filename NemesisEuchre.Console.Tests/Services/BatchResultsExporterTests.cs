using System.Text.Json;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;
using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Tests.Services;

public class BatchResultsExporterTests : IDisposable
{
    private readonly Mock<ILogger<BatchResultsExporter>> _mockLogger;
    private readonly BatchResultsExporter _exporter;
    private readonly string _tempDirectory;
    private readonly List<string> _filesToCleanup;

    public BatchResultsExporterTests()
    {
        _mockLogger = new Mock<ILogger<BatchResultsExporter>>();
        _exporter = new BatchResultsExporter(_mockLogger.Object);
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"BatchResultsExporterTests_{Guid.NewGuid()}");
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
        var results = new BatchGameResults
        {
            TotalGames = 100,
            Team1Wins = 55,
            Team2Wins = 45,
            FailedGames = 0,
            TotalDeals = 500,
            TotalTricks = 2500,
            TotalCallTrumpDecisions = 500,
            TotalDiscardCardDecisions = 250,
            TotalPlayCardDecisions = 2500,
            ElapsedTime = TimeSpan.FromMinutes(5),
        };

        var outputPath = Path.Combine(_tempDirectory, "test-results.json");
        _filesToCleanup.Add(outputPath);

        _exporter.ExportToJson(results, outputPath, null, null);

        File.Exists(outputPath).Should().BeTrue();
        var jsonContent = File.ReadAllText(outputPath);
        jsonContent.Should().Contain("\"TotalGames\": 100");
        jsonContent.Should().Contain("\"Team1Wins\": 55");
        jsonContent.Should().Contain("\"Team2Wins\": 45");
    }

    [Fact]
    public void ExportToJson_WithRelativePath_ResolvesToAbsolutePath()
    {
        var results = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 5,
            Team2Wins = 5,
            FailedGames = 0,
            TotalDeals = 50,
            TotalTricks = 250,
            TotalCallTrumpDecisions = 50,
            TotalDiscardCardDecisions = 25,
            TotalPlayCardDecisions = 250,
            ElapsedTime = TimeSpan.FromSeconds(30),
        };

        const string relativePath = "relative-results.json";
        var expectedPath = Path.GetFullPath(relativePath);
        _filesToCleanup.Add(expectedPath);

        _exporter.ExportToJson(results, relativePath, null, null);

        File.Exists(expectedPath).Should().BeTrue();
    }

    [Fact]
    public void ExportToJson_WithMissingDirectory_CreatesDirectory()
    {
        var results = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 5,
            Team2Wins = 5,
            FailedGames = 0,
            TotalDeals = 50,
            TotalTricks = 250,
            TotalCallTrumpDecisions = 50,
            TotalDiscardCardDecisions = 25,
            TotalPlayCardDecisions = 250,
            ElapsedTime = TimeSpan.FromSeconds(30),
        };

        var nestedPath = Path.Combine(_tempDirectory, "nested", "output", "results.json");
        _filesToCleanup.Add(nestedPath);

        _exporter.ExportToJson(results, nestedPath, null, null);

        File.Exists(nestedPath).Should().BeTrue();
        Directory.Exists(Path.GetDirectoryName(nestedPath)).Should().BeTrue();
    }

    [Fact]
    public void ExportToJson_WithMissingExtension_AppendsJsonExtension()
    {
        var results = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 5,
            Team2Wins = 5,
            FailedGames = 0,
            TotalDeals = 50,
            TotalTricks = 250,
            TotalCallTrumpDecisions = 50,
            TotalDiscardCardDecisions = 25,
            TotalPlayCardDecisions = 250,
            ElapsedTime = TimeSpan.FromSeconds(30),
        };

        var pathWithoutExtension = Path.Combine(_tempDirectory, "results-no-ext");
        var expectedPath = pathWithoutExtension + ".json";
        _filesToCleanup.Add(expectedPath);

        _exporter.ExportToJson(results, pathWithoutExtension, null, null);

        File.Exists(expectedPath).Should().BeTrue();
    }

    [Fact]
    public void ExportToJson_WithTeamActors_IncludesTeamConfigurations()
    {
        var results = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 5,
            Team2Wins = 5,
            FailedGames = 0,
            TotalDeals = 50,
            TotalTricks = 250,
            TotalCallTrumpDecisions = 50,
            TotalDiscardCardDecisions = 25,
            TotalPlayCardDecisions = 250,
            ElapsedTime = TimeSpan.FromSeconds(30),
        };

        var team1Actors = new[] { new Actor(ActorType.Model, "gen2", 0.0f) };
        var team2Actors = new[] { new Actor(ActorType.Chaos) };

        var outputPath = Path.Combine(_tempDirectory, "teams-results.json");
        _filesToCleanup.Add(outputPath);

        _exporter.ExportToJson(results, outputPath, team1Actors, team2Actors);

        var jsonContent = File.ReadAllText(outputPath);
        jsonContent.Should().Contain("\"TeamConfigurations\"");
        jsonContent.Should().Contain("\"team1\"");
        jsonContent.Should().Contain("\"team2\"");
        jsonContent.Should().Contain("\"ActorType\": \"Model\"");
        jsonContent.Should().Contain("\"ModelName\": \"gen2\"");
        jsonContent.Should().Contain("\"ActorType\": \"Chaos\"");
    }

    [Fact]
    public void ExportToJson_WithNullTeamActors_UsesDefaultConfiguration()
    {
        var results = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 5,
            Team2Wins = 5,
            FailedGames = 0,
            TotalDeals = 50,
            TotalTricks = 250,
            TotalCallTrumpDecisions = 50,
            TotalDiscardCardDecisions = 25,
            TotalPlayCardDecisions = 250,
            ElapsedTime = TimeSpan.FromSeconds(30),
        };

        var outputPath = Path.Combine(_tempDirectory, "default-teams-results.json");
        _filesToCleanup.Add(outputPath);

        _exporter.ExportToJson(results, outputPath, null, null);

        var jsonContent = File.ReadAllText(outputPath);
        jsonContent.Should().Contain("\"ActorType\": \"Chaos\"");
    }

    [Fact]
    public void ExportToJson_ComputedThroughput_IncludedInOutput()
    {
        var results = new BatchGameResults
        {
            TotalGames = 100,
            Team1Wins = 55,
            Team2Wins = 45,
            FailedGames = 0,
            TotalDeals = 500,
            TotalTricks = 2500,
            TotalCallTrumpDecisions = 500,
            TotalDiscardCardDecisions = 250,
            TotalPlayCardDecisions = 2500,
            ElapsedTime = TimeSpan.FromSeconds(10),
        };

        var outputPath = Path.Combine(_tempDirectory, "throughput-results.json");
        _filesToCleanup.Add(outputPath);

        _exporter.ExportToJson(results, outputPath, null, null);

        var jsonContent = File.ReadAllText(outputPath);
        jsonContent.Should().Contain("\"Throughput\"");

        var exportData = JsonSerializer.Deserialize<BatchGameResultsExport>(jsonContent, JsonSerializationOptions.WithNaNHandling);
        exportData.Should().NotBeNull();
        exportData!.Throughput.Should().BeApproximately(10.0, 0.1);
    }

    [Fact]
    public void ExportToJson_SerializedJson_CanBeDeserialized()
    {
        var results = new BatchGameResults
        {
            TotalGames = 100,
            Team1Wins = 60,
            Team2Wins = 40,
            FailedGames = 0,
            TotalDeals = 500,
            TotalTricks = 2500,
            TotalCallTrumpDecisions = 500,
            TotalDiscardCardDecisions = 250,
            TotalPlayCardDecisions = 2500,
            ElapsedTime = TimeSpan.FromMinutes(2),
            PlayingDuration = TimeSpan.FromMinutes(1.5),
            PersistenceDuration = TimeSpan.FromSeconds(30),
            IdvSaveDuration = TimeSpan.FromSeconds(15),
        };

        var team1Actors = new[] { new Actor(ActorType.Model, "gen3", 0.0f) };
        var team2Actors = new[] { new Actor(ActorType.Chad) };

        var outputPath = Path.Combine(_tempDirectory, "deserialization-test.json");
        _filesToCleanup.Add(outputPath);

        _exporter.ExportToJson(results, outputPath, team1Actors, team2Actors);

        var jsonContent = File.ReadAllText(outputPath);
        var deserialized = JsonSerializer.Deserialize<BatchGameResultsExport>(jsonContent, JsonSerializationOptions.WithNaNHandling);

        deserialized.Should().NotBeNull();
        deserialized!.TotalGames.Should().Be(100);
        deserialized.Team1Wins.Should().Be(60);
        deserialized.Team2Wins.Should().Be(40);
        deserialized.Team1WinRate.Should().BeApproximately(0.6, 0.001);
        deserialized.Team2WinRate.Should().BeApproximately(0.4, 0.001);
        deserialized.TotalDeals.Should().Be(500);
        deserialized.TotalTricks.Should().Be(2500);
        deserialized.ElapsedTime.Should().Be(TimeSpan.FromMinutes(2));
        deserialized.PlayingDuration.Should().Be(TimeSpan.FromMinutes(1.5));
        deserialized.PersistenceDuration.Should().Be(TimeSpan.FromSeconds(30));
        deserialized.IdvSaveDuration.Should().Be(TimeSpan.FromSeconds(15));
        deserialized.TeamConfigurations.Should().ContainKey("team1");
        deserialized.TeamConfigurations.Should().ContainKey("team2");
        deserialized.TeamConfigurations["team1"].ActorType.Should().Be(ActorType.Model);
        deserialized.TeamConfigurations["team1"].ModelName.Should().Be("gen3");
        deserialized.TeamConfigurations["team2"].ActorType.Should().Be(ActorType.Chad);
        deserialized.GeneratedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ExportToJson_WithNullOutputPath_ThrowsArgumentException()
    {
        var results = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 5,
            Team2Wins = 5,
            FailedGames = 0,
            TotalDeals = 50,
            TotalTricks = 250,
            TotalCallTrumpDecisions = 50,
            TotalDiscardCardDecisions = 25,
            TotalPlayCardDecisions = 250,
            ElapsedTime = TimeSpan.FromSeconds(30),
        };

        var act = () => _exporter.ExportToJson(results, null!, null, null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Output path cannot be null or empty*");
    }

    [Fact]
    public void ExportToJson_WithEmptyOutputPath_ThrowsArgumentException()
    {
        var results = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 5,
            Team2Wins = 5,
            FailedGames = 0,
            TotalDeals = 50,
            TotalTricks = 250,
            TotalCallTrumpDecisions = 50,
            TotalDiscardCardDecisions = 25,
            TotalPlayCardDecisions = 250,
            ElapsedTime = TimeSpan.FromSeconds(30),
        };

        var act = () => _exporter.ExportToJson(results, string.Empty, null, null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Output path cannot be null or empty*");
    }
}
