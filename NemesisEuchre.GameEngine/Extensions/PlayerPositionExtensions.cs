using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Extensions;

public static class PlayerPositionExtensions
{
    /// <summary>
    /// Total number of player positions in a Euchre game.
    /// Used for relative position calculations.
    /// </summary>
    private const int TotalPlayerPositions = 4;

    public static Team GetTeam(this PlayerPosition position)
    {
        return position switch
        {
            PlayerPosition.North => Team.Team1,
            PlayerPosition.South => Team.Team1,
            PlayerPosition.East => Team.Team2,
            PlayerPosition.West => Team.Team2,
            _ => throw new ArgumentOutOfRangeException(nameof(position))
        };
    }

    public static PlayerPosition GetNextPosition(this PlayerPosition position)
    {
        return position switch
        {
            PlayerPosition.North => PlayerPosition.East,
            PlayerPosition.East => PlayerPosition.South,
            PlayerPosition.South => PlayerPosition.West,
            PlayerPosition.West => PlayerPosition.North,
            _ => throw new ArgumentOutOfRangeException(nameof(position))
        };
    }

    public static PlayerPosition GetPartnerPosition(this PlayerPosition position)
    {
        return position switch
        {
            PlayerPosition.North => PlayerPosition.South,
            PlayerPosition.South => PlayerPosition.North,
            PlayerPosition.East => PlayerPosition.West,
            PlayerPosition.West => PlayerPosition.East,
            _ => throw new ArgumentOutOfRangeException(nameof(position))
        };
    }

    public static RelativePlayerPosition ToRelativePosition(
        this PlayerPosition absolutePosition,
        PlayerPosition self)
    {
        var distance = (absolutePosition - self + TotalPlayerPositions) % TotalPlayerPositions;
        return distance switch
        {
            0 => RelativePlayerPosition.Self,
            1 => RelativePlayerPosition.LeftHandOpponent,
            2 => RelativePlayerPosition.Partner,
            3 => RelativePlayerPosition.RightHandOpponent,
            _ => throw new InvalidOperationException($"Impossible modulo {TotalPlayerPositions} state")
        };
    }

    public static PlayerPosition ToAbsolutePosition(
        this RelativePlayerPosition relativePosition,
        PlayerPosition self)
    {
        var offset = relativePosition switch
        {
            RelativePlayerPosition.Self => 0,
            RelativePlayerPosition.LeftHandOpponent => 1,
            RelativePlayerPosition.Partner => 2,
            RelativePlayerPosition.RightHandOpponent => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(relativePosition))
        };

        return (PlayerPosition)(((int)self + offset) % TotalPlayerPositions);
    }

    public static Player GetPlayerAtRelativePosition(
        this Dictionary<PlayerPosition, Player> players,
        PlayerPosition self,
        RelativePlayerPosition relativePosition)
    {
        var absolutePosition = relativePosition.ToAbsolutePosition(self);
        return players[absolutePosition];
    }
}
