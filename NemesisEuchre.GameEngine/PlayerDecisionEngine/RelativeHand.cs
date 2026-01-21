using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public class RelativeHand
{
    public HandStatus HandStatus { get; set; } = HandStatus.NotStarted;

    public RelativePlayerPosition? DealerPosition { get; set; }

    public RelativeCard? UpCard { get; set; }

    public RelativePlayerPosition? CallingPlayer { get; set; }

    public bool CallingPlayerIsGoingAlone { get; set; }

    public RelativeTrick? CurrentTrick { get; set; }

    public List<RelativeTrick> CompletedTricks { get; } = [];
}
