namespace NemesisEuchre.Console.Models;

public class BatchGameResults
{
    public required int TotalGames { get; init; }

    public required int Team1Wins { get; init; }

    public required int Team2Wins { get; init; }

    public required int FailedGames { get; init; }

    public required int TotalDeals { get; init; }

    public required TimeSpan ElapsedTime { get; init; }

    public double Team1WinRate => (Team1Wins + Team2Wins) > 0
        ? (double)Team1Wins / (Team1Wins + Team2Wins)
        : 0.0;

    public double Team2WinRate => (Team1Wins + Team2Wins) > 0
        ? (double)Team2Wins / (Team1Wins + Team2Wins)
        : 0.0;
}
