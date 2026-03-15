using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.Console.Services;

public partial class GameToTrainingDataConverter
{
    public class ActorDecisionLists
    {
        public List<PlayCardTrainingData> PlayCardData { get; } = [];

        public List<CallTrumpTrainingData> CallTrumpData { get; } = [];

        public List<DiscardCardTrainingData> DiscardCardData { get; } = [];

        public HashSet<Actor> Actors { get; } = [];
    }

    public record ActorGameConversionResult(
        Dictionary<string, ActorDecisionLists> ActorData,
        int DealCount,
        int TrickCount,
        int ErrorCount);
}
