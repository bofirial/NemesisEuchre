using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public class DiscardCardRegressionTrainerExecutor(
    IModelTrainer<DiscardCardTrainingData> trainer,
    IIdvFileService idvFileService,
    IServiceProvider serviceProvider,
    ILogger<DiscardCardRegressionTrainerExecutor> logger) :
    RegressionTrainerExecutorBase<DiscardCardTrainingData>(trainer, idvFileService, serviceProvider, logger)
{
    public override string ModelType => "DiscardCard";

    public override DecisionType DecisionType => DecisionType.Discard;

    protected override Type GetTrainerType()
    {
        return typeof(DiscardCardRegressionModelTrainer);
    }
}
