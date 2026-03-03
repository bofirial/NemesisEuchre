using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Server.Models;

public record SeatInfo
{
    public required PlayerPosition Position { get; init; }

    public string? GitHubLogin { get; init; }
}
