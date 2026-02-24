namespace NemesisEuchre.Server.Services;

public interface IModelStorageService
{
    Task UploadModelAsync(string modelName, IReadOnlyList<IFormFile> files, CancellationToken cancellationToken);
}
