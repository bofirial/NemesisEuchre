using Moq.Language.Flow;

using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Tests.TestHelpers;

public static class PlayerActorMockExtensions
{
    public static IReturnsResult<IPlayerActor> ReturnsPlayCard(
        this ISetup<IPlayerActor, Task<CardDecisionContext>> setup,
        Func<PlayCardContext, Card> cardSelector)
    {
        return setup.Returns((PlayCardContext context) => Task.FromResult(new CardDecisionContext { ChosenCard = cardSelector(context) }));
    }

    public static IReturnsResult<IPlayerActor> ReturnsFirstValidCard(
        this ISetup<IPlayerActor, Task<CardDecisionContext>> setup)
    {
        return setup.Returns((PlayCardContext context) => Task.FromResult(new CardDecisionContext { ChosenCard = context.ValidCardsToPlay[0] }));
    }

    public static IReturnsResult<IPlayerActor> ReturnsFirstValidCard(
        this IReturnsThrows<IPlayerActor, Task<CardDecisionContext>> setup)
    {
        return setup.Returns((PlayCardContext context) => Task.FromResult(new CardDecisionContext { ChosenCard = context.ValidCardsToPlay[0] }));
    }

    public static IReturnsResult<IPlayerActor> ReturnsCallTrump(
        this ISetup<IPlayerActor, Task<CallTrumpDecisionContext>> setup,
        Func<CallTrumpContext, CallTrumpDecision> decisionSelector)
    {
        return setup.Returns((CallTrumpContext context) => Task.FromResult(new CallTrumpDecisionContext { ChosenCallTrumpDecision = decisionSelector(context) }));
    }

    public static IReturnsResult<IPlayerActor> ReturnsFirstValidDecision(
        this ISetup<IPlayerActor, Task<CallTrumpDecisionContext>> setup)
    {
        return setup.Returns((CallTrumpContext context) => Task.FromResult(new CallTrumpDecisionContext { ChosenCallTrumpDecision = context.ValidCallTrumpDecisions[0] }));
    }

    public static IReturnsResult<IPlayerActor> ReturnsDiscardCard(
        this ISetup<IPlayerActor, Task<CardDecisionContext>> setup,
        Func<DiscardCardContext, Card> cardSelector)
    {
        return setup.Returns((DiscardCardContext context) => Task.FromResult(new CardDecisionContext { ChosenCard = cardSelector(context) }));
    }

    public static IReturnsResult<IPlayerActor> ReturnsFirstValidDiscardCard(
        this ISetup<IPlayerActor, Task<CardDecisionContext>> setup)
    {
        return setup.Returns((DiscardCardContext context) => Task.FromResult(new CardDecisionContext { ChosenCard = context.ValidCardsToDiscard[0] }));
    }
}
