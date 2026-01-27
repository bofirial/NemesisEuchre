using Microsoft.Extensions.Options;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Handlers;
using NemesisEuchre.GameEngine.Mappers;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Services;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.GameEngine;

public interface ITrumpSelectionOrchestrator
{
    Task SelectTrumpAsync(Deal deal);
}

public class TrumpSelectionOrchestrator(
    IOptions<GameOptions> gameOptions,
    ITrumpSelectionValidator validator,
    ICallTrumpDecisionMapper decisionMapper,
    IDealerDiscardHandler dealerDiscardHandler,
    IPlayerContextBuilder contextBuilder,
    IPlayerActorResolver actorResolver,
    IDecisionRecorder decisionRecorder) : ITrumpSelectionOrchestrator
{
    private const int PlayersPerDeal = 4;
    private byte _decisionOrder;

    public async Task SelectTrumpAsync(Deal deal)
    {
        validator.ValidatePreconditions(deal);

        _decisionOrder = 0;

        bool trumpSelected = await ExecuteRound1Async(deal);

        if (!trumpSelected)
        {
            await ExecuteRound2Async(deal);
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

    private void SetTrumpResult(Deal deal, Suit trump, PlayerPosition callingPlayer, CallTrumpDecision decision)
    {
        deal.Trump = trump;
        deal.CallingPlayer = callingPlayer;
        deal.CallingPlayerIsGoingAlone = decisionMapper.IsGoingAloneDecision(decision);
    }

    private async Task<bool> ExecuteRound1Async(Deal deal)
    {
        var validDecisions = decisionMapper.GetValidRound1Decisions();
        var currentPosition = GetFirstPlayerPosition(deal);

        for (int i = 0; i < PlayersPerDeal; i++)
        {
            var decision = await GetAndValidatePlayerDecisionAsync(deal, currentPosition, validDecisions);

            decisionRecorder.RecordCallTrumpDecision(deal, currentPosition, validDecisions, decision, ref _decisionOrder);

            if (decision != CallTrumpDecision.Pass)
            {
                SetTrumpResult(deal, deal.UpCard!.Suit, currentPosition, decision);
                await dealerDiscardHandler.HandleDealerDiscardAsync(deal);
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
            var validDecisions = decisionMapper.GetValidRound2Decisions(upcardSuit, isDealer, gameOptions.Value.StickTheDealer);

            var decision = await GetAndValidatePlayerDecisionAsync(deal, currentPosition, validDecisions);

            decisionRecorder.RecordCallTrumpDecision(deal, currentPosition, validDecisions, decision, ref _decisionOrder);

            if (decision != CallTrumpDecision.Pass)
            {
                var trump = decisionMapper.ConvertDecisionToSuit(decision);
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
        var playerActor = actorResolver.GetPlayerActor(player);

        var (teamScore, opponentScore) = contextBuilder.GetScores(deal, playerPosition);

        return playerActor.CallTrumpAsync(
            [.. player.CurrentHand],
            playerPosition,
            teamScore,
            opponentScore,
            deal.DealerPosition!.Value,
            deal.UpCard!,
            [.. validDecisions]);
    }

    private async Task<CallTrumpDecision> GetAndValidatePlayerDecisionAsync(
        Deal deal,
        PlayerPosition playerPosition,
        CallTrumpDecision[] validDecisions)
    {
        var decision = await GetPlayerDecisionAsync(deal, playerPosition, validDecisions);
        validator.ValidateDecision(decision, validDecisions);
        return decision;
    }
}
