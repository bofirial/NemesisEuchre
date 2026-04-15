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
    int simulationCount,
    bool skipSimCallTrump = false,
    bool skipSimDiscard = false,
    bool skipSimPlayCard = false,
    float earlyTerminationMargin = 1.5f) : IPlayerActor
{
    public ActorType ActorType => ActorType.MonteCarlo;

    public async Task<CallTrumpDecisionContext> CallTrumpAsync(CallTrumpContext context)
    {
        if (skipSimCallTrump)
        {
            return await innerBots[0].CallTrumpAsync(context).ConfigureAwait(false);
        }

        var sittingOutPlayer = GetSittingOutPlayer();

        var (bestDecision, scores) = await EvaluateOptionsAsync(
            context.ValidCallTrumpDecisions,
            decision => (bot, rng) =>
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

        return new CallTrumpDecisionContext
        {
            ChosenCallTrumpDecision = bestDecision,
            DecisionPredictedPoints = scores,
        };
    }

    public async Task<CardDecisionContext> DiscardCardAsync(DiscardCardContext context)
    {
        if (skipSimDiscard)
        {
            return await innerBots[0].DiscardCardAsync(context).ConfigureAwait(false);
        }

        var sittingOutPlayer = context.CallingPlayerGoingAlone
            ? context.CallingPlayer.GetPartnerPosition()
            : (PlayerPosition?)null;

        var (bestCard, scores) = await EvaluateOptionsAsync(
            context.ValidCardsToDiscard,
            candidateDiscard => (bot, rng) =>
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

        return new CardDecisionContext
        {
            ChosenCard = bestCard,
            DecisionPredictedPoints = scores,
        };
    }

    public async Task<CardDecisionContext> PlayCardAsync(PlayCardContext context)
    {
        if (skipSimPlayCard)
        {
            return await innerBots[0].PlayCardAsync(context).ConfigureAwait(false);
        }

        var sittingOutPlayer = context.CallingPlayerIsGoingAlone
            ? context.CallingPlayer.GetPartnerPosition()
            : (PlayerPosition?)null;

        int otherPlayerCardCount = CalculateOtherPlayerCardCount(context);

        var (bestCard, scores) = await EvaluateOptionsAsync(
            context.ValidCardsToPlay,
            candidateCard => (bot, rng) =>
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

    private async Task<(TOption bestOption, Dictionary<TOption, float> scores)> EvaluateOptionsAsync<TOption>(
        TOption[] options,
        Func<TOption, Func<IPlayerActor, IRandomNumberGenerator, Task<float>>> simulationFactory)
        where TOption : notnull
    {
        if (options.Length == 1)
        {
            return (options[0], new Dictionary<TOption, float> { [options[0]] = 0f });
        }

        int minSims = Math.Min(simulationCount, Math.Max(simulationCount / 5, 10));
        int remainingSims = simulationCount - minSims;

        var sums = new Dictionary<TOption, float>();
        var scores = new Dictionary<TOption, float>();
        var bestOption = options[0];
        var bestScore = float.MinValue;
        var secondBestScore = float.MinValue;

        foreach (var option in options)
        {
            float sum = await RunSimulationsAsync(minSims, simulationFactory(option)).ConfigureAwait(false);
            float average = sum / minSims;

            sums[option] = sum;
            scores[option] = average;

            if (average > bestScore)
            {
                secondBestScore = bestScore;
                bestScore = average;
                bestOption = option;
            }
            else if (average > secondBestScore)
            {
                secondBestScore = average;
            }
        }

        if (remainingSims <= 0 || bestScore - secondBestScore >= earlyTerminationMargin)
        {
            return (bestOption, scores);
        }

        bestOption = options[0];
        bestScore = float.MinValue;

        foreach (var option in options)
        {
            float additionalSum = await RunSimulationsAsync(remainingSims, simulationFactory(option)).ConfigureAwait(false);
            float totalSum = sums[option] + additionalSum;
            float average = totalSum / simulationCount;

            scores[option] = average;

            if (average > bestScore)
            {
                bestScore = average;
                bestOption = option;
            }
        }

        return (bestOption, scores);
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
