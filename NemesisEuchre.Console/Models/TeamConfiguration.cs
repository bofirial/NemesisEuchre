using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Models;

public record TeamConfiguration(
    ActorType ActorType,
    string? ModelName = null,
    float ExplorationTemperature = default)
{
    public static TeamConfiguration FromActor(Actor? actor)
    {
        return actor != null
            ? new TeamConfiguration(actor.ActorType, actor.ModelName, actor.ExplorationTemperature)
            : new TeamConfiguration(ActorType.Chaos);
    }
}
