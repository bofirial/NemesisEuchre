using System.Collections.Concurrent;

using Azure.Storage.Blobs;

using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Loading;

namespace NemesisEuchre.Server.Services;

public class AzureBlobModelFileProvider(
    IConfiguration configuration,
    ILogger<AzureBlobModelFileProvider> logger) : IModelFileProvider
{
    private readonly string _connectionString = configuration["AzureStorage:ConnectionString"] ?? string.Empty;
    private readonly string _containerName = configuration["AzureStorage:ContainerName"] ?? "models";
    private readonly ConcurrentDictionary<string, Lazy<List<string>>> _downloadCache = new(StringComparer.OrdinalIgnoreCase);

    public string EnsureModelFile(string modelsDirectory, string modelName, string decisionType)
    {
        var localFiles = _downloadCache.GetOrAdd(
            modelName,
            key => new Lazy<List<string>>(() => DownloadAllBotModels(modelsDirectory, key))).Value;

        var suffix = $"_{decisionType}{FileExtensions.ModelZip}";
        var match = localFiles.FirstOrDefault(f => f.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

        if (match is not null)
        {
            return match;
        }

        var availableFiles = localFiles.Count > 0
            ? string.Join(", ", localFiles.Select(Path.GetFileName))
            : "(none)";

        throw new FileNotFoundException(
            $"No model file matching '*{suffix}' found for bot '{modelName}'. Available files: {availableFiles}");
    }

    private List<string> DownloadAllBotModels(string modelsDirectory, string modelName)
    {
        var botDirectory = Path.Combine(modelsDirectory, modelName);
        Directory.CreateDirectory(botDirectory);

        var container = new BlobServiceClient(_connectionString).GetBlobContainerClient(_containerName);
        var prefix = $"{modelName}/";
        var downloadedFiles = new List<string>();

        try
        {
            foreach (var blobItem in container.GetBlobs(prefix: prefix))
            {
                if (!blobItem.Name.EndsWith(FileExtensions.ModelZip, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var fileName = Path.GetFileName(blobItem.Name);
                var localPath = Path.Combine(botDirectory, fileName);

                if (File.Exists(localPath))
                {
                    downloadedFiles.Add(localPath);
                    continue;
                }

                var tempPath = localPath + ".downloading";
                var blobClient = container.GetBlobClient(blobItem.Name);
                blobClient.DownloadTo(tempPath);
                File.Move(tempPath, localPath, overwrite: true);

                downloadedFiles.Add(localPath);
            }

            LoggerMessages.LogBotModelsDownloadedFromBlob(logger, downloadedFiles.Count, modelName);
        }
        catch (Exception ex)
        {
            LoggerMessages.LogModelDownloadFromBlobFailed(logger, prefix, ex);
            throw;
        }

        return downloadedFiles;
    }
}
