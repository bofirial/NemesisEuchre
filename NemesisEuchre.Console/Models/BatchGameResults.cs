namespace NemesisEuchre.Console.Models;

public class BatchGameResults
{
    public required int TotalGames { get; init; }

    public required int Team1Wins { get; init; }

    public required int Team2Wins { get; init; }

    public required int FailedGames { get; init; }

    public required int TotalDeals { get; init; }

    public required int TotalTricks { get; init; }

    public required int TotalCallTrumpDecisions { get; init; }

    public required int TotalDiscardCardDecisions { get; init; }

    public required int TotalPlayCardDecisions { get; init; }

    public required TimeSpan ElapsedTime { get; init; }

    public TimeSpan? PlayingDuration { get; init; }

    public TimeSpan? PersistenceDuration { get; init; }

    public TimeSpan? IdvSaveDuration { get; init; }

    public double Team1WinRate => (Team1Wins + Team2Wins) > 0
        ? (double)Team1Wins / (Team1Wins + Team2Wins)
        : 0.0;

    public double Team2WinRate => (Team1Wins + Team2Wins) > 0
        ? (double)Team2Wins / (Team1Wins + Team2Wins)
        : 0.0;
}
