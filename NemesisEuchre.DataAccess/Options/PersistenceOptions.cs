namespace NemesisEuchre.DataAccess.Options;

public class PersistenceOptions
{
    public int BatchSize { get; set; } = 100;

    public int MaxBatchSizeForChangeTracking { get; set; } = 50;
}
