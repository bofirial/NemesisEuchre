using System.Diagnostics;

using NemesisEuchre.Console.Models.BehavioralTests;
using NemesisEuchre.MachineLearning.Loading;

namespace NemesisEuchre.Console.Services.BehavioralTests;

public interface IModelBehavioralTestRunner
{
    BehavioralTestSuiteResult RunTests(string modelName);
}

public class ModelBehavioralTestRunner(
    IEnumerable<ICallTrumpBehavioralTest> callTrumpTests,
    IEnumerable<IPlayCardBehavioralTest> playCardTests,
    IEnumerable<IDiscardCardBehavioralTest> discardCardTests,
    IEnumerable<ICallTrumpBehavioralTestRunner> callTrumpRunners,
    IEnumerable<IPlayCardBehavioralTestRunner> playCardRunners,
    IEnumerable<IDiscardCardBehavioralTestRunner> discardCardRunners,
    IPredictionEngineProvider engineProvider) : IModelBehavioralTestRunner
{
    public BehavioralTestSuiteResult RunTests(string modelName)
    {
        var sw = Stopwatch.StartNew();
        var results = new List<BehavioralTestResult>();

        foreach (var runner in callTrumpRunners)
        {
            foreach (var test in callTrumpTests.OrderBy(t => t.Name))
            {
                results.AddRange(test.Run(engineProvider, modelName, runner));
            }
        }

        foreach (var runner in discardCardRunners)
        {
            foreach (var test in discardCardTests.OrderBy(t => t.Name))
            {
                results.AddRange(test.Run(engineProvider, modelName, runner));
            }
        }

        foreach (var runner in playCardRunners)
        {
            foreach (var test in playCardTests.OrderBy(t => t.Name))
            {
                results.AddRange(test.Run(engineProvider, modelName, runner));
            }
        }

        results.Sort((a, b) =>
        {
            var typeCompare = a.DecisionType.CompareTo(b.DecisionType);
            return typeCompare != 0 ? typeCompare : StringComparer.Ordinal.Compare(a.TestName, b.TestName);
        });

        return new BehavioralTestSuiteResult(modelName, results, sw.Elapsed);
    }
}
