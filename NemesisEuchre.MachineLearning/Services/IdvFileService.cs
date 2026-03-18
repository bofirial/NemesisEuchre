using System.Text.Json;

using Microsoft.ML;
using Microsoft.ML.Data;

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

    IEnumerable<T> StreamFromBinaryDetached<T>(string filePath)
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

        (dataView as IDisposable)?.Dispose();
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

    public IEnumerable<T> StreamFromBinaryDetached<T>(string filePath)
        where T : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        return StreamFromBinaryDetachedCore<T>(filePath);
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
            foreach (var row in mlContext.Data.CreateEnumerable<T>(dataView, reuseRowObject: false))
            {
                yield return row;
            }
        }
        finally
        {
            (dataView as IDisposable)?.Dispose();
        }
    }

    private IEnumerable<T> StreamFromBinaryDetachedCore<T>(string filePath)
        where T : class, new()
    {
        var fileBytes = File.ReadAllBytes(filePath);
        var dataView = mlContext.Data.LoadFromBinary(new InMemoryStreamSource(fileBytes));
        try
        {
            foreach (var row in mlContext.Data.CreateEnumerable<T>(dataView, reuseRowObject: false))
            {
                yield return row;
            }
        }
        finally
        {
            (dataView as IDisposable)?.Dispose();
        }
    }

    private sealed class InMemoryStreamSource(byte[] data) : IMultiStreamSource
    {
        public int Count => 1;

        public string GetPathOrNull(int index)
        {
            return null!;
        }

        public Stream Open(int index)
        {
            return new MemoryStream(data, writable: false);
        }

        public TextReader OpenTextReader(int index)
        {
            return new StreamReader(Open(index));
        }
    }
}
