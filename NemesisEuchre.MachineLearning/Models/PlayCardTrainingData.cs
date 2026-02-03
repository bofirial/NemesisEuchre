using Microsoft.ML.Data;

namespace NemesisEuchre.MachineLearning.Models;

public class PlayCardTrainingData
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
    public float LeadPlayer { get; set; }

    [LoadColumn(11)]
    public float LeadSuit { get; set; }

    [LoadColumn(12)]
    public float PlayedCard1Rank { get; set; }

    [LoadColumn(13)]
    public float PlayedCard1Suit { get; set; }

    [LoadColumn(14)]
    public float PlayedCard2Rank { get; set; }

    [LoadColumn(15)]
    public float PlayedCard2Suit { get; set; }

    [LoadColumn(16)]
    public float PlayedCard3Rank { get; set; }

    [LoadColumn(17)]
    public float PlayedCard3Suit { get; set; }

    [LoadColumn(18)]
    public float TeamScore { get; set; }

    [LoadColumn(19)]
    public float OpponentScore { get; set; }

    [LoadColumn(20)]
    public float TrickNumber { get; set; }

    [LoadColumn(21)]
    public float CardsPlayedInTrick { get; set; }

    [LoadColumn(22)]
    public float WinningTrickPlayer { get; set; }

    [LoadColumn(23)]
    public float Card1IsValid { get; set; }

    [LoadColumn(24)]
    public float Card2IsValid { get; set; }

    [LoadColumn(25)]
    public float Card3IsValid { get; set; }

    [LoadColumn(26)]
    public float Card4IsValid { get; set; }

    [LoadColumn(27)]
    public float Card5IsValid { get; set; }

    [LoadColumn(28)]
    public float CallingPlayerPosition { get; set; }

    [LoadColumn(29)]
    public float CallingPlayerGoingAlone { get; set; }

    [LoadColumn(30)]
    public float Card1Chosen { get; set; }

    [LoadColumn(31)]
    public float Card2Chosen { get; set; }

    [LoadColumn(32)]
    public float Card3Chosen { get; set; }

    [LoadColumn(33)]
    public float Card4Chosen { get; set; }

    [LoadColumn(34)]
    public float Card5Chosen { get; set; }

    [LoadColumn(35)]
    [ColumnName("Label")]
    public float ExpectedDealPoints { get; set; }
}
