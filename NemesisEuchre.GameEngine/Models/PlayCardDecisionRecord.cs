using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.GameEngine.Models;

public class PlayCardDecisionRecord
{
    public Card[] CardsInHand { get; set; } = [];

    public PlayerPosition PlayerPosition { get; set; }

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public Suit TrumpSuit { get; set; }

    public PlayerPosition LeadPlayer { get; set; }

    public Suit? LeadSuit { get; set; }

    public Dictionary<PlayerPosition, Card> PlayedCards { get; set; } = [];

    public PlayerPosition? WinningTrickPlayer { get; set; }

    public short TrickNumber { get; set; }

    public Card[] ValidCardsToPlay { get; set; } = [];

    public PlayerPosition CallingPlayer { get; set; }

    public bool CallingPlayerGoingAlone { get; set; }

    public PlayerPosition Dealer { get; set; }

    public Card? DealerPickedUpCard { get; set; }

    public Card[] CardsAccountedFor { get; set; } = [];

    public PlayerSuitVoid[] KnownPlayerSuitVoids { get; set; } = [];

    public required Card ChosenCard { get; set; }

    public Dictionary<Card, float> DecisionPredictedPoints { get; set; } = [];
}
