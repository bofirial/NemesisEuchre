using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public class RelativePlayedCard
{
    public required RelativeCard RelativeCard { get; set; }

    public RelativePlayerPosition PlayerPosition { get; set; }
}
