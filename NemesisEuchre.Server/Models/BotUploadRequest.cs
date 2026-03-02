namespace NemesisEuchre.Server.Models;

public class BotUploadRequest : IBotFileRequest
{
    public string BotName { get; init; } = string.Empty;

    public IFormFile? CallTrumpZip { get; init; }

    public IFormFile? CallTrumpJson { get; init; }

    public IFormFile? DiscardCardZip { get; init; }

    public IFormFile? DiscardCardJson { get; init; }

    public IFormFile? PlayCardZip { get; init; }

    public IFormFile? PlayCardJson { get; init; }
}
