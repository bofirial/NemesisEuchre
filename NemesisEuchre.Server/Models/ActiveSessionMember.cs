using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.Server.Models;

public record ActiveSessionMember
{
    public required GameSessionUserEntity Membership { get; init; }

    public required IReadOnlyList<string> ConnectionIds { get; init; }
}
