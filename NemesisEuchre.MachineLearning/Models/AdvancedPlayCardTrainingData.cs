using Microsoft.ML.Data;

namespace NemesisEuchre.MachineLearning.Models;

public class AdvancedPlayCardTrainingData
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
    public float LeftHandOpponentPlayedCardRank { get; set; }

    [LoadColumn(13)]
    public float LeftHandOpponentPlayedCardSuit { get; set; }

    [LoadColumn(14)]
    public float PartnerPlayedCardRank { get; set; }

    [LoadColumn(15)]
    public float PartnerPlayedCardSuit { get; set; }

    [LoadColumn(16)]
    public float RightHandOpponentPlayedCardRank { get; set; }

    [LoadColumn(17)]
    public float RightHandOpponentPlayedCardSuit { get; set; }

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
    public float ChosenCardRank { get; set; }

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
    public float Card1UnaccountedForThreats { get; set; }

    [LoadColumn(70)]
    public float Card2UnaccountedForThreats { get; set; }

    [LoadColumn(71)]
    public float Card3UnaccountedForThreats { get; set; }

    [LoadColumn(72)]
    public float Card4UnaccountedForThreats { get; set; }

    [LoadColumn(73)]
    public float Card5UnaccountedForThreats { get; set; }

    [LoadColumn(74)]
    public float Card1BeatsWinningTrickCard { get; set; }

    [LoadColumn(75)]
    public float Card2BeatsWinningTrickCard { get; set; }

    [LoadColumn(76)]
    public float Card3BeatsWinningTrickCard { get; set; }

    [LoadColumn(77)]
    public float Card4BeatsWinningTrickCard { get; set; }

    [LoadColumn(78)]
    public float Card5BeatsWinningTrickCard { get; set; }

    [LoadColumn(79)]
    public float ChosenCardUnaccountedForThreats { get; set; }

    [LoadColumn(80)]
    public float ChosenCardBeatsWinningTrickCard { get; set; }
}
