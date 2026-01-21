using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Extensions;

public static class HandExtensions
{
    public static RelativeHand ToRelative(this Hand hand, PlayerPosition self)
    {
        if (!hand.Trump.HasValue)
        {
            throw new ArgumentException("Cannot create a relative hand until trump has been called");
        }

        var relativeHand = new RelativeHand
        {
            HandStatus = hand.HandStatus,
            DealerPosition = hand.DealerPosition?.ToRelativePosition(self),
            UpCard = hand.UpCard?.ToRelative(hand.Trump.Value),
            CallingPlayer = hand.CallingPlayer?.ToRelativePosition(self),
            CallingPlayerIsGoingAlone = hand.CallingPlayerIsGoingAlone,
            CurrentTrick = hand.CurrentTrick?.ToRelative(self, hand.Trump.Value),
        };

        foreach (var trick in hand.CompletedTricks)
        {
            relativeHand.CompletedTricks.Add(trick.ToRelative(self, hand.Trump.Value));
        }

        return relativeHand;
    }
}
