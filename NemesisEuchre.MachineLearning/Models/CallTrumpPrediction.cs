using Microsoft.ML.Data;

namespace NemesisEuchre.MachineLearning.Models;

public class CallTrumpPrediction
{
    [ColumnName("PredictedLabel")]
    public float PredictedDealPoints { get; set; }

    [ColumnName("Score")]
    public float[] Score { get; set; } = [];
}
