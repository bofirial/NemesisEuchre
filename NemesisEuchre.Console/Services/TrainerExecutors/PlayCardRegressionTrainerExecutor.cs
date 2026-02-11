using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public class PlayCardRegressionTrainerExecutor(
    IModelTrainer<PlayCardTrainingData> trainer,
    IIdvFileService idvFileService,
    ILogger<PlayCardRegressionTrainerExecutor> logger) :
    RegressionTrainerExecutorBase<PlayCardTrainingData>(trainer, idvFileService, logger)
{
    public override string ModelType => "PlayCardRegression";

    public override DecisionType DecisionType => DecisionType.Play;
}
