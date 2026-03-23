namespace NemesisEuchre.Server.Models;

public class BotUpdateRequest : IBotFileRequest
{
    public string? NewBotName { get; init; }

    public IFormFile? CallTrumpZip { get; init; }

    public IFormFile? CallTrumpJson { get; init; }

    public IFormFile? AdvancedCallTrumpZip { get; init; }

    public IFormFile? AdvancedCallTrumpJson { get; init; }

    public IFormFile? DiscardCardZip { get; init; }

    public IFormFile? DiscardCardJson { get; init; }

    public IFormFile? AdvancedDiscardCardZip { get; init; }

    public IFormFile? AdvancedDiscardCardJson { get; init; }

    public IFormFile? PlayCardZip { get; init; }

    public IFormFile? PlayCardJson { get; init; }

    public IFormFile? SimplePlayCardZip { get; init; }

    public IFormFile? SimplePlayCardJson { get; init; }

    public IFormFile? AdvancedPlayCardZip { get; init; }

    public IFormFile? AdvancedPlayCardJson { get; init; }
}
