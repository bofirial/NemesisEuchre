using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public static class PlayCardEntityDeserializer
{
    public static PlayCardFeatureContext Deserialize(PlayCardDecisionEntity entity)
    {
        var cards = JsonDeserializationHelper.DeserializeRelativeCards(entity.CardsInHandJson);
        var playedCards = JsonDeserializationHelper.DeserializePlayedCards(entity.PlayedCardsJson);
        var validCards = JsonDeserializationHelper.DeserializeRelativeCards(entity.ValidCardsToPlayJson);
        var dealerPickedUpCard = !string.IsNullOrEmpty(entity.DealerPickedUpCardJson)
            ? JsonDeserializationHelper.DeserializeRelativeCard(entity.DealerPickedUpCardJson)
            : null;
        var knownPlayerSuitVoids = JsonDeserializationHelper.DeserializeKnownPlayerVoids(entity.KnownPlayerSuitVoidsJson);
        var cardsAccountedFor = JsonDeserializationHelper.DeserializeRelativeCards(entity.CardsAccountedForJson);
        var chosenCard = JsonDeserializationHelper.DeserializeRelativeCard(entity.ChosenCardJson);

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
