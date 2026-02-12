using NemesisEuchre.Console.Models.BehavioralTests;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Loading;

namespace NemesisEuchre.Console.Services.BehavioralTests;

public interface IModelBehavioralTest
{
    string Name { get; }

    string Description { get; }

    DecisionType DecisionType { get; }

    string AssertionDescription { get; }

    BehavioralTestResult Run(IPredictionEngineProvider engineProvider, string modelName);
}
