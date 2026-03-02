namespace NemesisEuchre.Server.Models;

public interface IBotFileRequest
{
    IFormFile? CallTrumpZip { get; }

    IFormFile? CallTrumpJson { get; }

    IFormFile? DiscardCardZip { get; }

    IFormFile? DiscardCardJson { get; }

    IFormFile? PlayCardZip { get; }

    IFormFile? PlayCardJson { get; }
}
