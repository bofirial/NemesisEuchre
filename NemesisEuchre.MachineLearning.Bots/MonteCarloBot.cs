using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;
using NemesisEuchre.MachineLearning.Bots.Simulation;

namespace NemesisEuchre.MachineLearning.Bots;

public class MonteCarloBot(
    IPlayerActor[] innerBots,
    IDealSimulator dealSimulator,
    IHiddenCardDistributor hiddenCardDistributor,
    IRandomNumberGenerator[] randoms,
    int simulationCount) : IPlayerActor
{
    public ActorType ActorType => ActorType.MonteCarlo;

    public async Task<CallTrumpDecisionContext> CallTrumpAsync(CallTrumpContext context)
    {
        var scores = new Dictionary<CallTrumpDecision, float>();
        var bestDecision = context.ValidCallTrumpDecisions[0];
        var bestScore = float.MinValue;

        var sittingOutPlayer = GetSittingOutPlayer();

        foreach (var decision in context.ValidCallTrumpDecisions)
        {
            float totalScore = await RunSimulationsAsync(
                simulationCount,
                (bot, rng) =>
                {
                    var hands = hiddenCardDistributor.DistributeHiddenCards(
                        BuildAccountedForCallTrump(context),
                        context.PlayerPosition,
                        5,
                        context.UpCard.Suit,
                        [],
                        sittingOutPlayer,
                        rng);

                    return decision == CallTrumpDecision.Pass
                        ? dealSimulator.SimulateFromPassAsync(context, hands, bot)
                        : dealSimulator.SimulateFromCallTrumpAsync(context, decision, hands, bot);
                }).ConfigureAwait(false);

            float averageScore = totalScore / simulationCount;
            scores[decision] = averageScore;

            if (averageScore > bestScore)
            {
                bestScore = averageScore;
                bestDecision = decision;
            }
        }

        return new CallTrumpDecisionContext
        {
            ChosenCallTrumpDecision = bestDecision,
            DecisionPredictedPoints = scores,
        };
    }

    public async Task<CardDecisionContext> DiscardCardAsync(DiscardCardContext context)
    {
        var scores = new Dictionary<Card, float>();
        var bestCard = context.ValidCardsToDiscard[0];
        var bestScore = float.MinValue;

        var sittingOutPlayer = context.CallingPlayerGoingAlone
            ? context.CallingPlayer.GetPartnerPosition()
            : (PlayerPosition?)null;

        foreach (var candidateDiscard in context.ValidCardsToDiscard)
        {
            float totalScore = await RunSimulationsAsync(
                simulationCount,
                (bot, rng) =>
                {
                    var accountedFor = BuildAccountedForDiscard(context, candidateDiscard);
                    var hands = hiddenCardDistributor.DistributeHiddenCards(
                        accountedFor,
                        context.PlayerPosition,
                        5,
                        context.TrumpSuit,
                        [],
                        sittingOutPlayer,
                        rng);

                    return dealSimulator.SimulateFromDiscardAsync(context, candidateDiscard, hands, bot);
                }).ConfigureAwait(false);

            float averageScore = totalScore / simulationCount;
            scores[candidateDiscard] = averageScore;

            if (averageScore > bestScore)
            {
                bestScore = averageScore;
                bestCard = candidateDiscard;
            }
        }

        return new CardDecisionContext
        {
            ChosenCard = bestCard,
            DecisionPredictedPoints = scores,
        };
    }

    public async Task<CardDecisionContext> PlayCardAsync(PlayCardContext context)
    {
        var scores = new Dictionary<Card, float>();
        var bestCard = context.ValidCardsToPlay[0];
        var bestScore = float.MinValue;

        var sittingOutPlayer = context.CallingPlayerIsGoingAlone
            ? context.CallingPlayer.GetPartnerPosition()
            : (PlayerPosition?)null;

        int otherPlayerCardCount = CalculateOtherPlayerCardCount(context);

        foreach (var candidateCard in context.ValidCardsToPlay)
        {
            float totalScore = await RunSimulationsAsync(
                simulationCount,
                (bot, rng) =>
                {
                    var hands = hiddenCardDistributor.DistributeHiddenCards(
                        context.CardsAccountedFor,
                        context.PlayerPosition,
                        otherPlayerCardCount,
                        context.TrumpSuit,
                        context.KnownPlayerSuitVoids,
                        sittingOutPlayer,
                        rng);

                    return dealSimulator.SimulateFromPlayCardAsync(context, candidateCard, hands, bot);
                }).ConfigureAwait(false);

            float averageScore = totalScore / simulationCount;
            scores[candidateCard] = averageScore;

            if (averageScore > bestScore)
            {
                bestScore = averageScore;
                bestCard = candidateCard;
            }
        }

        return new CardDecisionContext
        {
            ChosenCard = bestCard,
            DecisionPredictedPoints = scores,
        };
    }

    private static int CalculateOtherPlayerCardCount(PlayCardContext context)
    {
        return context.CardsInHand.Length;
    }

    private static Card[] BuildAccountedForCallTrump(CallTrumpContext context)
    {
        var accounted = new List<Card>(context.CardsInHand) { context.UpCard };
        return [.. accounted];
    }

    private static Card[] BuildAccountedForDiscard(DiscardCardContext context, Card candidateDiscard)
    {
        var accounted = new List<Card>(context.CardsInHand.Where(c => c != candidateDiscard));
        return [.. accounted];
    }

    private static PlayerPosition? GetSittingOutPlayer()
    {
        return null;
    }

    private async Task<float> RunSimulationsAsync(
        int totalSimulations,
        Func<IPlayerActor, IRandomNumberGenerator, Task<float>> simulateOne)
    {
        float sum = 0f;
        for (int i = 0; i < totalSimulations; i++)
        {
            sum += await simulateOne(innerBots[0], randoms[0]).ConfigureAwait(false);
        }

        return sum;
    }
}
