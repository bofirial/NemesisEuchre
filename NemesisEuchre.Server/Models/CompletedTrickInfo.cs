using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Server.Models;

public record CompletedTrickInfo
{
    public required short TrickNumber { get; init; }

    public required PlayerPosition LeadPosition { get; init; }

    public required IReadOnlyList<PlayedCard> CardsPlayed { get; init; }

    public required PlayerPosition WinningPosition { get; init; }

    public required Team WinningTeam { get; init; }
}
