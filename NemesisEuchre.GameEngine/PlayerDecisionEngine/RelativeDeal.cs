using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public class RelativeDeal
{
    public DealStatus DealStatus { get; set; } = DealStatus.NotStarted;

    public RelativePlayerPosition? DealerPosition { get; set; }

    public RelativeCard? UpCard { get; set; }

    public RelativePlayerPosition? CallingPlayer { get; set; }

    public bool CallingPlayerIsGoingAlone { get; set; }

    public required RelativeTrick[] CompletedTricks { get; set; }
}
