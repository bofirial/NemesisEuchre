using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.GameEngine;

public static class PlayerPositionExtensions
{
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
}
