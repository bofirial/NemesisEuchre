using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public class CallTrumpRegressionTrainerExecutor(
    IModelTrainer<CallTrumpTrainingData> trainer,
    ITrainingDataLoader<CallTrumpTrainingData> dataLoader,
    ILogger<CallTrumpRegressionTrainerExecutor> logger) :
    RegressionTrainerExecutorBase<CallTrumpTrainingData>(trainer, dataLoader, logger)
{
    public override string ModelType => "CallTrumpRegression";

    public override DecisionType DecisionType => DecisionType.CallTrump;
}
