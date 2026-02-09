using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.GameEngine.Models;

public class DiscardCardDecisionRecord
{
    public Card[] CardsInHand { get; set; } = [];

    public PlayerPosition PlayerPosition { get; set; }

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public Suit TrumpSuit { get; set; }

    public PlayerPosition CallingPlayer { get; set; }

    public bool CallingPlayerGoingAlone { get; set; }

    public Card[] ValidCardsToDiscard { get; set; } = [];

    public required Card ChosenCard { get; set; }

    public Dictionary<Card, float> DecisionPredictedPoints { get; set; } = [];
}
