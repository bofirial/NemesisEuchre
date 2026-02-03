using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Services;

public interface IVoidDetector
{
    bool TryDetectVoid(
        Deal deal,
        Card chosenCard,
        Suit? leadSuit,
        Suit trump,
        PlayerPosition playerPosition,
        out Suit voidSuit);
}

public class VoidDetector : IVoidDetector
{
    public bool TryDetectVoid(
        Deal deal,
        Card chosenCard,
        Suit? leadSuit,
        Suit trump,
        PlayerPosition playerPosition,
        out Suit voidSuit)
    {
        voidSuit = default;

        if (leadSuit == null)
        {
            return false;
        }

        var effectiveSuit = chosenCard.GetEffectiveSuit(trump);

        if (effectiveSuit == leadSuit.Value)
        {
            return false;
        }

        if (deal.KnownPlayerSuitVoids.Any(v => v.PlayerPosition == playerPosition && v.Suit == leadSuit.Value))
        {
            return false;
        }

        voidSuit = leadSuit.Value;
        return true;
    }
}
