using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.Foundation;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.Services;

public interface IModelPersistenceService
{
    Task SaveModelAsync<TData>(
        ITransformer model,
        MLContext mlContext,
        string modelsDirectory,
        int generation,
        string modelType,
        TrainingResult trainingResult,
        ModelMetadata metadata,
        object evaluationReport,
        CancellationToken cancellationToken = default)
        where TData : class, new();
}

public class ModelPersistenceService(
    IModelVersionManager versionManager,
    ILogger<ModelPersistenceService> logger) : IModelPersistenceService
{
    public async Task SaveModelAsync<TData>(
        ITransformer model,
        MLContext mlContext,
        string modelsDirectory,
        int generation,
        string modelType,
        TrainingResult trainingResult,
        ModelMetadata metadata,
        object evaluationReport,
        CancellationToken cancellationToken = default)
        where TData : class, new()
    {
        ValidateSaveModelParameters(model, modelsDirectory, generation, trainingResult);

        EnsureDirectoryExists(modelsDirectory);

        var decisionType = modelType.ToLowerInvariant();
        var version = DetermineNextVersion(modelsDirectory, generation, decisionType);
        var modelPath = versionManager.GetModelPath(modelsDirectory, generation, decisionType, version);

        await SaveModelFileAsync<TData>(model, mlContext, modelPath, generation, decisionType, version, cancellationToken);
        await SaveMetadataAsync(modelPath, metadata, cancellationToken);
        await SaveEvaluationReportAsync(modelPath, evaluationReport, cancellationToken);
    }

    private static void ValidateSaveModelParameters(
        ITransformer model,
        string modelsDirectory,
        int generation,
        TrainingResult trainingResult)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelsDirectory);
        ArgumentNullException.ThrowIfNull(trainingResult);

        if (generation < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(generation), "Generation must be at least 1");
        }

        if (model == null)
        {
            throw new InvalidOperationException("No trained model to save. Call TrainAsync first.");
        }
    }

    private static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private int DetermineNextVersion(string modelsDirectory, int generation, string decisionType)
    {
        LoggerMessages.LogDeterminingNextVersion(logger, generation, decisionType);
        var version = versionManager.GetNextVersion(modelsDirectory, generation, decisionType);
        LoggerMessages.LogExistingVersionsFound(logger, version - 1, generation, decisionType);
        return version;
    }

    private Task SaveModelFileAsync<TData>(
        ITransformer model,
        MLContext mlContext,
        string modelPath,
        int generation,
        string decisionType,
        int version,
        CancellationToken cancellationToken)
        where TData : class, new()
    {
        var schema = mlContext.Data.LoadFromEnumerable([new TData()]).Schema;

        LoggerMessages.LogSavingModelWithVersion(logger, generation, decisionType, version);

        return Task.Run(
            () =>
                {
                    mlContext.Model.Save(model, schema, modelPath);
                    LoggerMessages.LogModelSaved(logger, modelPath);
                },
            cancellationToken);
    }

    private async Task SaveMetadataAsync(
        string modelPath,
        ModelMetadata metadata,
        CancellationToken cancellationToken)
    {
        var metadataPath = Path.ChangeExtension(modelPath, ".json");
        var json = JsonSerializer.Serialize(metadata, JsonSerializationOptions.Default);

        await File.WriteAllTextAsync(metadataPath, json, cancellationToken);
        LoggerMessages.LogMetadataSaved(logger, metadataPath);
    }

    private async Task SaveEvaluationReportAsync(
        string modelPath,
        object evaluationReport,
        CancellationToken cancellationToken)
    {
        var evaluationPath = Path.ChangeExtension(modelPath, ".evaluation.json");
        var reportJson = JsonSerializer.Serialize(evaluationReport, JsonSerializationOptions.WithNaNHandling);

        await File.WriteAllTextAsync(evaluationPath, reportJson, cancellationToken);
        LoggerMessages.LogEvaluationReportSaved(logger, evaluationPath);
    }
}
