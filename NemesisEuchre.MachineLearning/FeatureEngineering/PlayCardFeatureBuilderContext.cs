using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public sealed record PlayCardFeatureBuilderContext(
    RelativeCard[] CardsInHand,
    RelativeCard[] ValidCards,
    Dictionary<RelativePlayerPosition, RelativeCard> PlayedCards,
    short TeamScore,
    short OpponentScore,
    RelativePlayerPosition LeadPlayer,
    RelativeSuit? LeadSuit,
    RelativePlayerPosition CallingPlayer,
    bool CallingPlayerGoingAlone,
    RelativePlayerPosition Dealer,
    RelativeCard? DealerPickedUpCard,
    RelativePlayerSuitVoid[] KnownPlayerSuitVoids,
    RelativeCard[] CardsAccountedFor,
    RelativePlayerPosition? WinningTrickPlayer,
    short TrickNumber,
    short WonTricks,
    short OpponentsWonTricks,
    RelativeCard ChosenCard);
