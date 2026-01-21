using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Extensions;

public static class HandExtensions
{
    public static RelativeHand ToRelative(this Hand hand, PlayerPosition self)
    {
        var relativeHand = new RelativeHand
        {
            HandStatus = hand.HandStatus,
            DealerPosition = hand.DealerPosition?.ToRelativePosition(self),
            UpCard = hand.UpCard,
            Trump = hand.Trump,
            CallingPlayer = hand.CallingPlayer?.ToRelativePosition(self),
            CallingPlayerIsGoingAlone = hand.CallingPlayerIsGoingAlone,
            CurrentTrick = hand.CurrentTrick?.ToRelative(self),
        };

        foreach (var trick in hand.CompletedTricks)
        {
            relativeHand.CompletedTricks.Add(trick.ToRelative(self));
        }

        return relativeHand;
    }
}
