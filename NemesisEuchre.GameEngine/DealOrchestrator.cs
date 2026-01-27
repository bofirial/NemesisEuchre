using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.GameEngine;

public interface IDealOrchestrator
{
    Task OrchestrateDealAsync(Deal deal);
}

public class DealOrchestrator(
    ITrumpSelectionOrchestrator trumpSelectionOrchestrator,
    ITrickPlayingOrchestrator trickPlayingOrchestrator,
    ITrickWinnerCalculator trickWinnerCalculator,
    IDealResultCalculator dealResultCalculator,
    IDealValidator validator) : IDealOrchestrator
{
    private const int TricksPerDeal = 5;

    public async Task OrchestrateDealAsync(Deal deal)
    {
        validator.ValidateDealPreconditions(deal);

        await ExecuteTrumpSelectionPhaseAsync(deal);

        if (IsThrowIn(deal))
        {
            HandleThrowInScenario(deal);
            return;
        }

        await ExecuteTrickPlayingPhaseAsync(deal);

        ExecuteScoringPhase(deal);

        deal.DealStatus = DealStatus.Complete;
    }

    private static bool IsThrowIn(Deal deal)
    {
        return deal.Trump == null || deal.CallingPlayer == null;
    }

    private static void HandleThrowInScenario(Deal deal)
    {
        deal.DealResult = DealResult.ThrowIn;
        deal.DealStatus = DealStatus.Complete;
    }

    private Task ExecuteTrumpSelectionPhaseAsync(Deal deal)
    {
        deal.DealStatus = DealStatus.SelectingTrump;

        return trumpSelectionOrchestrator.SelectTrumpAsync(deal);
    }

    private async Task ExecuteTrickPlayingPhaseAsync(Deal deal)
    {
        deal.DealStatus = DealStatus.Playing;

        await PlayAllTricksAsync(deal);

        validator.ValidateAllTricksPlayed(deal);
    }

    private void ExecuteScoringPhase(Deal deal)
    {
        deal.DealStatus = DealStatus.Scoring;

        (deal.DealResult, deal.WinningTeam) = dealResultCalculator.CalculateDealResult(deal);

        validator.ValidateDealCompleted(deal);
    }

    private async Task PlayAllTricksAsync(Deal deal)
    {
        var leadPosition = deal.DealerPosition!.Value.GetNextPosition();

        for (int trickNumber = 0; trickNumber < TricksPerDeal; trickNumber++)
        {
            leadPosition = await PlaySingleTrickAsync(deal, leadPosition);
        }
    }

    private async Task<PlayerPosition> PlaySingleTrickAsync(Deal deal, PlayerPosition leadPosition)
    {
        var trick = await trickPlayingOrchestrator.PlayTrickAsync(deal, leadPosition);
        trick.TrickNumber = (short)(deal.CompletedTricks.Count + 1);

        var winningPosition = trickWinnerCalculator.CalculateWinner(trick, deal.Trump!.Value);

        trick.WinningPosition = winningPosition;
        trick.WinningTeam = winningPosition.GetTeam();

        deal.CompletedTricks.Add(trick);

        return winningPosition;
    }
}
