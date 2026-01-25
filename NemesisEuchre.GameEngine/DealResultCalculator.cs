using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public class DealResultCalculator : IDealResultCalculator
{
    private const int TotalTricksInDeal = 5;
    private const int MinimumTricksToWinBid = 3;
    private const int AllTricks = 5;

    public (DealResult DealResult, Team WinningTeam) CalculateDealResult(Deal deal)
    {
        ValidateDeal(deal);

        var callingTeam = deal.CallingPlayer!.Value.GetTeam();
        var callingTeamTricks = CountTricksWonByTeam(deal, callingTeam);
        var dealResult = DetermineResult(callingTeamTricks, deal.CallingPlayerIsGoingAlone);
        var winningTeam = DetermineWinningTeam(callingTeam, dealResult);

        return (dealResult, winningTeam);
    }

    private static void ValidateDeal(Deal deal)
    {
        ArgumentNullException.ThrowIfNull(deal);
        ValidateCompletedTricksCount(deal);
        ValidateCallingPlayerExists(deal);
        ValidateAllTricksHaveWinningTeam(deal);
    }

    private static void ValidateCompletedTricksCount(Deal deal)
    {
        if (deal.CompletedTricks.Count != TotalTricksInDeal)
        {
            throw new InvalidOperationException(
                $"Deal must have exactly {TotalTricksInDeal} completed tricks");
        }
    }

    private static void ValidateCallingPlayerExists(Deal deal)
    {
        if (deal.CallingPlayer is null)
        {
            throw new InvalidOperationException("Deal must have a CallingPlayer set");
        }
    }

    private static void ValidateAllTricksHaveWinningTeam(Deal deal)
    {
        if (deal.CompletedTricks.Any(trick => trick.WinningTeam is null))
        {
            throw new InvalidOperationException(
                "All completed tricks must have WinningTeam set");
        }
    }

    private static int CountTricksWonByTeam(Deal deal, Team team)
    {
        return deal.CompletedTricks.Count(trick => trick.WinningTeam == team);
    }

    private static DealResult DetermineResult(int callingTeamTricks, bool isGoingAlone)
    {
        return callingTeamTricks switch
        {
            AllTricks when isGoingAlone => DealResult.WonAndWentAlone,
            AllTricks => DealResult.WonGotAllTricks,
            >= MinimumTricksToWinBid => DealResult.WonStandardBid,
            _ => DealResult.OpponentsEuchred
        };
    }

    private static Team DetermineWinningTeam(Team callingTeam, DealResult dealResult)
    {
        return dealResult == DealResult.OpponentsEuchred
            ? GetOpposingTeam(callingTeam)
            : callingTeam;
    }

    private static Team GetOpposingTeam(Team team)
    {
        return team == Team.Team1 ? Team.Team2 : Team.Team1;
    }
}
