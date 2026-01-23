using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerBots;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine;

public class TrumpSelectionOrchestrator(IEnumerable<IPlayerBot> playerBots) : ITrumpSelectionOrchestrator
{
    private readonly Dictionary<BotType, IPlayerBot> _playerBots = playerBots.ToDictionary(playerBot => playerBot.BotType, playerBot => playerBot);

    public async Task SelectTrumpAsync(Deal deal)
    {
        ValidatePreconditions(deal);

        bool trumpSelected = await ExecuteRound1Async(deal);

        if (!trumpSelected)
        {
            await ExecuteRound2Async(deal);
        }

        ValidatePostConditions(deal);
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

        if (deal.Players.Count != 4)
        {
            throw new InvalidOperationException($"Deal must have exactly 4 players, but had {deal.Players.Count}");
        }
    }

    private static void ValidatePostConditions(Deal deal)
    {
        if (deal.Trump == null)
        {
            throw new InvalidOperationException("Trump must be set after trump selection");
        }

        if (deal.CallingPlayer == null)
        {
            throw new InvalidOperationException("CallingPlayer must be set after trump selection");
        }
    }

    private static CallTrumpDecision[] GetValidRound1Decisions()
    {
        return [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp, CallTrumpDecision.OrderItUpAndGoAlone];
    }

    private static CallTrumpDecision[] GetValidRound2Decisions(Suit upcardSuit, bool isDealer)
    {
        var decisions = new List<CallTrumpDecision>();

        if (!isDealer)
        {
            decisions.Add(CallTrumpDecision.Pass);
        }

        foreach (var suit in from suit in Enum.GetValues<Suit>()
                             where suit != upcardSuit
                             select suit)
        {
            switch (suit)
            {
                case Suit.Clubs:
                    decisions.Add(CallTrumpDecision.CallClubs);
                    decisions.Add(CallTrumpDecision.CallClubsAndGoAlone);
                    break;
                case Suit.Diamonds:
                    decisions.Add(CallTrumpDecision.CallDiamonds);
                    decisions.Add(CallTrumpDecision.CallDiamondsAndGoAlone);
                    break;
                case Suit.Hearts:
                    decisions.Add(CallTrumpDecision.CallHearts);
                    decisions.Add(CallTrumpDecision.CallHeartsAndGoAlone);
                    break;
                case Suit.Spades:
                    decisions.Add(CallTrumpDecision.CallSpades);
                    decisions.Add(CallTrumpDecision.CallSpadesAndGoAlone);
                    break;
                default:
                    throw new InvalidOperationException("Invalid Suit");
            }
        }

        return [.. decisions];
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
        var currentPosition = deal.DealerPosition!.Value.GetNextPosition();
        var validDecisions = GetValidRound1Decisions();

        for (int i = 0; i < 4; i++)
        {
            var decision = await GetPlayerDecisionAsync(deal, currentPosition, [.. validDecisions]);

            if (!validDecisions.Contains(decision))
            {
                throw new InvalidOperationException("CallTrumpDecision was not included in ValidDecisions");
            }

            if (decision != CallTrumpDecision.Pass)
            {
                deal.Trump = deal.UpCard!.Suit;
                deal.CallingPlayer = currentPosition;
                deal.CallingPlayerIsGoingAlone = decision == CallTrumpDecision.OrderItUpAndGoAlone;

                await HandleDealerDiscardAsync(deal);

                return true;
            }

            currentPosition = currentPosition.GetNextPosition();
        }

        return false;
    }

    private async Task ExecuteRound2Async(Deal deal)
    {
        var currentPosition = deal.DealerPosition!.Value.GetNextPosition();
        var upcardSuit = deal.UpCard!.Suit;

        for (int i = 0; i < 4; i++)
        {
            bool isDealer = currentPosition == deal.DealerPosition.Value;
            var validDecisions = GetValidRound2Decisions(upcardSuit, isDealer);

            var decision = await GetPlayerDecisionAsync(deal, currentPosition, [.. validDecisions]);

            if (!validDecisions.Contains(decision))
            {
                throw new InvalidOperationException("CallTrumpDecision was not included in ValidDecisions");
            }

            if (decision != CallTrumpDecision.Pass)
            {
                deal.Trump = ConvertDecisionToSuit(decision);
                deal.CallingPlayer = currentPosition;
                deal.CallingPlayerIsGoingAlone = IsGoingAloneDecision(decision);

                return;
            }

            currentPosition = currentPosition.GetNextPosition();
        }

        throw new InvalidOperationException("Dealer must call trump in Round 2, but all players passed");
    }

    private Task<CallTrumpDecision> GetPlayerDecisionAsync(
        Deal deal,
        PlayerPosition playerPosition,
        CallTrumpDecision[] validDecisions)
    {
        var player = deal.Players[playerPosition];
        var playerBot = _playerBots[player.BotType!.Value];

        var cardsInHand = player.CurrentHand.ToArray();
        var upCard = deal.UpCard!;
        var dealerPosition = deal.DealerPosition!.Value;
        var relativeDealerPosition = dealerPosition.ToRelativePosition(playerPosition);

        var teamScore = playerPosition.GetTeam() == Team.Team1 ? deal.Team1Score : deal.Team2Score;
        var opponentScore = playerPosition.GetTeam() == Team.Team1 ? deal.Team2Score : deal.Team1Score;

        return playerBot.CallTrumpAsync(
            cardsInHand,
            upCard,
            relativeDealerPosition,
            teamScore,
            opponentScore,
            validDecisions);
    }

    private async Task HandleDealerDiscardAsync(Deal deal)
    {
        var dealerPosition = deal.DealerPosition!.Value;
        var dealer = deal.Players[dealerPosition];

        dealer.CurrentHand.Add(deal.UpCard!);

        var dealerBot = _playerBots[dealer.BotType!.Value];

        var trump = deal.Trump!.Value;
        var relativeHand = dealer.CurrentHand
            .Select(c => c.ToRelative(trump))
            .ToArray();

        var teamScore = dealerPosition.GetTeam() == Team.Team1 ? deal.Team1Score : deal.Team2Score;
        var opponentScore = dealerPosition.GetTeam() == Team.Team1 ? deal.Team2Score : deal.Team1Score;

        var cardToDiscard = await dealerBot.DiscardCardAsync(
            [.. relativeHand],
            deal.ToRelative(dealerPosition),
            teamScore,
            opponentScore,
            [.. relativeHand]);

        if (!relativeHand.Contains(cardToDiscard))
        {
            throw new InvalidOperationException("CardToDiscard was not included in ValidCardsToDiscard");
        }

        dealer.CurrentHand.Remove(cardToDiscard.Card);
    }
}
