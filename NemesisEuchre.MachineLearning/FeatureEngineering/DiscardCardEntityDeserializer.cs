using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public static class DiscardCardEntityDeserializer
{
    public static DiscardCardFeatureContext Deserialize(DiscardCardDecisionEntity entity)
    {
        var cardsInHand = JsonDeserializationHelper.DeserializeRelativeCards(entity.CardsInHandJson);
        var chosenCard = JsonDeserializationHelper.DeserializeRelativeCard(entity.ChosenCardJson);

        return new DiscardCardFeatureContext
        {
            CardsInHand = cardsInHand,
            ChosenCard = chosenCard,
        };
    }
}
