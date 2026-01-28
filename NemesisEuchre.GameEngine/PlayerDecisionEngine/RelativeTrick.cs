using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public class RelativeTrick
{
    public RelativePlayerPosition LeadPosition { get; set; }

    public RelativePlayedCard[] CardsPlayed { get; set; } = [];

    public RelativeSuit? LeadSuit { get; set; }
}
