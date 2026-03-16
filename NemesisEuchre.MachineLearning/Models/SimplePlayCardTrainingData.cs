using Microsoft.ML.Data;

namespace NemesisEuchre.MachineLearning.Models;

public class SimplePlayCardTrainingData : PlayCardTrainingDataBase
{
    [LoadColumn(68)]
    [ColumnName("Label")]
    public float ExpectedDealPoints { get; set; }
}
