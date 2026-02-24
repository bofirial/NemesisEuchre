using Azure.Storage.Blobs;

namespace NemesisEuchre.Server.Services;

public class ModelStorageService(IConfiguration configuration) : IModelStorageService
{
    private readonly string _connectionString = configuration["AzureStorage:ConnectionString"] ?? string.Empty;
    private readonly string _containerName = configuration["AzureStorage:ContainerName"] ?? "models";

    public async Task UploadModelAsync(string modelName, IReadOnlyList<IFormFile> files, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("AzureStorage:ConnectionString is not configured.");
        }

        var serviceClient = new BlobServiceClient(_connectionString);
        var containerClient = serviceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        foreach (var file in files)
        {
            var blobName = $"{modelName}/{file.FileName}";
            var blobClient = containerClient.GetBlobClient(blobName);
            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: cancellationToken);
        }
    }
}
