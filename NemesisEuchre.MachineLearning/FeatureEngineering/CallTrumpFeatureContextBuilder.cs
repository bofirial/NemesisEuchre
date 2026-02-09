using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public static class CallTrumpFeatureContextBuilder
{
    public static CallTrumpFeatureContext Build(CallTrumpDecisionEntity entity)
    {
        var cards = entity.CardsInHand
            .OrderBy(c => c.SortOrder)
            .Select(c => CardIdHelper.ToCard(c.CardId))
            .ToArray();

        var upCard = CardIdHelper.ToCard(entity.UpCardId);

        var validDecisions = entity.ValidDecisions
            .Select(d => (CallTrumpDecision)d.CallTrumpDecisionValueId)
            .ToArray();

        var chosenDecision = (CallTrumpDecision)entity.ChosenDecisionValueId;

        return new CallTrumpFeatureContext
        {
            Cards = cards,
            UpCard = upCard,
            ValidDecisions = validDecisions,
            ChosenDecision = chosenDecision,
        };
    }
}
