using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Extensions;

public static class TrickExtensions
{
    public static RelativeTrick ToRelative(this Trick trick, PlayerPosition self, Suit trump)
    {
        return new RelativeTrick
        {
            LeadPosition = trick.LeadPosition.ToRelativePosition(self),
            LeadSuit = trick.LeadSuit?.ToRelativeSuit(trump),
            CardsPlayed = [.. trick.CardsPlayed.Select(cardPlayed => cardPlayed.ToRelative(self, trump))],
        };
    }
}
