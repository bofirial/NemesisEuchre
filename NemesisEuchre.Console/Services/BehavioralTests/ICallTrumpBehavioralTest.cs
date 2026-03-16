using NemesisEuchre.Console.Models.BehavioralTests;
using NemesisEuchre.MachineLearning.Loading;

namespace NemesisEuchre.Console.Services.BehavioralTests;

public interface ICallTrumpBehavioralTest
{
    string Name { get; }

    string Description { get; }

    string AssertionDescription { get; }

    IReadOnlyList<BehavioralTestResult> Run(
        IPredictionEngineProvider engineProvider, string modelName, ICallTrumpBehavioralTestRunner runner);
}
