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
        string modelName,
        string modelType,
        TrainingResult trainingResult,
        ModelMetadata metadata,
        object evaluationReport,
        CancellationToken cancellationToken = default)
        where TData : class, new();
}

public class ModelPersistenceService(
    ILogger<ModelPersistenceService> logger) : IModelPersistenceService
{
    public async Task SaveModelAsync<TData>(
        ITransformer model,
        MLContext mlContext,
        string modelsDirectory,
        string modelName,
        string modelType,
        TrainingResult trainingResult,
        ModelMetadata metadata,
        object evaluationReport,
        CancellationToken cancellationToken = default)
        where TData : class, new()
    {
        ValidateSaveModelParameters(model, modelsDirectory, modelName, trainingResult);

        EnsureDirectoryExists(modelsDirectory);

        var decisionType = modelType.ToLowerInvariant();

        var normalizedDecisionType = decisionType.ToLowerInvariant();
        var fileName = $"{modelName}_{normalizedDecisionType}.zip";
        var modelFilePath = Path.Combine(modelsDirectory, fileName);

        await SaveModelFileAsync<TData>(model, mlContext, modelFilePath, modelName, decisionType, cancellationToken);
        await SaveMetadataAsync(modelFilePath, metadata, cancellationToken);
        await SaveEvaluationReportAsync(modelFilePath, evaluationReport, cancellationToken);
    }

    private static void ValidateSaveModelParameters(
        ITransformer model,
        string modelsDirectory,
        string modelName,
        TrainingResult trainingResult)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelsDirectory);
        ArgumentNullException.ThrowIfNull(trainingResult);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelName);

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

    private Task SaveModelFileAsync<TData>(
        ITransformer model,
        MLContext mlContext,
        string modelPath,
        string modelName,
        string decisionType,
        CancellationToken cancellationToken)
        where TData : class, new()
    {
        var schema = mlContext.Data.LoadFromEnumerable([new TData()]).Schema;

        LoggerMessages.LogSavingModel(logger, modelName, decisionType);

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
