using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.MachineLearning.Loading;

public interface IModelFileProvider
{
    string EnsureModelFile(string modelsDirectory, string modelName, string decisionType);
}

public class LocalModelFileProvider : IModelFileProvider
{
    public string EnsureModelFile(string modelsDirectory, string modelName, string decisionType)
    {
        var normalizedDecisionType = decisionType.ToLowerInvariant();
        var fileName = $"{modelName}_{normalizedDecisionType}{FileExtensions.ModelZip}";
        return Path.Combine(modelsDirectory, fileName);
    }
}
