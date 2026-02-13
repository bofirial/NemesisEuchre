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
    public float DealerPlayerPosition { get; set; }

    [LoadColumn(31)]
    public float DealerPickedUpCardRank { get; set; }

    [LoadColumn(32)]
    public float DealerPickedUpCardSuit { get; set; }

    [LoadColumn(33)]
    public float LeftHandOpponentMayHaveTrump { get; set; }

    [LoadColumn(34)]
    public float LeftHandOpponentMayHaveNonTrumpSameColor { get; set; }

    [LoadColumn(35)]
    public float LeftHandOpponentMayHaveNonTrumpOppositeColor1 { get; set; }

    [LoadColumn(36)]
    public float LeftHandOpponentMayHaveNonTrumpOppositeColor2 { get; set; }

    [LoadColumn(37)]
    public float PartnerMayHaveTrump { get; set; }

    [LoadColumn(38)]
    public float PartnerMayHaveNonTrumpSameColor { get; set; }

    [LoadColumn(39)]
    public float PartnerMayHaveNonTrumpOppositeColor1 { get; set; }

    [LoadColumn(40)]
    public float PartnerMayHaveNonTrumpOppositeColor2 { get; set; }

    [LoadColumn(41)]
    public float RightHandOpponentMayHaveTrump { get; set; }

    [LoadColumn(42)]
    public float RightHandOpponentMayHaveNonTrumpSameColor { get; set; }

    [LoadColumn(43)]
    public float RightHandOpponentMayHaveNonTrumpOppositeColor1 { get; set; }

    [LoadColumn(44)]
    public float RightHandOpponentMayHaveNonTrumpOppositeColor2 { get; set; }

    [LoadColumn(45)]
    public float RightBowerOfTrumpHasBeenAccountedFor { get; set; }

    [LoadColumn(46)]
    public float LeftBowerOfTrumpHasBeenAccountedFor { get; set; }

    [LoadColumn(47)]
    public float AceOfTrumpHasBeenAccountedFor { get; set; }

    [LoadColumn(48)]
    public float KingOfTrumpHasBeenAccountedFor { get; set; }

    [LoadColumn(49)]
    public float QueenOfTrumpHasBeenAccountedFor { get; set; }

    [LoadColumn(50)]
    public float TenOfTrumpHasBeenAccountedFor { get; set; }

    [LoadColumn(51)]
    public float NineOfTrumpHasBeenAccountedFor { get; set; }

    [LoadColumn(52)]
    public float AceOfNonTrumpSameColorHasBeenAccountedFor { get; set; }

    [LoadColumn(53)]
    public float KingOfNonTrumpSameColorHasBeenAccountedFor { get; set; }

    [LoadColumn(54)]
    public float QueenOfNonTrumpSameColorHasBeenAccountedFor { get; set; }

    [LoadColumn(55)]
    public float TenOfNonTrumpSameColorHasBeenAccountedFor { get; set; }

    [LoadColumn(56)]
    public float NineOfNonTrumpSameColorHasBeenAccountedFor { get; set; }

    [LoadColumn(57)]
    public float AceOfNonTrumpOppositeColor1HasBeenAccountedFor { get; set; }

    [LoadColumn(58)]
    public float KingOfNonTrumpOppositeColor1HasBeenAccountedFor { get; set; }

    [LoadColumn(59)]
    public float QueenOfNonTrumpOppositeColor1HasBeenAccountedFor { get; set; }

    [LoadColumn(60)]
    public float JackOfNonTrumpOppositeColor1HasBeenAccountedFor { get; set; }

    [LoadColumn(61)]
    public float TenOfNonTrumpOppositeColor1HasBeenAccountedFor { get; set; }

    [LoadColumn(62)]
    public float NineOfNonTrumpOppositeColor1HasBeenAccountedFor { get; set; }

    [LoadColumn(63)]
    public float AceOfNonTrumpOppositeColor2HasBeenAccountedFor { get; set; }

    [LoadColumn(64)]
    public float KingOfNonTrumpOppositeColor2HasBeenAccountedFor { get; set; }

    [LoadColumn(65)]
    public float QueenOfNonTrumpOppositeColor2HasBeenAccountedFor { get; set; }

    [LoadColumn(66)]
    public float JackOfNonTrumpOppositeColor2HasBeenAccountedFor { get; set; }

    [LoadColumn(67)]
    public float TenOfNonTrumpOppositeColor2HasBeenAccountedFor { get; set; }

    [LoadColumn(68)]
    public float NineOfNonTrumpOppositeColor2HasBeenAccountedFor { get; set; }

    [LoadColumn(69)]
    public float Card1Chosen { get; set; }

    [LoadColumn(70)]
    public float Card2Chosen { get; set; }

    [LoadColumn(71)]
    public float Card3Chosen { get; set; }

    [LoadColumn(72)]
    public float Card4Chosen { get; set; }

    [LoadColumn(73)]
    public float Card5Chosen { get; set; }

    [LoadColumn(74)]
    public float WonTricks { get; set; }

    [LoadColumn(75)]
    public float OpponentsWonTricks { get; set; }

    [LoadColumn(76)]
    [ColumnName("Label")]
    public float ExpectedDealPoints { get; set; }
}
