using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace NemesisEuchre.Server.Services;

public interface IBotStorageService
{
    Task<IReadOnlyList<string>> ListBotNamesAsync(CancellationToken cancellationToken);

    Task UploadBotAsync(string botName, IReadOnlyList<IFormFile> files, CancellationToken cancellationToken);

    Task UpdateBotAsync(string botName, string? newBotName, IReadOnlyList<IFormFile> files, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetBotFilesAsync(string botName, CancellationToken cancellationToken);

    Task DeleteBotAsync(string botName, CancellationToken cancellationToken);
}

public class BotStorageService(IConfiguration configuration) : IBotStorageService
{
    private readonly string _connectionString = configuration["AzureStorage:ConnectionString"] ?? string.Empty;
    private readonly string _containerName = configuration["AzureStorage:ContainerName"] ?? "models";

    public async Task<IReadOnlyList<string>> ListBotNamesAsync(CancellationToken cancellationToken)
    {
        var container = GetContainerClient();
        var botNames = new List<string>();

        await foreach (var item in container.GetBlobsByHierarchyAsync(delimiter: "/", cancellationToken: cancellationToken))
        {
            if (item.IsPrefix)
            {
                botNames.Add(item.Prefix.TrimEnd('/'));
            }
        }

        return botNames;
    }

    public async Task UploadBotAsync(string botName, IReadOnlyList<IFormFile> files, CancellationToken cancellationToken)
    {
        var container = GetContainerClient();
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        foreach (var file in files)
        {
            var blobClient = container.GetBlobClient($"{botName}/{file.FileName}");
            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: cancellationToken);
        }
    }

    public async Task UpdateBotAsync(string botName, string? newBotName, IReadOnlyList<IFormFile> files, CancellationToken cancellationToken)
    {
        var container = GetContainerClient();
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        foreach (var file in files)
        {
            var blobClient = container.GetBlobClient($"{botName}/{file.FileName}");
            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: cancellationToken);
        }

        if (newBotName is not null && !string.Equals(newBotName, botName, StringComparison.Ordinal))
        {
            await RenamePrefixAsync(container, botName, newBotName, cancellationToken);
        }
    }

    public async Task<IReadOnlyList<string>> GetBotFilesAsync(string botName, CancellationToken cancellationToken)
    {
        var container = GetContainerClient();
        var prefix = $"{botName}/";
        var fileNames = new List<string>();

        await foreach (var blob in container.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
        {
            fileNames.Add(blob.Name[prefix.Length..]);
        }

        return fileNames;
    }

    public async Task DeleteBotAsync(string botName, CancellationToken cancellationToken)
    {
        var container = GetContainerClient();

        await foreach (var blob in container.GetBlobsAsync(prefix: $"{botName}/", cancellationToken: cancellationToken))
        {
            await container.GetBlobClient(blob.Name).DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
    }

    private static async Task RenamePrefixAsync(
        BlobContainerClient container,
        string oldPrefix,
        string newPrefix,
        CancellationToken cancellationToken)
    {
        await foreach (var blob in container.GetBlobsAsync(prefix: $"{oldPrefix}/", cancellationToken: cancellationToken))
        {
            var newBlobName = newPrefix + blob.Name[oldPrefix.Length..];
            var sourceClient = container.GetBlobClient(blob.Name);
            var destClient = container.GetBlobClient(newBlobName);

            var sasUri = sourceClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(5));
            var copyOperation = await destClient.StartCopyFromUriAsync(sasUri, cancellationToken: cancellationToken);
            await copyOperation.WaitForCompletionAsync(cancellationToken);
            await sourceClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
    }

    private BlobContainerClient GetContainerClient()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("AzureStorage:ConnectionString is not configured.");
        }

        return new BlobServiceClient(_connectionString).GetBlobContainerClient(_containerName);
    }
}
