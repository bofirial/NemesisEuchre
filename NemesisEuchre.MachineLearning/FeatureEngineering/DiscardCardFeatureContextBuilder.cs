using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public static class DiscardCardFeatureContextBuilder
{
    public static DiscardCardFeatureContext Build(DiscardCardDecisionEntity entity)
    {
        var cardsInHand = entity.CardsInHand
            .OrderBy(c => c.SortOrder)
            .Select(c => CardIdHelper.ToRelativeCard(c.RelativeCardId))
            .ToArray();

        var chosenCard = CardIdHelper.ToRelativeCard(entity.ChosenRelativeCardId);

        return new DiscardCardFeatureContext
        {
            CardsInHand = cardsInHand,
            ChosenCard = chosenCard,
        };
    }
}
