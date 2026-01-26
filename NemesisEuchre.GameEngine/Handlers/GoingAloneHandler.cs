using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Handlers;

public interface IGoingAloneHandler
{
    bool ShouldPlayerSit(Deal deal, PlayerPosition position);

    PlayerPosition GetNextActivePlayer(PlayerPosition current, Deal deal);

    int GetNumberOfCardsToPlay(Deal deal);
}

public class GoingAloneHandler : IGoingAloneHandler
{
    public bool ShouldPlayerSit(Deal deal, PlayerPosition position)
    {
        if (!deal.CallingPlayerIsGoingAlone)
        {
            return false;
        }

        var partnerPosition = deal.CallingPlayer!.Value.GetPartnerPosition();
        return position == partnerPosition;
    }

    public PlayerPosition GetNextActivePlayer(PlayerPosition current, Deal deal)
    {
        var next = current.GetNextPosition();
        while (ShouldPlayerSit(deal, next))
        {
            next = next.GetNextPosition();
        }

        return next;
    }

    public int GetNumberOfCardsToPlay(Deal deal)
    {
        return deal.CallingPlayerIsGoingAlone ? 3 : 4;
    }
}
