namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public class RelativeCardDecisionContext
{
    public required RelativeCard ChosenCard { get; set; }

    public Dictionary<RelativeCard, float> DecisionPredictedPoints { get; set; } = [];
}
