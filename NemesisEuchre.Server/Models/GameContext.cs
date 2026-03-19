using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Server.Models;

public record GameContext
{
    public required int SessionId { get; init; }

    public required string SessionName { get; init; }

    public required IReadOnlyList<ActiveSessionMember> Members { get; init; }

    public IReadOnlyList<SeatInfo> Seats { get; init; } = [];

    public GameStatusViewModel Status { get; init; } = GameStatusViewModel.Lobby;

    public Game? ActiveGame { get; init; }
}
