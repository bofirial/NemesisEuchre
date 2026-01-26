using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Models;

public class Player
{
    public PlayerPosition Position { get; set; }

    public Team Team => Position.GetTeam();

    public ActorType? ActorType { get; set; }
}
