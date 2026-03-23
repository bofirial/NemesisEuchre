namespace NemesisEuchre.Server.Models;

public interface IBotFileRequest
{
    IFormFile? CallTrumpZip { get; }

    IFormFile? CallTrumpJson { get; }

    IFormFile? AdvancedCallTrumpZip { get; }

    IFormFile? AdvancedCallTrumpJson { get; }

    IFormFile? DiscardCardZip { get; }

    IFormFile? DiscardCardJson { get; }

    IFormFile? AdvancedDiscardCardZip { get; }

    IFormFile? AdvancedDiscardCardJson { get; }

    IFormFile? PlayCardZip { get; }

    IFormFile? PlayCardJson { get; }

    IFormFile? SimplePlayCardZip { get; }

    IFormFile? SimplePlayCardJson { get; }

    IFormFile? AdvancedPlayCardZip { get; }

    IFormFile? AdvancedPlayCardJson { get; }
}
