using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.GameEngine;

public interface IDealResultCalculator
{
    (DealResult DealResult, Team WinningTeam) CalculateDealResult(Deal deal);
}

public class DealResultCalculator(IDealResultValidator validator) : IDealResultCalculator
{
    private const int MinimumTricksToWinBid = 3;
    private const int AllTricks = 5;

    public (DealResult DealResult, Team WinningTeam) CalculateDealResult(Deal deal)
    {
        validator.ValidateDeal(deal);

        var callingTeam = deal.CallingPlayer!.Value.GetTeam();
        var callingTeamTricks = CountTricksWonByTeam(deal, callingTeam);
        var dealResult = DetermineResult(callingTeamTricks, deal.CallingPlayerIsGoingAlone);
        var winningTeam = DetermineWinningTeam(callingTeam, dealResult);

        return (dealResult, winningTeam);
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
