using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Extensions;

public static class TrickExtensions
{
    public static RelativeTrick ToRelative(this Trick trick, PlayerPosition self, Suit trump)
    {
        var relativeTrick = new RelativeTrick
        {
            LeadPosition = trick.LeadPosition.ToRelativePosition(self),
            LeadSuit = trick.LeadSuit?.ToRelativeSuit(trump),
        };

        foreach (var playedCard in trick.CardsPlayed)
        {
            relativeTrick.CardsPlayed.Add(playedCard.ToRelative(self, trump));
        }

        return relativeTrick;
    }
}
