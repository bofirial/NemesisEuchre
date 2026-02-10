using Microsoft.ML;

namespace NemesisEuchre.MachineLearning.Services;

public interface IIdvFileService
{
    void Save<T>(IEnumerable<T> data, string filePath)
        where T : class;

    IDataView Load(string filePath);
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
}
