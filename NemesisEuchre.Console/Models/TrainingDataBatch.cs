using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.Console.Models;

public record TrainingDataBatch(
    List<PlayCardTrainingData> PlayCardData,
    List<CallTrumpTrainingData> CallTrumpData,
    List<DiscardCardTrainingData> DiscardCardData);
