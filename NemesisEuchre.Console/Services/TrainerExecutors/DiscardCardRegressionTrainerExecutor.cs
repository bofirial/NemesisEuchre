using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public class DiscardCardRegressionTrainerExecutor(
    IModelTrainer<DiscardCardTrainingData> trainer,
    ITrainingDataLoader<DiscardCardTrainingData> dataLoader,
    IIdvFileService idvFileService,
    ILogger<DiscardCardRegressionTrainerExecutor> logger) :
    RegressionTrainerExecutorBase<DiscardCardTrainingData>(trainer, dataLoader, idvFileService, logger)
{
    public override string ModelType => "DiscardCardRegression";

    public override DecisionType DecisionType => DecisionType.Discard;
}
