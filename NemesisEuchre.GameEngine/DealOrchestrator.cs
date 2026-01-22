using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public class DealOrchestrator(
    ITrumpSelectionOrchestrator trumpSelectionOrchestrator,
    ITrickPlayingOrchestrator trickPlayingOrchestrator,
    ITrickWinnerCalculator trickWinnerCalculator,
    IDealResultCalculator dealResultCalculator) : IDealOrchestrator
{
    public async Task OrchestrateDealAsync(Deal deal)
    {
        ValidateDealPreconditions(deal);

        deal.DealStatus = DealStatus.SelectingTrump;
        await trumpSelectionOrchestrator.SelectTrumpAsync(deal);
        ValidateTrumpSelected(deal);

        deal.DealStatus = DealStatus.Playing;
        await PlayAllTricksAsync(deal);
        ValidateAllTricksPlayed(deal);

        deal.DealStatus = DealStatus.Scoring;
        (deal.DealResult, deal.WinningTeam) = dealResultCalculator.CalculateDealResult(deal);
        ValidateDealCompleted(deal);

        deal.DealStatus = DealStatus.Complete;
    }

    private static void ValidateDealPreconditions(Deal deal)
    {
        ArgumentNullException.ThrowIfNull(deal);

        if (deal.DealStatus != DealStatus.NotStarted)
        {
            throw new InvalidOperationException($"Deal must be in NotStarted status, but was {deal.DealStatus}");
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

    private static void ValidateTrumpSelected(Deal deal)
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

    private static void ValidateAllTricksPlayed(Deal deal)
    {
        if (deal.CompletedTricks.Count != 5)
        {
            throw new InvalidOperationException($"Deal must have exactly 5 completed tricks, but had {deal.CompletedTricks.Count}");
        }
    }

    private static void ValidateDealCompleted(Deal deal)
    {
        if (deal.DealResult == null)
        {
            throw new InvalidOperationException("DealResult must be set after scoring");
        }

        if (deal.WinningTeam == null)
        {
            throw new InvalidOperationException("WinningTeam must be set after scoring");
        }
    }

    private async Task PlayAllTricksAsync(Deal deal)
    {
        var leadPosition = deal.DealerPosition!.Value.GetNextPosition();

        for (int trickNumber = 0; trickNumber < 5; trickNumber++)
        {
            var trick = await trickPlayingOrchestrator.PlayTrickAsync(deal, leadPosition);

            var winningPosition = trickWinnerCalculator.CalculateWinner(trick, deal.Trump!.Value);

            deal.CompletedTricks.Add(trick);
            leadPosition = winningPosition;
        }
    }
}
