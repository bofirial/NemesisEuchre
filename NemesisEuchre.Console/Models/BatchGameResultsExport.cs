using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Models;

public record BatchGameResultsExport(
    int TotalGames,
    int Team1Wins,
    int Team2Wins,
    int FailedGames,
    double Team1WinRate,
    double Team2WinRate,
    int TotalDeals,
    int TotalTricks,
    int TotalCallTrumpDecisions,
    int TotalDiscardCardDecisions,
    int TotalPlayCardDecisions,
    TimeSpan ElapsedTime,
    TimeSpan? PlayingDuration,
    TimeSpan? PersistenceDuration,
    TimeSpan? IdvSaveDuration,
    double Throughput,
    DateTime GeneratedAtUtc,
    Dictionary<string, TeamConfiguration> TeamConfigurations)
{
    public static BatchGameResultsExport FromBatchResults(
        BatchGameResults results,
        Actor[]? team1Actors,
        Actor[]? team2Actors)
    {
        var throughput = results.ElapsedTime.TotalSeconds > 0
            ? results.TotalGames / results.ElapsedTime.TotalSeconds
            : 0.0;

        var teamConfigurations = new Dictionary<string, TeamConfiguration>
        {
            ["team1"] = TeamConfiguration.FromActor(team1Actors?.FirstOrDefault()),
            ["team2"] = TeamConfiguration.FromActor(team2Actors?.FirstOrDefault()),
        };

        return new BatchGameResultsExport(
            TotalGames: results.TotalGames,
            Team1Wins: results.Team1Wins,
            Team2Wins: results.Team2Wins,
            FailedGames: results.FailedGames,
            Team1WinRate: results.Team1WinRate,
            Team2WinRate: results.Team2WinRate,
            TotalDeals: results.TotalDeals,
            TotalTricks: results.TotalTricks,
            TotalCallTrumpDecisions: results.TotalCallTrumpDecisions,
            TotalDiscardCardDecisions: results.TotalDiscardCardDecisions,
            TotalPlayCardDecisions: results.TotalPlayCardDecisions,
            ElapsedTime: results.ElapsedTime,
            PlayingDuration: results.PlayingDuration,
            PersistenceDuration: results.PersistenceDuration,
            IdvSaveDuration: results.IdvSaveDuration,
            Throughput: throughput,
            GeneratedAtUtc: DateTime.UtcNow,
            TeamConfigurations: teamConfigurations);
    }
}
