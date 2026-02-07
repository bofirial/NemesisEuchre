namespace NemesisEuchre.GameEngine.Models;

public class CardDecisionContext
{
    public required Card ChosenCard { get; set; }

    public Dictionary<Card, float> DecisionPredictedPoints { get; set; } = [];
}
