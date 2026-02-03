using Microsoft.ML.Data;

namespace NemesisEuchre.MachineLearning.Models;

public class CallTrumpRegressionPrediction
{
    [ColumnName("Score")]
    public float PredictedPoints { get; set; }
}
