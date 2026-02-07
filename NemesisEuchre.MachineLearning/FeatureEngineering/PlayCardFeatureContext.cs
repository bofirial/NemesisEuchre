using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public sealed class PlayCardFeatureContext
{
    public required RelativeCard[] CardsInHand { get; init; }

    public required Dictionary<RelativePlayerPosition, RelativeCard> PlayedCards { get; init; }

    public required RelativeCard[] ValidCards { get; init; }

    public required RelativeCard? DealerPickedUpCard { get; init; }

    public required RelativePlayerSuitVoid[] KnownPlayerSuitVoids { get; init; }

    public required RelativeCard[] CardsAccountedFor { get; init; }

    public required RelativeCard ChosenCard { get; init; }
}
