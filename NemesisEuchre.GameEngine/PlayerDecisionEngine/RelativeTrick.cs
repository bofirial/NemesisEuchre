using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public class RelativeTrick
{
    public RelativePlayerPosition LeadPosition { get; set; }

    public List<RelativePlayedCard> CardsPlayed { get; } = [];

    public Suit? LeadSuit { get; set; }
}
