using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public class PlayCardRegressionTrainerExecutor(
    IModelTrainer<PlayCardTrainingData> trainer,
    IIdvFileService idvFileService,
    IServiceProvider serviceProvider,
    ILogger<PlayCardRegressionTrainerExecutor> logger) :
    RegressionTrainerExecutorBase<PlayCardTrainingData>(trainer, idvFileService, serviceProvider, logger)
{
    public override string ModelType => "PlayCard";

    public override DecisionType DecisionType => DecisionType.Play;

    protected override Type GetTrainerType()
    {
        return typeof(PlayCardRegressionModelTrainer);
    }
}
