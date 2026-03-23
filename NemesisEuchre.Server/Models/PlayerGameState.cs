using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.Server.Models.Events;

namespace NemesisEuchre.Server.Models;

public record PlayerGameState
{
    public required string SessionName { get; init; }

    public required GameStatusViewModel GameStatus { get; init; }

    public required PlayerPosition MyPosition { get; init; }

    public required IReadOnlyDictionary<PlayerPosition, PlayerInfo> Players { get; init; }

    public required short Team1Score { get; init; }

    public required short Team2Score { get; init; }

    public required IReadOnlyList<ConnectedUserInfo> ConnectedUsers { get; init; }

    public required IReadOnlyDictionary<PlayerPosition, SeatInfo> Seats { get; init; }

    public DealState? CurrentDeal { get; init; }

    public IReadOnlyList<PlayerGameEvent> Events { get; init; } = [];
}
