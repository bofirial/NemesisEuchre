using System.ComponentModel.DataAnnotations;

namespace NemesisEuchre.DataAccess.Options;

public class PersistenceOptions
{
    [Range(1, int.MaxValue, ErrorMessage = "BatchSize must be at least 1")]
    public int BatchSize { get; set; } = 100;

    [Range(1, int.MaxValue, ErrorMessage = "MaxBatchSizeForChangeTracking must be at least 1")]
    public int MaxBatchSizeForChangeTracking { get; set; } = 50;
}
