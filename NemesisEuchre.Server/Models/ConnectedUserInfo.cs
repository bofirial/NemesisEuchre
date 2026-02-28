namespace NemesisEuchre.Server.Models;

public record ConnectedUserInfo
{
    public required string GitHubLogin { get; init; }

    public required bool IsSessionLeader { get; init; }
}
