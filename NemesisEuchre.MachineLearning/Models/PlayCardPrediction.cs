using Microsoft.ML.Data;

namespace NemesisEuchre.MachineLearning.Models;

public class PlayCardPrediction
{
    [ColumnName("PredictedLabel")]
    public uint PredictedDecisionIndex { get; set; }

    [ColumnName("Score")]
    public float[] Score { get; set; } = [];
}
