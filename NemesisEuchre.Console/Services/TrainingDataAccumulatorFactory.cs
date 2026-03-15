using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.Console.Services;

public interface ITrainingDataAccumulatorFactory
{
    ITrainingDataAccumulator Create();
}

public class TrainingDataAccumulatorFactory(
    IIdvFileService idvFileService,
    IIdvChunkMerger merger,
    IIdvMetadataService metadataService,
    IOptions<PersistenceOptions> persistenceOptions,
    ILoggerFactory loggerFactory) : ITrainingDataAccumulatorFactory
{
    public ITrainingDataAccumulator Create()
    {
        var buffer = new TrainingDataBuffer(
            idvFileService,
            loggerFactory.CreateLogger<TrainingDataBuffer>());

        return new TrainingDataAccumulator(
            buffer,
            merger,
            metadataService,
            persistenceOptions,
            loggerFactory.CreateLogger<TrainingDataAccumulator>());
    }
}
