using System.Diagnostics;

using NemesisEuchre.Console.Models.BehavioralTests;
using NemesisEuchre.MachineLearning.Loading;

namespace NemesisEuchre.Console.Services.BehavioralTests;

public interface IModelBehavioralTestRunner
{
    BehavioralTestSuiteResult RunTests(string modelName);
}

public class ModelBehavioralTestRunner(
    IEnumerable<IModelBehavioralTest> tests,
    IPredictionEngineProvider engineProvider) : IModelBehavioralTestRunner
{
    public BehavioralTestSuiteResult RunTests(string modelName)
    {
        var sw = Stopwatch.StartNew();

        var results = tests
            .OrderBy(t => t.DecisionType)
            .ThenBy(t => t.Name)
            .SelectMany(t => t.Run(engineProvider, modelName))
            .ToList();

        return new BehavioralTestSuiteResult(modelName, results, sw.Elapsed);
    }
}
