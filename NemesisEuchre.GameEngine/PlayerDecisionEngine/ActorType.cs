namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public static class ActorType
{
    public const string User = "User";
    public const string Chaos = "Chaos";
    public const string Chad = "Chad";
    public const string Beta = "Beta";

    public static bool IsValid(string? actorType)
    {
        if (string.IsNullOrWhiteSpace(actorType))
        {
            return false;
        }

        if (actorType is User or Chaos or Chad or Beta)
        {
            return true;
        }

        return actorType.StartsWith($"{Chad}_Gen", StringComparison.Ordinal) ||
               actorType.StartsWith($"{Beta}_Gen", StringComparison.Ordinal);
    }

    public static string GetGenerationName(string baseActorType, int generation)
    {
        if (generation <= 0)
        {
            throw new ArgumentException("Generation must be greater than 0", nameof(generation));
        }

        if (baseActorType is not Chad and not Beta)
        {
            throw new ArgumentException($"Only {Chad} and {Beta} support generations", nameof(baseActorType));
        }

        return $"{baseActorType}_Gen{generation}";
    }
}
