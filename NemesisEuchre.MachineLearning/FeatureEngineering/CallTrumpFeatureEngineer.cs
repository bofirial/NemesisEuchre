using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public sealed class CallTrumpFeatureEngineer(CallTrumpFeatureBuilder builder)
    : FeatureEngineerBase<CallTrumpDecisionEntity, AllCallTrumpTrainingData>(builder);
