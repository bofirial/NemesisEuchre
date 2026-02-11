using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public class CallTrumpRegressionTrainerExecutor(
    IModelTrainer<CallTrumpTrainingData> trainer,
    IIdvFileService idvFileService,
    ILogger<CallTrumpRegressionTrainerExecutor> logger) :
    RegressionTrainerExecutorBase<CallTrumpTrainingData>(trainer, idvFileService, logger)
{
    public override string ModelType => "CallTrumpRegression";

    public override DecisionType DecisionType => DecisionType.CallTrump;
}
