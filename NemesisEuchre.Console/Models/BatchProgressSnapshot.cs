namespace NemesisEuchre.Console.Models;

public record BatchProgressSnapshot(
    int CompletedGames,
    int Team1Wins,
    int Team2Wins,
    int FailedGames,
    int TotalDeals,
    int TotalTricks,
    int TotalCallTrumpDecisions,
    int TotalDiscardCardDecisions,
    int TotalPlayCardDecisions)
{
    public static BatchProgressSnapshot Empty => new(0, 0, 0, 0, 0, 0, 0, 0, 0);

    public BatchProgressSnapshot Add(BatchProgressSnapshot other)
    {
        return new(
            CompletedGames + other.CompletedGames,
            Team1Wins + other.Team1Wins,
            Team2Wins + other.Team2Wins,
            FailedGames + other.FailedGames,
            TotalDeals + other.TotalDeals,
            TotalTricks + other.TotalTricks,
            TotalCallTrumpDecisions + other.TotalCallTrumpDecisions,
            TotalDiscardCardDecisions + other.TotalDiscardCardDecisions,
            TotalPlayCardDecisions + other.TotalPlayCardDecisions);
    }
}
