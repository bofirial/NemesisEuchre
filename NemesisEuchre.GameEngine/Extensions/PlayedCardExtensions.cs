using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Extensions;

public static class PlayedCardExtensions
{
    public static RelativePlayedCard ToRelative(this PlayedCard playedCard, PlayerPosition self)
    {
        return new RelativePlayedCard
        {
            Card = playedCard.Card,
            PlayerPosition = playedCard.PlayerPosition.ToRelativePosition(self),
        };
    }
}
