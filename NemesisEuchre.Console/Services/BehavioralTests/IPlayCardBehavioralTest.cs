using NemesisEuchre.Console.Models.BehavioralTests;
using NemesisEuchre.MachineLearning.Loading;

namespace NemesisEuchre.Console.Services.BehavioralTests;

public interface IPlayCardBehavioralTest
{
    string Name { get; }

    string Description { get; }

    string AssertionDescription { get; }

    IReadOnlyList<BehavioralTestResult> Run(
        IPredictionEngineProvider engineProvider, string modelName, IPlayCardBehavioralTestRunner runner);
}
