using Microsoft.ML.Data;

namespace NemesisEuchre.MachineLearning.Models;

public class AdvancedPlayCardTrainingData : PlayCardTrainingDataBase
{
    [LoadColumn(24)]
    public float ChosenCardRelativeSuit { get; set; }

    [LoadColumn(25)]
    public float CallingPlayerPosition { get; set; }

    [LoadColumn(26)]
    public float CallingPlayerGoingAlone { get; set; }

    [LoadColumn(27)]
    public float DealerPlayerPosition { get; set; }

    [LoadColumn(28)]
    public float DealerPickedUpCardRank { get; set; }

    [LoadColumn(29)]
    public float DealerPickedUpCardSuit { get; set; }

    [LoadColumn(66)]
    public float WonTricks { get; set; }

    [LoadColumn(67)]
    public float OpponentsWonTricks { get; set; }

    [LoadColumn(68)]
    [ColumnName("Label")]
    public float ExpectedDealPoints { get; set; }

    [LoadColumn(69)]
    public float Card1Threats { get; set; }

    [LoadColumn(70)]
    public float Card2Threats { get; set; }

    [LoadColumn(71)]
    public float Card3Threats { get; set; }

    [LoadColumn(72)]
    public float Card4Threats { get; set; }

    [LoadColumn(73)]
    public float Card5Threats { get; set; }

    [LoadColumn(74)]
    public float Card1ThreatsThisTrick { get; set; }

    [LoadColumn(75)]
    public float Card2ThreatsThisTrick { get; set; }

    [LoadColumn(76)]
    public float Card3ThreatsThisTrick { get; set; }

    [LoadColumn(77)]
    public float Card4ThreatsThisTrick { get; set; }

    [LoadColumn(78)]
    public float Card5ThreatsThisTrick { get; set; }

    [LoadColumn(79)]
    public float ChosenCardThreats { get; set; }

    [LoadColumn(80)]
    public float ChosenCardThreatsThisTrick { get; set; }
}
