using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Extensions;

public static class DealExtensions
{
    public static RelativeDeal ToRelative(this Deal deal, PlayerPosition self)
    {
        if (!deal.Trump.HasValue)
        {
            throw new ArgumentException("Cannot create a relative deal until trump has been called");
        }

        var relativeDeal = new RelativeDeal
        {
            DealStatus = deal.DealStatus,
            DealerPosition = deal.DealerPosition?.ToRelativePosition(self),
            UpCard = deal.UpCard?.ToRelative(deal.Trump.Value),
            CallingPlayer = deal.CallingPlayer?.ToRelativePosition(self),
            CallingPlayerIsGoingAlone = deal.CallingPlayerIsGoingAlone,
            CurrentTrick = deal.CurrentTrick?.ToRelative(self, deal.Trump.Value),
        };

        foreach (var trick in deal.CompletedTricks)
        {
            relativeDeal.CompletedTricks.Add(trick.ToRelative(self, deal.Trump.Value));
        }

        return relativeDeal;
    }
}
