using Microsoft.Extensions.Options;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine;

public interface ITrumpSelectionOrchestrator
{
    Task SelectTrumpAsync(Deal deal);
}

public class TrumpSelectionOrchestrator(IEnumerable<IPlayerActor> playerActors, IOptions<GameOptions> gameOptions) : ITrumpSelectionOrchestrator
{
    private const int PlayersPerDeal = 4;
    private readonly Dictionary<ActorType, IPlayerActor> _playerActors = playerActors.ToDictionary(x => x.ActorType, x => x);

    public async Task SelectTrumpAsync(Deal deal)
    {
        ValidatePreconditions(deal);

        bool trumpSelected = await ExecuteRound1Async(deal);

        if (!trumpSelected)
        {
            await ExecuteRound2Async(deal);
        }
    }

    private static void ValidatePreconditions(Deal deal)
    {
        ArgumentNullException.ThrowIfNull(deal);

        if (deal.DealStatus != DealStatus.SelectingTrump)
        {
            throw new InvalidOperationException($"Deal must be in SelectingTrump status, but was {deal.DealStatus}");
        }

        if (deal.DealerPosition == null)
        {
            throw new InvalidOperationException("DealerPosition must be set");
        }

        if (deal.UpCard == null)
        {
            throw new InvalidOperationException("UpCard must be set");
        }

        if (deal.Players.Count != PlayersPerDeal)
        {
            throw new InvalidOperationException($"Deal must have exactly {PlayersPerDeal} players, but had {deal.Players.Count}");
        }
    }

    private static PlayerPosition GetFirstPlayerPosition(Deal deal)
    {
        return deal.DealerPosition!.Value.GetNextPosition();
    }

    private static bool IsDealer(Deal deal, PlayerPosition position)
    {
        return position == deal.DealerPosition!.Value;
    }

    private static void ValidateDecision(CallTrumpDecision decision, CallTrumpDecision[] validDecisions)
    {
        if (!validDecisions.Contains(decision))
        {
            throw new InvalidOperationException("CallTrumpDecision was not included in ValidDecisions");
        }
    }

    private static void SetTrumpResult(Deal deal, Suit trump, PlayerPosition callingPlayer, CallTrumpDecision decision)
    {
        deal.Trump = trump;
        deal.CallingPlayer = callingPlayer;
        deal.CallingPlayerIsGoingAlone = IsGoingAloneDecision(decision);
    }

    private static void AddUpcardToDealerHand(DealPlayer dealer, Card upCard)
    {
        dealer.CurrentHand.Add(upCard);
    }

    private static RelativeCard[] ConvertToRelativeCards(List<Card> cards, Suit trump)
    {
        return [.. cards.Select(c => c.ToRelative(trump))];
    }

    private static void ValidateDiscard(RelativeCard cardToDiscard, RelativeCard[] validCards)
    {
        if (!validCards.Contains(cardToDiscard))
        {
            throw new InvalidOperationException("CardToDiscard was not included in ValidCardsToDiscard");
        }
    }

    private static (short TeamScore, short OpponentScore) GetScores(Deal deal, PlayerPosition playerPosition)
    {
        var isTeam1 = playerPosition.GetTeam() == Team.Team1;
        return isTeam1 ? (deal.Team1Score, deal.Team2Score) : (deal.Team2Score, deal.Team1Score);
    }

    private static RelativePlayerPosition GetRelativeDealerPosition(Deal deal, PlayerPosition playerPosition)
    {
        return deal.DealerPosition!.Value.ToRelativePosition(playerPosition);
    }

    private static CallTrumpDecision[] GetValidRound1Decisions()
    {
        return [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp, CallTrumpDecision.OrderItUpAndGoAlone];
    }

    private static void AddSuitDecisions(List<CallTrumpDecision> decisions, Suit suit)
    {
        var (callDecision, callAloneDecision) = suit switch
        {
            Suit.Clubs => (CallTrumpDecision.CallClubs, CallTrumpDecision.CallClubsAndGoAlone),
            Suit.Diamonds => (CallTrumpDecision.CallDiamonds, CallTrumpDecision.CallDiamondsAndGoAlone),
            Suit.Hearts => (CallTrumpDecision.CallHearts, CallTrumpDecision.CallHeartsAndGoAlone),
            Suit.Spades => (CallTrumpDecision.CallSpades, CallTrumpDecision.CallSpadesAndGoAlone),
            _ => throw new InvalidOperationException($"Invalid Suit: {suit}")
        };

        decisions.Add(callDecision);
        decisions.Add(callAloneDecision);
    }

    private static Suit ConvertDecisionToSuit(CallTrumpDecision decision)
    {
        return decision switch
        {
            CallTrumpDecision.CallClubs or CallTrumpDecision.CallClubsAndGoAlone => Suit.Clubs,
            CallTrumpDecision.CallDiamonds or CallTrumpDecision.CallDiamondsAndGoAlone => Suit.Diamonds,
            CallTrumpDecision.CallHearts or CallTrumpDecision.CallHeartsAndGoAlone => Suit.Hearts,
            CallTrumpDecision.CallSpades or CallTrumpDecision.CallSpadesAndGoAlone => Suit.Spades,
            CallTrumpDecision.Pass or
            CallTrumpDecision.OrderItUp or
            CallTrumpDecision.OrderItUpAndGoAlone or
            _ => throw new ArgumentOutOfRangeException(nameof(decision), $"Cannot convert {decision} to Suit")
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

    private async Task<bool> ExecuteRound1Async(Deal deal)
    {
        var validDecisions = GetValidRound1Decisions();
        var currentPosition = GetFirstPlayerPosition(deal);

        for (int i = 0; i < PlayersPerDeal; i++)
        {
            var decision = await GetAndValidatePlayerDecisionAsync(deal, currentPosition, validDecisions);

            if (decision != CallTrumpDecision.Pass)
            {
                SetTrumpResult(deal, deal.UpCard!.Suit, currentPosition, decision);
                await HandleDealerDiscardAsync(deal);
                return true;
            }

            currentPosition = currentPosition.GetNextPosition();
        }

        return false;
    }

    private async Task ExecuteRound2Async(Deal deal)
    {
        var upcardSuit = deal.UpCard!.Suit;
        var currentPosition = GetFirstPlayerPosition(deal);

        for (int i = 0; i < PlayersPerDeal; i++)
        {
            bool isDealer = IsDealer(deal, currentPosition);
            var validDecisions = GetValidRound2Decisions(upcardSuit, isDealer);

            var decision = await GetAndValidatePlayerDecisionAsync(deal, currentPosition, validDecisions);

            if (decision != CallTrumpDecision.Pass)
            {
                var trump = ConvertDecisionToSuit(decision);
                SetTrumpResult(deal, trump, currentPosition, decision);
                return;
            }

            currentPosition = currentPosition.GetNextPosition();
        }

        if (gameOptions.Value.StickTheDealer)
        {
            throw new InvalidOperationException("Dealer must call trump in Round 2, but all players passed");
        }
    }

    private Task<CallTrumpDecision> GetPlayerDecisionAsync(
        Deal deal,
        PlayerPosition playerPosition,
        CallTrumpDecision[] validDecisions)
    {
        var player = deal.Players[playerPosition];
        var playerActor = GetPlayerActor(player);

        var (teamScore, opponentScore) = GetScores(deal, playerPosition);

        return playerActor.CallTrumpAsync(
            [.. player.CurrentHand],
            deal.UpCard!,
            GetRelativeDealerPosition(deal, playerPosition),
            teamScore,
            opponentScore,
            validDecisions);
    }

    private async Task<CallTrumpDecision> GetAndValidatePlayerDecisionAsync(
        Deal deal,
        PlayerPosition playerPosition,
        CallTrumpDecision[] validDecisions)
    {
        var decision = await GetPlayerDecisionAsync(deal, playerPosition, validDecisions);
        ValidateDecision(decision, validDecisions);
        return decision;
    }

    private async Task HandleDealerDiscardAsync(Deal deal)
    {
        var dealerPosition = deal.DealerPosition!.Value;
        var dealer = deal.Players[dealerPosition];

        AddUpcardToDealerHand(dealer, deal.UpCard!);

        var relativeHand = ConvertToRelativeCards(dealer.CurrentHand, deal.Trump!.Value);
        var cardToDiscard = await GetDealerDiscardDecisionAsync(deal, dealerPosition, dealer, relativeHand);

        ValidateDiscard(cardToDiscard, relativeHand);

        dealer.CurrentHand.Remove(cardToDiscard.Card);
    }

    private Task<RelativeCard> GetDealerDiscardDecisionAsync(
        Deal deal,
        PlayerPosition dealerPosition,
        DealPlayer dealer,
        RelativeCard[] relativeHand)
    {
        var dealerActor = GetPlayerActor(dealer);
        var (teamScore, opponentScore) = GetScores(deal, dealerPosition);

        return dealerActor.DiscardCardAsync(
            [.. relativeHand],
            deal.ToRelative(dealerPosition),
            teamScore,
            opponentScore,
            [.. relativeHand]);
    }

    private IPlayerActor GetPlayerActor(DealPlayer player)
    {
        return _playerActors[player.ActorType!.Value];
    }

    private CallTrumpDecision[] GetValidRound2Decisions(Suit upcardSuit, bool isDealer)
    {
        var decisions = new List<CallTrumpDecision>();

        if (!isDealer || !gameOptions.Value.StickTheDealer)
        {
            decisions.Add(CallTrumpDecision.Pass);
        }

        foreach (var suit in Enum.GetValues<Suit>().Where(suit => suit != upcardSuit))
        {
            AddSuitDecisions(decisions, suit);
        }

        return [.. decisions];
    }
}
