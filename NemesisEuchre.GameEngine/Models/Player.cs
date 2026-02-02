using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;

namespace NemesisEuchre.GameEngine.Models;

public class Player
{
    public PlayerPosition Position { get; set; }

    public Team Team => Position.GetTeam();

    public ActorType? ActorType { get; set; }
}
