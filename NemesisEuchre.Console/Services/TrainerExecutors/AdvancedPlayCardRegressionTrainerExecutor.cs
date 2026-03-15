using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public class AdvancedPlayCardRegressionTrainerExecutor(
    IModelTrainer<AdvancedPlayCardTrainingData> trainer,
    IIdvFileService idvFileService,
    IServiceProvider serviceProvider,
    ILogger<AdvancedPlayCardRegressionTrainerExecutor> logger,
    IOptions<MachineLearningOptions> options,
    MLContext mlContext) :
    RegressionTrainerExecutorBase<AdvancedPlayCardTrainingData>(trainer, idvFileService, serviceProvider, logger, options, mlContext)
{
    public override string ModelType => "AdvancedPlayCard";

    public override DecisionType DecisionType => DecisionType.AdvancedPlay;

    protected override Type GetTrainerType()
    {
        return typeof(AdvancedPlayCardRegressionModelTrainer);
    }
}
