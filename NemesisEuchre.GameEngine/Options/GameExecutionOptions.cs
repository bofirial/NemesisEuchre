using System.ComponentModel.DataAnnotations;

namespace NemesisEuchre.GameEngine.Options;

public class GameExecutionOptions
{
    [Range(1, int.MaxValue, ErrorMessage = "MaxDegreeOfParallelism must be at least 1")]
    public int MaxDegreeOfParallelism { get; set; } = 4;
}
