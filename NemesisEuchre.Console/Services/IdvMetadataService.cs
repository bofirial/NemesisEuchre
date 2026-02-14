using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.Console.Services;

public interface IIdvMetadataService
{
    void SaveMetadataWithVerification(string idvFilePath, IdvFileMetadata metadata);
}

public sealed class IdvMetadataService(
    IIdvFileService idvFileService,
    ILogger<IdvMetadataService> logger) : IIdvMetadataService
{
    public void SaveMetadataWithVerification(string idvFilePath, IdvFileMetadata metadata)
    {
        var metadataPath = idvFilePath + FileExtensions.IdvMetadataSuffix;

        idvFileService.SaveMetadata(metadata, metadataPath);
        LoggerMessages.LogIdvMetadataSaved(logger, metadataPath);

        var readBack = idvFileService.LoadMetadata(metadataPath);
        if (readBack.RowCount != metadata.RowCount)
        {
            LoggerMessages.LogIdvMetadataVerificationFailed(logger, metadataPath);
            throw new InvalidOperationException(
                $"IDV metadata verification failed for {metadataPath}: expected {metadata.RowCount} rows but read back {readBack.RowCount}");
        }
    }
}
