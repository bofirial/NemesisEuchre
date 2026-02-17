namespace NemesisEuchre.Foundation.Constants;

public record Actor(ActorType ActorType, Dictionary<string, string>? ModelNames = null, float ExplorationTemperature = default, DecisionType ExplorationDecisionType = DecisionType.All)
{
    public string? ModelName => ModelNames?.GetValueOrDefault("default");

    public static Actor WithModel(ActorType actorType, string modelName, float explorationTemperature = default, DecisionType explorationDecisionType = DecisionType.All)
    {
        return new Actor(
            actorType,
            new Dictionary<string, string> { ["default"] = modelName },
            explorationTemperature,
            explorationDecisionType);
    }

    public static Actor WithModels(
        ActorType actorType,
        string? playCardModel = null,
        string? callTrumpModel = null,
        string? discardCardModel = null,
        string? defaultModel = null,
        float explorationTemperature = default,
        DecisionType explorationDecisionType = DecisionType.All)
    {
        var modelNames = new Dictionary<string, string>();

        if (playCardModel != null)
        {
            modelNames["PlayCard"] = playCardModel;
        }

        if (callTrumpModel != null)
        {
            modelNames["CallTrump"] = callTrumpModel;
        }

        if (discardCardModel != null)
        {
            modelNames["DiscardCard"] = discardCardModel;
        }

        if (defaultModel != null)
        {
            modelNames["default"] = defaultModel;
        }

        return new Actor(actorType, modelNames, explorationTemperature, explorationDecisionType);
    }

    public string? GetModelName(string decisionType)
    {
        if (ModelNames == null)
        {
            return null;
        }

        return ModelNames.GetValueOrDefault(decisionType) ?? ModelNames.GetValueOrDefault("default");
    }
}
