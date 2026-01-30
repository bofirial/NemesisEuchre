namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public enum CallTrumpDecision
{
    Pass = 0,
    CallSpades = 1,
    CallHearts = 2,
    CallClubs = 3,
    CallDiamonds = 4,
    CallSpadesAndGoAlone = 5,
    CallHeartsAndGoAlone = 6,
    CallClubsAndGoAlone = 7,
    CallDiamondsAndGoAlone = 8,
    OrderItUp = 9,
    OrderItUpAndGoAlone = 10,
}
