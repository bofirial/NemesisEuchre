using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Extensions;

public static class PlayedCardExtensions
{
    public static RelativePlayedCard ToRelative(this PlayedCard playedCard, PlayerPosition self, Suit trump)
    {
        return new RelativePlayedCard
        {
            RelativeCard = playedCard.Card.ToRelative(trump),
            PlayerPosition = playedCard.PlayerPosition.ToRelativePosition(self),
        };
    }
}
