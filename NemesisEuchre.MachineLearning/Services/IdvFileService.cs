using System.Text.Json;

using Microsoft.ML;

using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.Services;

public interface IIdvFileService
{
    void Save<T>(IEnumerable<T> data, string filePath)
        where T : class;

    IDataView Load(string filePath);

    IEnumerable<T> StreamFromBinary<T>(string filePath)
        where T : class, new();

    void SaveMetadata(IdvFileMetadata metadata, string metadataPath);

    IdvFileMetadata LoadMetadata(string metadataPath);
}

public class IdvFileService(MLContext mlContext) : IIdvFileService
{
    public void Save<T>(IEnumerable<T> data, string filePath)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var dataView = mlContext.Data.LoadFromEnumerable(data);

        using var stream = File.Create(filePath);
        mlContext.Data.SaveAsBinary(dataView, stream);
    }

    public IDataView Load(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        return mlContext.Data.LoadFromBinary(filePath);
    }

    public IEnumerable<T> StreamFromBinary<T>(string filePath)
        where T : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        return StreamFromBinaryCore<T>(filePath);
    }

    public void SaveMetadata(IdvFileMetadata metadata, string metadataPath)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentException.ThrowIfNullOrWhiteSpace(metadataPath);

        var json = JsonSerializer.Serialize(metadata, JsonSerializationOptions.WithNaNHandling);
        File.WriteAllText(metadataPath, json);
    }

    public IdvFileMetadata LoadMetadata(string metadataPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metadataPath);

        if (!File.Exists(metadataPath))
        {
            throw new FileNotFoundException($"IDV metadata file not found: {metadataPath}", metadataPath);
        }

        var json = File.ReadAllText(metadataPath);
        return JsonSerializer.Deserialize<IdvFileMetadata>(json, JsonSerializationOptions.WithNaNHandling)
            ?? throw new InvalidOperationException($"Failed to deserialize IDV metadata from: {metadataPath}");
    }

    private IEnumerable<T> StreamFromBinaryCore<T>(string filePath)
        where T : class, new()
    {
        var dataView = mlContext.Data.LoadFromBinary(filePath);
        try
        {
            foreach (var item in mlContext.Data.CreateEnumerable<T>(dataView, reuseRowObject: false))
            {
                yield return item;
            }
        }
        finally
        {
            (dataView as IDisposable)?.Dispose();
        }
    }
}
