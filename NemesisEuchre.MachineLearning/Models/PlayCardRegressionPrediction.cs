using Microsoft.ML.Data;

namespace NemesisEuchre.MachineLearning.Models;

public class PlayCardRegressionPrediction
{
    [ColumnName("Score")]
    public float PredictedPoints { get; set; }
}
