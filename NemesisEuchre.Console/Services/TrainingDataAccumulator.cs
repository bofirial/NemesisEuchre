using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.Console.Models;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.Foundation;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.Console.Services;

public interface ITrainingDataAccumulator
{
    void Add(TrainingDataBatch batch);

    void Save(string generationName);
}

public class TrainingDataAccumulator(
    IIdvFileService idvFileService,
    IOptions<PersistenceOptions> persistenceOptions,
    ILogger<TrainingDataAccumulator> logger) : ITrainingDataAccumulator
{
    private readonly List<PlayCardTrainingData> _playCardData = [];
    private readonly List<CallTrumpTrainingData> _callTrumpData = [];
    private readonly List<DiscardCardTrainingData> _discardCardData = [];

    public void Add(TrainingDataBatch batch)
    {
        _playCardData.AddRange(batch.PlayCardData);
        _callTrumpData.AddRange(batch.CallTrumpData);
        _discardCardData.AddRange(batch.DiscardCardData);
    }

    public void Save(string generationName)
    {
        var outputPath = persistenceOptions.Value.IdvOutputPath;

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        SaveIdvFile(_playCardData, Path.Combine(outputPath, $"{generationName}_PlayCard.idv"));
        SaveIdvFile(_callTrumpData, Path.Combine(outputPath, $"{generationName}_CallTrump.idv"));
        SaveIdvFile(_discardCardData, Path.Combine(outputPath, $"{generationName}_DiscardCard.idv"));
    }

    private void SaveIdvFile<T>(List<T> data, string filePath)
        where T : class
    {
        LoggerMessages.LogIdvFileSaving(logger, filePath, data.Count);
        idvFileService.Save(data, filePath);
        LoggerMessages.LogIdvFileSaved(logger, filePath, data.Count);
    }
}
