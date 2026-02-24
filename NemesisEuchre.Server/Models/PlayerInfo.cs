using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Server.Models;

public record PlayerInfo
{
    public required PlayerPosition Position { get; init; }

    public required string Name { get; init; }

    public required Team Team { get; init; }
}
