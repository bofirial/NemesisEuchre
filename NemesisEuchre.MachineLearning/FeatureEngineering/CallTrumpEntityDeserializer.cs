using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public static class CallTrumpEntityDeserializer
{
    public static CallTrumpFeatureContext Deserialize(CallTrumpDecisionEntity entity)
    {
        var cards = JsonDeserializationHelper.DeserializeCards(entity.CardsInHandJson);
        var upCard = JsonDeserializationHelper.DeserializeCard(entity.UpCardJson);
        var validDecisions = JsonDeserializationHelper.DeserializeCallTrumpDecisions(entity.ValidDecisionsJson);
        var chosenDecision = JsonDeserializationHelper.DeserializeCallTrumpDecision(entity.ChosenDecisionJson);

        return new CallTrumpFeatureContext
        {
            Cards = cards,
            UpCard = upCard,
            ValidDecisions = validDecisions,
            ChosenDecision = chosenDecision,
        };
    }
}
