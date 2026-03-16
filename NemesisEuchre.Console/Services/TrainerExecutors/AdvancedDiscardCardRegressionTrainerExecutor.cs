using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public class AdvancedDiscardCardRegressionTrainerExecutor(
    IModelTrainer<AdvancedDiscardCardTrainingData> trainer,
    IIdvFileService idvFileService,
    IServiceProvider serviceProvider,
    ILogger<AdvancedDiscardCardRegressionTrainerExecutor> logger,
    IOptions<MachineLearningOptions> options,
    MLContext mlContext) :
    RegressionTrainerExecutorBase<AdvancedDiscardCardTrainingData>(trainer, idvFileService, serviceProvider, logger, options, mlContext)
{
    public override string ModelType => "AdvancedDiscardCard";

    public override DecisionType DecisionType => DecisionType.AdvancedDiscard;

    protected override Type GetTrainerType()
    {
        return typeof(AdvancedDiscardCardRegressionModelTrainer);
    }
}
