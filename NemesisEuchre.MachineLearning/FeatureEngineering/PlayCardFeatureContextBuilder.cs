using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public static class PlayCardFeatureContextBuilder
{
    public static PlayCardFeatureContext Build(PlayCardDecisionEntity entity)
    {
        var cards = entity.CardsInHand
            .OrderBy(c => c.SortOrder)
            .Select(c => CardIdHelper.ToRelativeCard(c.RelativeCardId))
            .ToArray();

        var playedCards = entity.PlayedCards
            .ToDictionary(
                p => (RelativePlayerPosition)p.RelativePlayerPositionId,
                p => CardIdHelper.ToRelativeCard(p.RelativeCardId));

        var validCards = entity.ValidCards
            .Select(c => CardIdHelper.ToRelativeCard(c.RelativeCardId))
            .ToArray();

        var dealerPickedUpCard = entity.DealerPickedUpRelativeCardId.HasValue
            ? CardIdHelper.ToRelativeCard(entity.DealerPickedUpRelativeCardId.Value)
            : null;

        var knownPlayerSuitVoids = entity.KnownVoids
            .Select(v => new RelativePlayerSuitVoid
            {
                PlayerPosition = (RelativePlayerPosition)v.RelativePlayerPositionId,
                Suit = (RelativeSuit)v.RelativeSuitId,
            })
            .ToArray();

        var cardsAccountedFor = entity.CardsAccountedFor
            .Select(c => CardIdHelper.ToRelativeCard(c.RelativeCardId))
            .ToArray();

        var chosenCard = CardIdHelper.ToRelativeCard(entity.ChosenRelativeCardId);

        return new PlayCardFeatureContext
        {
            CardsInHand = cards,
            PlayedCards = playedCards,
            ValidCards = validCards,
            DealerPickedUpCard = dealerPickedUpCard,
            KnownPlayerSuitVoids = knownPlayerSuitVoids,
            CardsAccountedFor = cardsAccountedFor,
            ChosenCard = chosenCard,
        };
    }
}
