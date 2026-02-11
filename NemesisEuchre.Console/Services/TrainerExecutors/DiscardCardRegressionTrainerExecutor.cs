using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public class DiscardCardRegressionTrainerExecutor(
    IModelTrainer<DiscardCardTrainingData> trainer,
    IIdvFileService idvFileService,
    ILogger<DiscardCardRegressionTrainerExecutor> logger) :
    RegressionTrainerExecutorBase<DiscardCardTrainingData>(trainer, idvFileService, logger)
{
    public override string ModelType => "DiscardCardRegression";

    public override DecisionType DecisionType => DecisionType.Discard;
}
