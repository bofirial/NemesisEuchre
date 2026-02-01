using Microsoft.ML.Data;

namespace NemesisEuchre.MachineLearning.Models;

public class DiscardCardRegressionPrediction
{
    [ColumnName("Score")]
    public float PredictedPoints { get; set; }
}
