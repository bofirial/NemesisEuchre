using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public interface IPlayerActor
{
    ActorType ActorType { get; }

    Task<CallTrumpDecisionContext> CallTrumpAsync(CallTrumpContext context);

    Task<CardDecisionContext> DiscardCardAsync(DiscardCardContext context);

    Task<CardDecisionContext> PlayCardAsync(PlayCardContext context);
}
