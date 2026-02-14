using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public sealed class DiscardCardFeatureEngineer(DiscardCardFeatureBuilder builder)
    : FeatureEngineerBase<DiscardCardDecisionEntity, DiscardCardTrainingData>(builder);
