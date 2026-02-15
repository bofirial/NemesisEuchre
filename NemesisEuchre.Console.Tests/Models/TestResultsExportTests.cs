using FluentAssertions;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Models.BehavioralTests;
using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Tests.Models;

public class TestResultsExportTests
{
    [Fact]
    public void FromSuiteResult_WithAllPassedTests_ComputesCorrectStatistics()
    {
        var results = new List<BehavioralTestResult>
        {
            CreateTestResult("Test1", DecisionType.Play, passed: true),
            CreateTestResult("Test2", DecisionType.CallTrump, passed: true),
            CreateTestResult("Test3", DecisionType.Discard, passed: true),
        };
        var suiteResult = new BehavioralTestSuiteResult("gen2", results, TimeSpan.FromSeconds(10));

        var export = TestResultsExport.FromSuiteResult(suiteResult);

        export.ModelName.Should().Be("gen2");
        export.TotalTests.Should().Be(3);
        export.PassedTests.Should().Be(3);
        export.FailedTests.Should().Be(0);
        export.PassRate.Should().Be(1.0);
        export.Duration.Should().Be(TimeSpan.FromSeconds(10));
        export.GeneratedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void FromSuiteResult_WithAllFailedTests_ComputesCorrectStatistics()
    {
        var results = new List<BehavioralTestResult>
        {
            CreateTestResult("Test1", DecisionType.Play, passed: false),
            CreateTestResult("Test2", DecisionType.CallTrump, passed: false),
        };
        var suiteResult = new BehavioralTestSuiteResult("gen2", results, TimeSpan.FromSeconds(5));

        var export = TestResultsExport.FromSuiteResult(suiteResult);

        export.TotalTests.Should().Be(2);
        export.PassedTests.Should().Be(0);
        export.FailedTests.Should().Be(2);
        export.PassRate.Should().Be(0.0);
    }

    [Fact]
    public void FromSuiteResult_WithMixedResults_GroupsByDecisionType()
    {
        var results = new List<BehavioralTestResult>
        {
            CreateTestResult("Test1", DecisionType.Play, passed: true),
            CreateTestResult("Test2", DecisionType.Play, passed: false),
            CreateTestResult("Test3", DecisionType.CallTrump, passed: true),
            CreateTestResult("Test4", DecisionType.Discard, passed: true),
        };
        var suiteResult = new BehavioralTestSuiteResult("gen2", results, TimeSpan.FromSeconds(5));

        var export = TestResultsExport.FromSuiteResult(suiteResult);

        export.TestsByDecisionType.Should().HaveCount(3);
        export.TestsByDecisionType.Should().ContainKey(DecisionType.Play);
        export.TestsByDecisionType.Should().ContainKey(DecisionType.CallTrump);
        export.TestsByDecisionType.Should().ContainKey(DecisionType.Discard);

        export.TestsByDecisionType[DecisionType.Play].Total.Should().Be(2);
        export.TestsByDecisionType[DecisionType.Play].Passed.Should().Be(1);
        export.TestsByDecisionType[DecisionType.Play].Failed.Should().Be(1);
        export.TestsByDecisionType[DecisionType.Play].PassRate.Should().BeApproximately(0.5, 0.001);

        export.TestsByDecisionType[DecisionType.CallTrump].Total.Should().Be(1);
        export.TestsByDecisionType[DecisionType.CallTrump].Passed.Should().Be(1);
        export.TestsByDecisionType[DecisionType.CallTrump].PassRate.Should().Be(1.0);
    }

    [Fact]
    public void FromSuiteResult_WithEmptyResults_HandlesGracefully()
    {
        var results = new List<BehavioralTestResult>();
        var suiteResult = new BehavioralTestSuiteResult("gen2", results, TimeSpan.FromSeconds(1));

        var export = TestResultsExport.FromSuiteResult(suiteResult);

        export.TotalTests.Should().Be(0);
        export.PassedTests.Should().Be(0);
        export.FailedTests.Should().Be(0);
        export.PassRate.Should().Be(0.0);
        export.TestsByDecisionType.Should().BeEmpty();
        export.TestResults.Should().BeEmpty();
    }

    [Fact]
    public void FromSuiteResult_IncludesOptionScoresForAllResults()
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

        var export = TestResultsExport.FromSuiteResult(suiteResult);

        export.TestResults.Should().HaveCount(1);
        export.TestResults[0].OptionScores.Should().BeEquivalentTo(optionScores);
        export.TestResults[0].OptionScores["9♣"].Should().BeApproximately(0.8542f, 0.0001f);
    }

    [Fact]
    public void FromSuiteResult_PreservesFailureReasons()
    {
        var results = new List<BehavioralTestResult>
        {
            CreateTestResult("Test1", DecisionType.Play, passed: false, failureReason: "Expected trump card"),
            CreateTestResult("Test2", DecisionType.CallTrump, passed: true),
        };
        var suiteResult = new BehavioralTestSuiteResult("gen2", results, TimeSpan.FromSeconds(5));

        var export = TestResultsExport.FromSuiteResult(suiteResult);

        export.TestResults[0].FailureReason.Should().Be("Expected trump card");
        export.TestResults[1].FailureReason.Should().BeNull();
    }

    [Fact]
    public void FromSuiteResult_ComputesOverallPassRate()
    {
        var results = new List<BehavioralTestResult>
        {
            CreateTestResult("Test1", DecisionType.Play, passed: true),
            CreateTestResult("Test2", DecisionType.Play, passed: true),
            CreateTestResult("Test3", DecisionType.CallTrump, passed: false),
            CreateTestResult("Test4", DecisionType.Discard, passed: true),
        };
        var suiteResult = new BehavioralTestSuiteResult("gen2", results, TimeSpan.FromSeconds(5));

        var export = TestResultsExport.FromSuiteResult(suiteResult);

        export.PassRate.Should().BeApproximately(0.75, 0.001);
        export.PassedTests.Should().Be(3);
        export.FailedTests.Should().Be(1);
    }

    [Fact]
    public void FromSuiteResult_PreservesTestDetails()
    {
        var optionScores = new Dictionary<string, float>
        {
            ["Option1"] = 0.7f,
            ["Option2"] = 0.3f,
        };
        var results = new List<BehavioralTestResult>
        {
            new(
                "PartnerWinningTrickShouldNotPlayTrump",
                DecisionType.Play,
                true,
                "9♣",
                "Should choose non-trump",
                optionScores,
                null),
        };
        var suiteResult = new BehavioralTestSuiteResult("gen2", results, TimeSpan.FromSeconds(5));

        var export = TestResultsExport.FromSuiteResult(suiteResult);

        export.TestResults.Should().HaveCount(1);
        var detail = export.TestResults[0];
        detail.TestName.Should().Be("PartnerWinningTrickShouldNotPlayTrump");
        detail.DecisionType.Should().Be(DecisionType.Play);
        detail.Passed.Should().BeTrue();
        detail.ChosenOption.Should().Be("9♣");
        detail.ExpectedBehavior.Should().Be("Should choose non-trump");
        detail.OptionScores.Should().BeEquivalentTo(optionScores);
        detail.FailureReason.Should().BeNull();
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
