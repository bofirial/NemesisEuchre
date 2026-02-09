using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public sealed class DiscardCardFeatureContext
{
    public required RelativeCard[] CardsInHand { get; init; }

    public required RelativeCard ChosenCard { get; init; }
}
