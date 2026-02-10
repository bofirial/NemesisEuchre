using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public class PlayCardRegressionTrainerExecutor(
    IModelTrainer<PlayCardTrainingData> trainer,
    ITrainingDataLoader<PlayCardTrainingData> dataLoader,
    IIdvFileService idvFileService,
    ILogger<PlayCardRegressionTrainerExecutor> logger) :
    RegressionTrainerExecutorBase<PlayCardTrainingData>(trainer, dataLoader, idvFileService, logger)
{
    public override string ModelType => "PlayCardRegression";

    public override DecisionType DecisionType => DecisionType.Play;
}
