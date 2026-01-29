using System.ComponentModel.DataAnnotations;

namespace NemesisEuchre.GameEngine.Options;

public class GameExecutionOptions
{
    public int MaxDegreeOfParallelism { get; set; }

    public ParallelismStrategy Strategy { get; set; } = ParallelismStrategy.Auto;

    [Range(1, 128, ErrorMessage = "MaxThreads must be between 1 and 128")]
    public int MaxThreads { get; set; } = 64;

    [Range(0, 16, ErrorMessage = "ReservedCores must be between 0 and 16")]
    public int ReservedCores { get; set; } = 1;
}

public enum ParallelismStrategy
{
    Fixed = 0,
    Auto = 1,
    Conservative = 2,
}
