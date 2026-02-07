using System.Text.Json;

using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public static class JsonDeserializationHelper
{
    public static Card[] DeserializeCards(string json)
    {
        return JsonSerializer.Deserialize<Card[]>(json, JsonSerializationOptions.Default)!;
    }

    public static Card DeserializeCard(string json)
    {
        return JsonSerializer.Deserialize<Card>(json, JsonSerializationOptions.Default)!;
    }

    public static RelativeCard[] DeserializeRelativeCards(string json)
    {
        return JsonSerializer.Deserialize<RelativeCard[]>(json, JsonSerializationOptions.Default)!;
    }

    public static RelativeCard DeserializeRelativeCard(string json)
    {
        return JsonSerializer.Deserialize<RelativeCard>(json, JsonSerializationOptions.Default)!;
    }

    public static Dictionary<RelativePlayerPosition, RelativeCard> DeserializePlayedCards(string json)
    {
        return JsonSerializer.Deserialize<Dictionary<RelativePlayerPosition, RelativeCard>>(json, JsonSerializationOptions.Default)!;
    }

    public static CallTrumpDecision[] DeserializeCallTrumpDecisions(string json)
    {
        return JsonSerializer.Deserialize<CallTrumpDecision[]>(json, JsonSerializationOptions.Default)!;
    }

    public static CallTrumpDecision DeserializeCallTrumpDecision(string json)
    {
        return JsonSerializer.Deserialize<CallTrumpDecision>(json, JsonSerializationOptions.Default);
    }

    public static RelativePlayerSuitVoid[] DeserializeKnownPlayerVoids(string json)
    {
        return JsonSerializer.Deserialize<RelativePlayerSuitVoid[]>(json, JsonSerializationOptions.Default)!;
    }
}
