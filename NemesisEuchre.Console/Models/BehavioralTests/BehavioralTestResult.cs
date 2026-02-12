using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Models.BehavioralTests;

public record BehavioralTestResult(
    string TestName,
    DecisionType DecisionType,
    bool Passed,
    string ChosenOptionDisplay,
    string AssertionDescription,
    Dictionary<string, float> OptionScores,
    string? FailureReason);

public record BehavioralTestSuiteResult(
    string ModelName,
    IReadOnlyList<BehavioralTestResult> Results,
    TimeSpan Duration);
