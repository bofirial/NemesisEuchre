using System.Diagnostics.CodeAnalysis;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Extensions;

public static class DealExtensions
{
    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Conditional Expression would reduce code readability considerably")]
    public static RelativeDeal ToRelative(this Deal deal, PlayerPosition self)
    {
        if (!deal.Trump.HasValue)
        {
            throw new ArgumentException("Cannot create a relative deal until trump has been called");
        }

        return new RelativeDeal
        {
            DealStatus = deal.DealStatus,
            DealerPosition = deal.DealerPosition?.ToRelativePosition(self),
            UpCard = deal.UpCard?.ToRelative(deal.Trump.Value),
            CallingPlayer = deal.CallingPlayer?.ToRelativePosition(self),
            CallingPlayerIsGoingAlone = deal.CallingPlayerIsGoingAlone,
            CompletedTricks = [.. deal.CompletedTricks.Select(trick => trick.ToRelative(self, deal.Trump.Value))],
        };
    }
}
