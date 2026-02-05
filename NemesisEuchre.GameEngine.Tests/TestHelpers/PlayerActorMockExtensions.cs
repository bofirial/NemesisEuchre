using Moq.Language.Flow;

using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Tests.TestHelpers;

public static class PlayerActorMockExtensions
{
    public static IReturnsResult<IPlayerActor> ReturnsPlayCard(
        this ISetup<IPlayerActor, Task<Card>> setup,
        Func<PlayCardContext, Card> cardSelector)
    {
        return setup.Returns((PlayCardContext context) => Task.FromResult(cardSelector(context)));
    }

    public static IReturnsResult<IPlayerActor> ReturnsFirstValidCard(
        this ISetup<IPlayerActor, Task<Card>> setup)
    {
        return setup.Returns((PlayCardContext context) => Task.FromResult(context.ValidCardsToPlay[0]));
    }

    public static IReturnsResult<IPlayerActor> ReturnsFirstValidCard(
        this IReturnsThrows<IPlayerActor, Task<Card>> setup)
    {
        return setup.Returns((PlayCardContext context) => Task.FromResult(context.ValidCardsToPlay[0]));
    }

    public static IReturnsResult<IPlayerActor> ReturnsCallTrump(
        this ISetup<IPlayerActor, Task<CallTrumpDecision>> setup,
        Func<CallTrumpContext, CallTrumpDecision> decisionSelector)
    {
        return setup.Returns((CallTrumpContext context) => Task.FromResult(decisionSelector(context)));
    }

    public static IReturnsResult<IPlayerActor> ReturnsFirstValidDecision(
        this ISetup<IPlayerActor, Task<CallTrumpDecision>> setup)
    {
        return setup.Returns((CallTrumpContext context) => Task.FromResult(context.ValidCallTrumpDecisions[0]));
    }

    public static IReturnsResult<IPlayerActor> ReturnsDiscardCard(
        this ISetup<IPlayerActor, Task<Card>> setup,
        Func<DiscardCardContext, Card> cardSelector)
    {
        return setup.Returns((DiscardCardContext context) => Task.FromResult(cardSelector(context)));
    }

    public static IReturnsResult<IPlayerActor> ReturnsFirstValidDiscardCard(
        this ISetup<IPlayerActor, Task<Card>> setup)
    {
        return setup.Returns((DiscardCardContext context) => Task.FromResult(context.ValidCardsToDiscard[0]));
    }
}
