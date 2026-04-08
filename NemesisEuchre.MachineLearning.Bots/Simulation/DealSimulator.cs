using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.MachineLearning.Bots.Simulation;

public interface IDealSimulator
{
    Task<float> SimulateFromPlayCardAsync(
        PlayCardContext context,
        Card candidateCard,
        Dictionary<PlayerPosition, List<Card>> simulatedHands,
        IPlayerActor innerBot);

    Task<float> SimulateFromDiscardAsync(
        DiscardCardContext context,
        Card candidateDiscard,
        Dictionary<PlayerPosition, List<Card>> simulatedHands,
        IPlayerActor innerBot);

    Task<float> SimulateFromCallTrumpAsync(
        CallTrumpContext context,
        CallTrumpDecision candidateDecision,
        Dictionary<PlayerPosition, List<Card>> simulatedHands,
        IPlayerActor innerBot);

    Task<float> SimulateFromPassAsync(
        CallTrumpContext context,
        Dictionary<PlayerPosition, List<Card>> simulatedHands,
        IPlayerActor innerBot);
}

public class DealSimulator : IDealSimulator
{
    private const int TricksPerDeal = 5;
    private const int TrumpValueOffset = 100;
    private const int RightBowerValue = 16;
    private const int LeftBowerValue = 15;
    private const int MinimumTricksToWinBid = 3;
    private const int AllTricks = 5;

    public async Task<float> SimulateFromPlayCardAsync(
        PlayCardContext context,
        Card candidateCard,
        Dictionary<PlayerPosition, List<Card>> simulatedHands,
        IPlayerActor innerBot)
    {
        var adjustedHands = AdjustHandsForPriorPlays(simulatedHands, context.PlayedCardsInTrick);
        var state = BuildStateFromPlayContext(context, adjustedHands);

        var currentTrick = new SimulatedTrick { LeadPosition = context.LeadPlayer };
        AddPriorCardsToTrick(currentTrick, context.PlayedCardsInTrick, context.LeadPlayer, state);

        PlayCardIntoTrick(currentTrick, candidateCard, context.PlayerPosition, state);

        await CompleteTrickAsync(state, currentTrick, context.PlayerPosition, innerBot).ConfigureAwait(false);
        FinalizeTrick(state, currentTrick);

        int currentTrickNumber = context.WonTricks + context.OpponentsWonTricks + 1;
        await PlayRemainingTricksAsync(state, currentTrickNumber + 1, innerBot).ConfigureAwait(false);

        return CalculateSignedScore(state, context.PlayerPosition);
    }

    public async Task<float> SimulateFromDiscardAsync(
        DiscardCardContext context,
        Card candidateDiscard,
        Dictionary<PlayerPosition, List<Card>> simulatedHands,
        IPlayerActor innerBot)
    {
        var allHands = new Dictionary<PlayerPosition, List<Card>>(simulatedHands)
        {
            [context.PlayerPosition] = [.. context.CardsInHand.Where(c => c != candidateDiscard)],
        };

        var (team1Score, team2Score) = context.PlayerPosition.GetTeam() == Team.Team1
            ? (team1Score: context.TeamScore, team2Score: context.OpponentScore)
            : (team1Score: context.OpponentScore, team2Score: context.TeamScore);

        var state = new SimulationState
        {
            PlayerHands = allHands,
            Trump = context.TrumpSuit,
            CallingPlayer = context.CallingPlayer,
            CallingPlayerIsGoingAlone = context.CallingPlayerGoingAlone,
            DealerPosition = context.PlayerPosition,
            DealerPickedUpCard = null,
            Team1Score = team1Score,
            Team2Score = team2Score,
            UpCard = null,
            DiscardedCard = candidateDiscard,
            ChosenDecision = CallTrumpDecision.OrderItUp,
            KnownPlayerSuitVoids = [],
        };

        var leadPosition = state.DealerPosition.GetNextPosition();
        await PlayRemainingTricksAsync(state, 1, innerBot, leadPosition).ConfigureAwait(false);

        return CalculateSignedScore(state, context.PlayerPosition);
    }

    public async Task<float> SimulateFromCallTrumpAsync(
        CallTrumpContext context,
        CallTrumpDecision candidateDecision,
        Dictionary<PlayerPosition, List<Card>> simulatedHands,
        IPlayerActor innerBot)
    {
        var trump = GetTrumpSuitFromDecision(candidateDecision, context.UpCard.Suit);
        var isGoingAlone = IsGoingAloneDecision(candidateDecision);
        var isOrderItUp = candidateDecision is CallTrumpDecision.OrderItUp or CallTrumpDecision.OrderItUpAndGoAlone;

        var allHands = new Dictionary<PlayerPosition, List<Card>>(simulatedHands)
        {
            [context.PlayerPosition] = [.. context.CardsInHand],
        };

        var (callTeam1Score, callTeam2Score) = context.PlayerPosition.GetTeam() == Team.Team1
            ? (callTeam1Score: context.TeamScore, callTeam2Score: context.OpponentScore)
            : (callTeam1Score: context.OpponentScore, callTeam2Score: context.TeamScore);

        var state = new SimulationState
        {
            PlayerHands = allHands,
            Trump = trump,
            CallingPlayer = context.PlayerPosition,
            CallingPlayerIsGoingAlone = isGoingAlone,
            DealerPosition = context.DealerPosition,
            DealerPickedUpCard = isOrderItUp ? context.UpCard : null,
            Team1Score = callTeam1Score,
            Team2Score = callTeam2Score,
            UpCard = context.UpCard,
            DiscardedCard = null,
            ChosenDecision = candidateDecision,
            KnownPlayerSuitVoids = [],
        };

        if (isOrderItUp)
        {
            await SimulateDealerDiscardAsync(state, innerBot).ConfigureAwait(false);
        }

        var leadPosition = state.DealerPosition.GetNextPosition();
        await PlayRemainingTricksAsync(state, 1, innerBot, leadPosition).ConfigureAwait(false);

        return CalculateSignedScore(state, context.PlayerPosition);
    }

    public async Task<float> SimulateFromPassAsync(
        CallTrumpContext context,
        Dictionary<PlayerPosition, List<Card>> simulatedHands,
        IPlayerActor innerBot)
    {
        var allHands = new Dictionary<PlayerPosition, List<Card>>(simulatedHands)
        {
            [context.PlayerPosition] = [.. context.CardsInHand],
        };

        bool isRound1 = context.ValidCallTrumpDecisions.Contains(CallTrumpDecision.OrderItUp);
        byte decisionNumber = context.DecisionNumber;

        var (trump, callingPlayer, chosenDecision, isGoingAlone) = isRound1
            ? await ContinueRound1ThenRound2Async(context, allHands, decisionNumber, innerBot).ConfigureAwait(false)
            : await ContinueRound2Async(context, allHands, decisionNumber, innerBot).ConfigureAwait(false);

        if (trump == null)
        {
            return 0f;
        }

        var isOrderItUp = chosenDecision is CallTrumpDecision.OrderItUp or CallTrumpDecision.OrderItUpAndGoAlone;

        var (passTeam1Score, passTeam2Score) = context.PlayerPosition.GetTeam() == Team.Team1
            ? (passTeam1Score: context.TeamScore, passTeam2Score: context.OpponentScore)
            : (passTeam1Score: context.OpponentScore, passTeam2Score: context.TeamScore);

        var state = new SimulationState
        {
            PlayerHands = allHands,
            Trump = trump.Value,
            CallingPlayer = callingPlayer!.Value,
            CallingPlayerIsGoingAlone = isGoingAlone,
            DealerPosition = context.DealerPosition,
            DealerPickedUpCard = isOrderItUp ? context.UpCard : null,
            Team1Score = passTeam1Score,
            Team2Score = passTeam2Score,
            UpCard = context.UpCard,
            DiscardedCard = null,
            ChosenDecision = chosenDecision,
            KnownPlayerSuitVoids = [],
        };

        if (isOrderItUp)
        {
            await SimulateDealerDiscardAsync(state, innerBot).ConfigureAwait(false);
        }

        var leadPosition = state.DealerPosition.GetNextPosition();
        await PlayRemainingTricksAsync(state, 1, innerBot, leadPosition).ConfigureAwait(false);

        return CalculateSignedScore(state, context.PlayerPosition);
    }

    private static async Task<(Suit? trump, PlayerPosition? callingPlayer, CallTrumpDecision? decision, bool goingAlone)> ContinueRound1ThenRound2Async(
        CallTrumpContext context,
        Dictionary<PlayerPosition, List<Card>> hands,
        byte startDecisionNumber,
        IPlayerActor innerBot)
    {
        var currentPosition = context.PlayerPosition.GetNextPosition();
        byte decisionNumber = (byte)(startDecisionNumber + 1);
        CallTrumpDecision[] round1Decisions = [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp, CallTrumpDecision.OrderItUpAndGoAlone];

        while (currentPosition != context.PlayerPosition)
        {
            var hand = hands[currentPosition];
            var (teamScore, opponentScore) = GetScoresForPlayer(context, currentPosition);

            var callContext = new CallTrumpContext
            {
                CardsInHand = [.. hand],
                PlayerPosition = currentPosition,
                TeamScore = teamScore,
                OpponentScore = opponentScore,
                DealerPosition = context.DealerPosition,
                UpCard = context.UpCard,
                ValidCallTrumpDecisions = round1Decisions,
                DecisionNumber = decisionNumber,
            };

            var decision = await innerBot.CallTrumpAsync(callContext).ConfigureAwait(false);
            if (decision.ChosenCallTrumpDecision != CallTrumpDecision.Pass)
            {
                return (
                    trump: context.UpCard.Suit,
                    callingPlayer: currentPosition,
                    decision: decision.ChosenCallTrumpDecision,
                    goingAlone: IsGoingAloneDecision(decision.ChosenCallTrumpDecision));
            }

            currentPosition = currentPosition.GetNextPosition();
            decisionNumber++;
        }

        return await RunRound2Async(context, hands, decisionNumber, innerBot).ConfigureAwait(false);
    }

    private static async Task<(Suit? trump, PlayerPosition? callingPlayer, CallTrumpDecision? decision, bool goingAlone)> ContinueRound2Async(
        CallTrumpContext context,
        Dictionary<PlayerPosition, List<Card>> hands,
        byte startDecisionNumber,
        IPlayerActor innerBot)
    {
        var currentPosition = context.PlayerPosition.GetNextPosition();
        for (byte decisionNumber = (byte)(startDecisionNumber + 1); currentPosition != context.PlayerPosition; decisionNumber++)
        {
            bool isDealer = currentPosition == context.DealerPosition;
            var validDecisions = GetValidRound2Decisions(context.UpCard.Suit, isDealer);
            var hand = hands[currentPosition];
            var (teamScore, opponentScore) = GetScoresForPlayer(context, currentPosition);

            var callContext = new CallTrumpContext
            {
                CardsInHand = [.. hand],
                PlayerPosition = currentPosition,
                TeamScore = teamScore,
                OpponentScore = opponentScore,
                DealerPosition = context.DealerPosition,
                UpCard = context.UpCard,
                ValidCallTrumpDecisions = validDecisions,
                DecisionNumber = decisionNumber,
            };

            var decision = await innerBot.CallTrumpAsync(callContext).ConfigureAwait(false);
            if (decision.ChosenCallTrumpDecision != CallTrumpDecision.Pass)
            {
                var trump = GetTrumpSuitFromDecision(decision.ChosenCallTrumpDecision, context.UpCard.Suit);
                return (trump,
                    callingPlayer: currentPosition,
                    decision: decision.ChosenCallTrumpDecision,
                    goingAlone: IsGoingAloneDecision(decision.ChosenCallTrumpDecision));
            }

            currentPosition = currentPosition.GetNextPosition();
        }

        return (trump: null, callingPlayer: null, decision: null, goingAlone: false);
    }

    private static async Task<(Suit? trump, PlayerPosition? callingPlayer, CallTrumpDecision? decision, bool goingAlone)> RunRound2Async(
        CallTrumpContext context,
        Dictionary<PlayerPosition, List<Card>> hands,
        byte startDecisionNumber,
        IPlayerActor innerBot)
    {
        var currentPosition = context.DealerPosition.GetNextPosition();
        byte decisionNumber = startDecisionNumber;

        for (int i = 0; i < 4; i++)
        {
            bool isDealer = currentPosition == context.DealerPosition;
            var validDecisions = GetValidRound2Decisions(context.UpCard.Suit, isDealer);
            var hand = hands[currentPosition];
            var (teamScore, opponentScore) = GetScoresForPlayer(context, currentPosition);

            var callContext = new CallTrumpContext
            {
                CardsInHand = [.. hand],
                PlayerPosition = currentPosition,
                TeamScore = teamScore,
                OpponentScore = opponentScore,
                DealerPosition = context.DealerPosition,
                UpCard = context.UpCard,
                ValidCallTrumpDecisions = validDecisions,
                DecisionNumber = decisionNumber,
            };

            var decision = await innerBot.CallTrumpAsync(callContext).ConfigureAwait(false);
            if (decision.ChosenCallTrumpDecision != CallTrumpDecision.Pass)
            {
                var trump = GetTrumpSuitFromDecision(decision.ChosenCallTrumpDecision, context.UpCard.Suit);
                return (trump,
                    callingPlayer: currentPosition,
                    decision: decision.ChosenCallTrumpDecision,
                    goingAlone: IsGoingAloneDecision(decision.ChosenCallTrumpDecision));
            }

            currentPosition = currentPosition.GetNextPosition();
            decisionNumber++;
        }

        return (trump: null, callingPlayer: null, decision: null, goingAlone: false);
    }

    private static CallTrumpDecision[] GetValidRound2Decisions(Suit upcardSuit, bool isDealerWithStickTheDealer)
    {
        var decisions = new List<CallTrumpDecision>();

        if (!isDealerWithStickTheDealer)
        {
            decisions.Add(CallTrumpDecision.Pass);
        }

        foreach (var suit in Enum.GetValues<Suit>().Where(s => s != upcardSuit))
        {
            var (baseDecision, aloneDecision) = GetSuitDecisions(suit);
            decisions.Add(baseDecision);
            decisions.Add(aloneDecision);
        }

        return [.. decisions];
    }

    private static (CallTrumpDecision decision, CallTrumpDecision alone) GetSuitDecisions(Suit suit)
    {
        return suit switch
        {
            Suit.Clubs => (decision: CallTrumpDecision.CallClubs, alone: CallTrumpDecision.CallClubsAndGoAlone),
            Suit.Diamonds => (decision: CallTrumpDecision.CallDiamonds, alone: CallTrumpDecision.CallDiamondsAndGoAlone),
            Suit.Hearts => (decision: CallTrumpDecision.CallHearts, alone: CallTrumpDecision.CallHeartsAndGoAlone),
            Suit.Spades => (decision: CallTrumpDecision.CallSpades, alone: CallTrumpDecision.CallSpadesAndGoAlone),
            _ => throw new ArgumentOutOfRangeException(nameof(suit)),
        };
    }

    private static (short teamScore, short opponentScore) GetScoresForPlayer(CallTrumpContext originalContext, PlayerPosition position)
    {
        bool sameTeam = position.GetTeam() == originalContext.PlayerPosition.GetTeam();
        return sameTeam
            ? (teamScore: originalContext.TeamScore, opponentScore: originalContext.OpponentScore)
            : (teamScore: originalContext.OpponentScore, opponentScore: originalContext.TeamScore);
    }

    private static async Task SimulateDealerDiscardAsync(SimulationState state, IPlayerActor innerBot)
    {
        var dealerPosition = state.DealerPosition;
        var dealerHand = state.PlayerHands[dealerPosition];

        dealerHand.Add(state.UpCard!);
        dealerHand.Sort((a, b) => a.GetTrumpValue(state.Trump).CompareTo(b.GetTrumpValue(state.Trump)));

        var (teamScore, opponentScore) = GetScoresForPosition(state, dealerPosition);

        var discardContext = new DiscardCardContext
        {
            CardsInHand = [.. dealerHand],
            PlayerPosition = dealerPosition,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            TrumpSuit = state.Trump,
            CallingPlayer = state.CallingPlayer,
            CallingPlayerGoingAlone = state.CallingPlayerIsGoingAlone,
            ValidCardsToDiscard = [.. dealerHand],
        };

        var decision = await innerBot.DiscardCardAsync(discardContext).ConfigureAwait(false);
        state.DiscardedCard = decision.ChosenCard;
        dealerHand.Remove(decision.ChosenCard);
    }

    private static async Task PlayRemainingTricksAsync(SimulationState state, int startTrickNumber, IPlayerActor innerBot, PlayerPosition? leadOverride = null)
    {
        var leadPosition = leadOverride ?? GetNextLeadPosition(state);

        for (int trickNumber = startTrickNumber; trickNumber <= TricksPerDeal; trickNumber++)
        {
            var trick = new SimulatedTrick { LeadPosition = leadPosition };
            await PlayFullTrickAsync(state, trick, innerBot).ConfigureAwait(false);
            FinalizeTrick(state, trick);
            leadPosition = trick.WinningPosition!.Value;
        }
    }

    private static async Task PlayFullTrickAsync(SimulationState state, SimulatedTrick trick, IPlayerActor innerBot)
    {
        var currentPosition = trick.LeadPosition;
        int cardsToPlay = state.CallingPlayerIsGoingAlone ? 3 : 4;

        while (trick.CardsPlayed.Count < cardsToPlay)
        {
            if (ShouldPlayerSit(state, currentPosition))
            {
                currentPosition = GetNextActivePlayer(currentPosition, state);
                continue;
            }

            await PlaySimulatedCardAsync(state, trick, currentPosition, innerBot).ConfigureAwait(false);
            currentPosition = GetNextActivePlayer(currentPosition, state);
        }
    }

    private static async Task CompleteTrickAsync(
        SimulationState state,
        SimulatedTrick trick,
        PlayerPosition skipPosition,
        IPlayerActor innerBot)
    {
        int cardsToPlay = state.CallingPlayerIsGoingAlone ? 3 : 4;
        var currentPosition = GetNextActivePlayer(skipPosition, state);

        while (trick.CardsPlayed.Count < cardsToPlay)
        {
            if (ShouldPlayerSit(state, currentPosition))
            {
                currentPosition = GetNextActivePlayer(currentPosition, state);
                continue;
            }

            await PlaySimulatedCardAsync(state, trick, currentPosition, innerBot).ConfigureAwait(false);
            currentPosition = GetNextActivePlayer(currentPosition, state);
        }
    }

    private static async Task PlaySimulatedCardAsync(
        SimulationState state,
        SimulatedTrick trick,
        PlayerPosition position,
        IPlayerActor innerBot)
    {
        var hand = state.PlayerHands[position];
        var handArray = hand.ToArray();
        var validCards = GetValidCardsToPlay(handArray, state.Trump, trick.LeadSuit);

        var (teamScore, opponentScore) = GetScoresForPosition(state, position);
        var playerTeam = position.GetTeam();
        var wonTricks = (short)state.CompletedTricks.Count(t => t.WinningTeam == playerTeam);
        var opponentsWonTricks = (short)state.CompletedTricks.Count(t => t.WinningTeam != null && t.WinningTeam != playerTeam);

        var playedCards = trick.CardsPlayed.ToDictionary(pc => pc.PlayerPosition, pc => pc.Card);

        PlayerPosition? winningTrickPlayer = null;
        if (trick.CardsPlayed.Count > 0 && trick.LeadSuit.HasValue)
        {
            winningTrickPlayer = CalculateWinner(trick, state.Trump);
        }

        var accountedForCards = BuildAccountedForCards(state, trick, position, handArray);

        var context = new PlayCardContext
        {
            CardsInHand = handArray,
            ValidCardsToPlay = validCards,
            PlayerPosition = position,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            WonTricks = wonTricks,
            OpponentsWonTricks = opponentsWonTricks,
            TrumpSuit = state.Trump,
            CallingPlayer = state.CallingPlayer,
            CallingPlayerIsGoingAlone = state.CallingPlayerIsGoingAlone,
            Dealer = state.DealerPosition,
            DealerPickedUpCard = state.DealerPickedUpCard,
            LeadPlayer = trick.LeadPosition,
            LeadSuit = trick.LeadSuit,
            TrickNumber = (short)(state.CompletedTricks.Count + 1),
            PlayedCardsInTrick = playedCards,
            CurrentlyWinningTrickPlayer = winningTrickPlayer,
            KnownPlayerSuitVoids = [.. state.KnownPlayerSuitVoids],
            CardsAccountedFor = accountedForCards,
        };

        var decision = await innerBot.PlayCardAsync(context).ConfigureAwait(false);
        PlayCardIntoTrick(trick, decision.ChosenCard, position, state);
    }

    private static void PlayCardIntoTrick(SimulatedTrick trick, Card card, PlayerPosition position, SimulationState state)
    {
        if (trick.CardsPlayed.Count == 0)
        {
            trick.LeadSuit = card.GetEffectiveSuit(state.Trump);
        }

        trick.CardsPlayed.Add(new PlayedCard(card, position));
        state.PlayerHands[position].Remove(card);

        var effectiveSuit = card.GetEffectiveSuit(state.Trump);
        if (trick.LeadSuit.HasValue && effectiveSuit != trick.LeadSuit.Value && !card.IsTrump(state.Trump))
        {
            var existingVoid = state.KnownPlayerSuitVoids
                .Any(v => v.PlayerPosition == position && v.Suit == trick.LeadSuit.Value);
            if (!existingVoid)
            {
                state.KnownPlayerSuitVoids.Add(new PlayerSuitVoid(position, trick.LeadSuit.Value));
            }
        }
    }

    private static Dictionary<PlayerPosition, List<Card>> AdjustHandsForPriorPlays(
        Dictionary<PlayerPosition, List<Card>> simulatedHands,
        Dictionary<PlayerPosition, Card> playedCardsInTrick)
    {
        if (playedCardsInTrick.Count == 0)
        {
            return simulatedHands;
        }

        var adjusted = new Dictionary<PlayerPosition, List<Card>>(simulatedHands);

        foreach (var (position, _) in playedCardsInTrick)
        {
            if (adjusted.TryGetValue(position, out var hand) && hand.Count > 0)
            {
                adjusted[position] = hand[..^1];
            }
        }

        return adjusted;
    }

    private static void AddPriorCardsToTrick(
        SimulatedTrick trick,
        Dictionary<PlayerPosition, Card> playedCardsInTrick,
        PlayerPosition leadPosition,
        SimulationState state)
    {
        var currentPosition = leadPosition;
        int cardsToPlay = state.CallingPlayerIsGoingAlone ? 3 : 4;

        for (int i = 0; i < cardsToPlay; i++)
        {
            if (playedCardsInTrick.TryGetValue(currentPosition, out var card))
            {
                if (trick.CardsPlayed.Count == 0)
                {
                    trick.LeadSuit = card.GetEffectiveSuit(state.Trump);
                }

                trick.CardsPlayed.Add(new PlayedCard(card, currentPosition));
            }

            currentPosition = ShouldPlayerSit(state, currentPosition.GetNextPosition())
                ? currentPosition.GetNextPosition().GetNextPosition()
                : currentPosition.GetNextPosition();
        }
    }

    private static void FinalizeTrick(SimulationState state, SimulatedTrick trick)
    {
        var winner = CalculateWinner(trick, state.Trump);
        trick.WinningPosition = winner;
        trick.WinningTeam = winner.GetTeam();
        state.CompletedTricks.Add(trick);
    }

    private static Card[] GetValidCardsToPlay(Card[] hand, Suit trump, Suit? leadSuit)
    {
        if (leadSuit == null)
        {
            return hand;
        }

        var matching = hand.Where(c => c.GetEffectiveSuit(trump) == leadSuit).ToArray();
        return matching.Length > 0 ? matching : hand;
    }

    private static PlayerPosition CalculateWinner(SimulatedTrick trick, Suit trump)
    {
        var leadSuit = trick.LeadSuit!.Value;
        var winning = trick.CardsPlayed
            .MaxBy(pc => GetCardValue(pc.Card, leadSuit, trump));
        return winning!.PlayerPosition;
    }

    private static int GetCardValue(Card card, Suit leadSuit, Suit trump)
    {
        if (card.IsTrump(trump))
        {
            return TrumpValueOffset + GetTrumpRankValue(card, trump);
        }

        if (card.GetEffectiveSuit(trump) == leadSuit)
        {
            return (int)card.Rank;
        }

        return 0;
    }

    private static int GetTrumpRankValue(Card card, Suit trump)
    {
        if (card.IsRightBower(trump))
        {
            return RightBowerValue;
        }

        if (card.IsLeftBower(trump))
        {
            return LeftBowerValue;
        }

        return (int)card.Rank;
    }

    private static bool ShouldPlayerSit(SimulationState state, PlayerPosition position)
    {
        if (!state.CallingPlayerIsGoingAlone)
        {
            return false;
        }

        return position == state.CallingPlayer.GetPartnerPosition();
    }

    private static PlayerPosition GetNextActivePlayer(PlayerPosition current, SimulationState state)
    {
        var next = current.GetNextPosition();
        while (ShouldPlayerSit(state, next))
        {
            next = next.GetNextPosition();
        }

        return next;
    }

    private static PlayerPosition GetNextLeadPosition(SimulationState state)
    {
        if (state.CompletedTricks.Count > 0)
        {
            return state.CompletedTricks[^1].WinningPosition!.Value;
        }

        return state.DealerPosition.GetNextPosition();
    }

    private static Card[] BuildAccountedForCards(
        SimulationState state,
        SimulatedTrick currentTrick,
        PlayerPosition currentPlayer,
        Card[] currentHand)
    {
        var accounted = new List<Card>();

        foreach (var completedTrick in state.CompletedTricks)
        {
            accounted.AddRange(completedTrick.CardsPlayed.Select(pc => pc.Card));
        }

        accounted.AddRange(currentTrick.CardsPlayed.Select(pc => pc.Card));
        accounted.AddRange(currentHand);

        var isRound1 = state.ChosenDecision is CallTrumpDecision.OrderItUp or CallTrumpDecision.OrderItUpAndGoAlone;
        if (!isRound1 && state.UpCard != null)
        {
            accounted.Add(state.UpCard);
        }

        if (currentPlayer == state.DealerPosition && state.DiscardedCard != null)
        {
            accounted.Add(state.DiscardedCard);
        }

        return [.. accounted];
    }

    private static float CalculateSignedScore(SimulationState state, PlayerPosition simulatingPlayer)
    {
        var callingTeam = state.CallingPlayer.GetTeam();
        int callingTeamTricks = state.CompletedTricks.Count(t => t.WinningTeam == callingTeam);

        var dealResult = callingTeamTricks switch
        {
            AllTricks when state.CallingPlayerIsGoingAlone => DealResult.WonAndWentAlone,
            AllTricks => DealResult.WonGotAllTricks,
            >= MinimumTricksToWinBid => DealResult.WonStandardBid,
            _ => DealResult.OpponentsEuchred,
        };

        short points = dealResult switch
        {
            DealResult.WonStandardBid => 1,
            DealResult.WonGotAllTricks => 2,
            DealResult.OpponentsEuchred => 2,
            DealResult.WonAndWentAlone => 4,
            DealResult.ThrowIn => throw new NotImplementedException(),
            _ => 0,
        };

        var opposingTeam = callingTeam == Team.Team1 ? Team.Team2 : Team.Team1;
        var winningTeam = dealResult == DealResult.OpponentsEuchred ? opposingTeam : callingTeam;

        return winningTeam == simulatingPlayer.GetTeam() ? points : -points;
    }

    private static SimulationState BuildStateFromPlayContext(
        PlayCardContext context,
        Dictionary<PlayerPosition, List<Card>> simulatedHands)
    {
        var allHands = new Dictionary<PlayerPosition, List<Card>>(simulatedHands)
        {
            [context.PlayerPosition] = [.. context.CardsInHand],
        };

        var team1Score = context.TeamScore;
        var team2Score = context.OpponentScore;
        if (context.PlayerPosition.GetTeam() == Team.Team2)
        {
            team1Score = context.OpponentScore;
            team2Score = context.TeamScore;
        }

        var isOrderItUp = context.DealerPickedUpCard != null;

        return new SimulationState
        {
            PlayerHands = allHands,
            Trump = context.TrumpSuit,
            CallingPlayer = context.CallingPlayer,
            CallingPlayerIsGoingAlone = context.CallingPlayerIsGoingAlone,
            DealerPosition = context.Dealer,
            DealerPickedUpCard = context.DealerPickedUpCard,
            Team1Score = team1Score,
            Team2Score = team2Score,
            UpCard = null,
            DiscardedCard = null,
            ChosenDecision = isOrderItUp ? CallTrumpDecision.OrderItUp : null,
            KnownPlayerSuitVoids = [.. context.KnownPlayerSuitVoids],
        };
    }

    private static (short teamScore, short opponentScore) GetScoresForPosition(SimulationState state, PlayerPosition position)
    {
        return position.GetTeam() == Team.Team1
            ? (teamScore: state.Team1Score, opponentScore: state.Team2Score)
            : (teamScore: state.Team2Score, opponentScore: state.Team1Score);
    }

    private static Suit GetTrumpSuitFromDecision(CallTrumpDecision decision, Suit upcardSuit)
    {
        return decision switch
        {
            CallTrumpDecision.OrderItUp or CallTrumpDecision.OrderItUpAndGoAlone => upcardSuit,
            CallTrumpDecision.CallClubs or CallTrumpDecision.CallClubsAndGoAlone => Suit.Clubs,
            CallTrumpDecision.CallDiamonds or CallTrumpDecision.CallDiamondsAndGoAlone => Suit.Diamonds,
            CallTrumpDecision.CallHearts or CallTrumpDecision.CallHeartsAndGoAlone => Suit.Hearts,
            CallTrumpDecision.CallSpades or CallTrumpDecision.CallSpadesAndGoAlone => Suit.Spades,
            CallTrumpDecision.Pass => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(decision)),
        };
    }

    private static bool IsGoingAloneDecision(CallTrumpDecision decision)
    {
        return decision is CallTrumpDecision.OrderItUpAndGoAlone
            or CallTrumpDecision.CallClubsAndGoAlone
            or CallTrumpDecision.CallDiamondsAndGoAlone
            or CallTrumpDecision.CallHeartsAndGoAlone
            or CallTrumpDecision.CallSpadesAndGoAlone;
    }
}
