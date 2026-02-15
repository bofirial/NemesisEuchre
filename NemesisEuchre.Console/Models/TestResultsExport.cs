using NemesisEuchre.Console.Models.BehavioralTests;
using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Models;

public record TestResultsExport(
    string ModelName,
    int TotalTests,
    int PassedTests,
    int FailedTests,
    double PassRate,
    TimeSpan Duration,
    DateTime GeneratedAtUtc,
    Dictionary<DecisionType, TestCategorySummary> TestsByDecisionType,
    IReadOnlyList<TestResultDetail> TestResults)
{
    public static TestResultsExport FromSuiteResult(BehavioralTestSuiteResult suiteResult)
    {
        var totalTests = suiteResult.Results.Count;
        var passedTests = suiteResult.Results.Count(r => r.Passed);
        var failedTests = totalTests - passedTests;
        var passRate = totalTests > 0 ? (double)passedTests / totalTests : 0.0;

        var testsByDecisionType = suiteResult.Results
            .GroupBy(r => r.DecisionType)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var total = g.Count();
                    var passed = g.Count(r => r.Passed);
                    var failed = total - passed;
                    var rate = total > 0 ? (double)passed / total : 0.0;
                    return new TestCategorySummary(total, passed, failed, rate);
                });

        var testResults = suiteResult.Results
            .Select(r => new TestResultDetail(
                r.TestName,
                r.DecisionType,
                r.Passed,
                r.ChosenOptionDisplay,
                r.AssertionDescription,
                r.OptionScores,
                r.FailureReason))
            .ToList();

        return new TestResultsExport(
            ModelName: suiteResult.ModelName,
            TotalTests: totalTests,
            PassedTests: passedTests,
            FailedTests: failedTests,
            PassRate: passRate,
            Duration: suiteResult.Duration,
            GeneratedAtUtc: DateTime.UtcNow,
            TestsByDecisionType: testsByDecisionType,
            TestResults: testResults);
    }
}

public record TestCategorySummary(
    int Total,
    int Passed,
    int Failed,
    double PassRate);

public record TestResultDetail(
    string TestName,
    DecisionType DecisionType,
    bool Passed,
    string ChosenOption,
    string ExpectedBehavior,
    Dictionary<string, float> OptionScores,
    string? FailureReason);
