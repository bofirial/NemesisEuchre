using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.GameEngine.Models;

public sealed class PlayCardContext
{
    public required Card[] CardsInHand { get; init; }

    public required Card[] ValidCardsToPlay { get; init; }

    public required PlayerPosition PlayerPosition { get; init; }

    public required short TeamScore { get; init; }

    public required short OpponentScore { get; init; }

    public required short WonTricks { get; init; }

    public required short OpponentsWonTricks { get; init; }

    public required Suit TrumpSuit { get; init; }

    public required PlayerPosition CallingPlayer { get; init; }

    public required bool CallingPlayerIsGoingAlone { get; init; }

    public required PlayerPosition Dealer { get; init; }

    public required Card? DealerPickedUpCard { get; init; }

    public required PlayerPosition LeadPlayer { get; init; }

    public required Suit? LeadSuit { get; init; }

    public required short TrickNumber { get; init; }

    public required Dictionary<PlayerPosition, Card> PlayedCardsInTrick { get; init; }

    public required PlayerPosition? CurrentlyWinningTrickPlayer { get; init; }

    public required PlayerSuitVoid[] KnownPlayerSuitVoids { get; init; }

    public required Card[] CardsAccountedFor { get; init; }
}
