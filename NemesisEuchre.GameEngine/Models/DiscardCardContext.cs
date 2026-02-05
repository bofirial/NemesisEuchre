using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.GameEngine.Models;

public sealed class DiscardCardContext
{
    public required Card[] CardsInHand { get; init; }

    public required PlayerPosition PlayerPosition { get; init; }

    public required short TeamScore { get; init; }

    public required short OpponentScore { get; init; }

    public required Suit TrumpSuit { get; init; }

    public required PlayerPosition CallingPlayer { get; init; }

    public required bool CallingPlayerGoingAlone { get; init; }

    public required Card[] ValidCardsToDiscard { get; init; }
}
