using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public interface IPlayerActor
{
    ActorType ActorType { get; }

    Task<CallTrumpDecision> CallTrumpAsync(CallTrumpContext context);

    Task<Card> DiscardCardAsync(DiscardCardContext context);

    Task<Card> PlayCardAsync(PlayCardContext context);
}
