using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Models;

public record TeamConfiguration(
    ActorType ActorType,
    Dictionary<string, string>? ModelNames = null,
    float ExplorationTemperature = default)
{
    public string? ModelName => ModelNames?.GetValueOrDefault("default");

    public static TeamConfiguration FromActor(Actor? actor)
    {
        return actor != null
            ? new TeamConfiguration(actor.ActorType, actor.ModelNames, actor.ExplorationTemperature)
            : new TeamConfiguration(ActorType.Chaos);
    }
}
