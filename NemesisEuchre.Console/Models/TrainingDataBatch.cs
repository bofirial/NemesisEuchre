using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.Console.Models;

public record TrainingDataBatch(
    List<PlayCardTrainingData> PlayCardData,
    List<CallTrumpTrainingData> CallTrumpData,
    List<DiscardCardTrainingData> DiscardCardData,
    TrainingDataBatchStats Stats);

public record TrainingDataBatchStats(
    int GameCount,
    int DealCount,
    int TrickCount,
    HashSet<Actor> Actors);
