using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Models;

public record TeamConfiguration(
    ActorType ActorType,
    Dictionary<string, string>? ModelNames = null,
    float ExplorationTemperature = default,
    int SimulationCount = 0,
    bool SkipSimCallTrump = false,
    bool SkipSimDiscard = false,
    bool SkipSimPlayCard = false)
{
    public string? ModelName => ModelNames?.GetValueOrDefault("default");

    public static TeamConfiguration FromActor(Actor? actor)
    {
        return actor != null
            ? new TeamConfiguration(
                actor.ActorType,
                actor.ModelNames,
                actor.ExplorationTemperature,
                actor.SimulationCount,
                actor.SkipSimCallTrump,
                actor.SkipSimDiscard,
                actor.SkipSimPlayCard)
            : new TeamConfiguration(ActorType.Chaos);
    }
}
