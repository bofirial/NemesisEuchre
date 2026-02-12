using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.Console.Services;

public partial class GameToTrainingDataConverter
{
    public record GameConversionResult(
        List<PlayCardTrainingData> PlayCardData,
        List<CallTrumpTrainingData> CallTrumpData,
        List<DiscardCardTrainingData> DiscardCardData,
        int DealCount,
        int TrickCount,
        HashSet<Actor> Actors,
        int ErrorCount);
}
