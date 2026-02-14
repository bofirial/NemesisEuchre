using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Models;

public sealed record PlayCardRecordingContext(
    Deal Deal,
    Trick Trick,
    PlayerPosition PlayerPosition,
    Card[] Hand,
    Card[] ValidCards,
    CardDecisionContext CardDecisionContext,
    ITrickWinnerCalculator TrickWinnerCalculator);

public sealed record CallTrumpRecordingContext(
    Deal Deal,
    PlayerPosition PlayerPosition,
    CallTrumpDecision[] ValidDecisions,
    CallTrumpDecisionContext CallTrumpDecisionContext);

public sealed record DiscardCardRecordingContext(
    Deal Deal,
    PlayerPosition PlayerPosition,
    Card[] Hand,
    CardDecisionContext CardDecisionContext);
