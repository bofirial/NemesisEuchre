using Microsoft.ML.Data;

namespace NemesisEuchre.MachineLearning.Models;

public class DiscardCardTrainingData
{
    [LoadColumn(0)]
    public float Card1Rank { get; set; }

    [LoadColumn(1)]
    public float Card1Suit { get; set; }

    [LoadColumn(2)]
    public float Card2Rank { get; set; }

    [LoadColumn(3)]
    public float Card2Suit { get; set; }

    [LoadColumn(4)]
    public float Card3Rank { get; set; }

    [LoadColumn(5)]
    public float Card3Suit { get; set; }

    [LoadColumn(6)]
    public float Card4Rank { get; set; }

    [LoadColumn(7)]
    public float Card4Suit { get; set; }

    [LoadColumn(8)]
    public float Card5Rank { get; set; }

    [LoadColumn(9)]
    public float Card5Suit { get; set; }

    [LoadColumn(10)]
    public float Card6Rank { get; set; }

    [LoadColumn(11)]
    public float Card6Suit { get; set; }

    [LoadColumn(12)]
    public float CallingPlayerPosition { get; set; }

    [LoadColumn(13)]
    public float CallingPlayerGoingAlone { get; set; }

    [LoadColumn(14)]
    public float TeamScore { get; set; }

    [LoadColumn(15)]
    public float OpponentScore { get; set; }

    [LoadColumn(16)]
    public float Card1Chosen { get; set; }

    [LoadColumn(17)]
    public float Card2Chosen { get; set; }

    [LoadColumn(18)]
    public float Card3Chosen { get; set; }

    [LoadColumn(19)]
    public float Card4Chosen { get; set; }

    [LoadColumn(20)]
    public float Card5Chosen { get; set; }

    [LoadColumn(21)]
    public float Card6Chosen { get; set; }

    [LoadColumn(22)]
    [ColumnName("Label")]
    public float ExpectedDealPoints { get; set; }
}
