using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public sealed class PlayCardFeatureEngineer(PlayCardFeatureBuilder builder)
    : FeatureEngineerBase<PlayCardDecisionEntity, PlayCardTrainingData>(builder);
