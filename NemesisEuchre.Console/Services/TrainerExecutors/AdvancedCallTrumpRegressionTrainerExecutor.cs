using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public class AdvancedCallTrumpRegressionTrainerExecutor(
    IModelTrainer<AdvancedCallTrumpTrainingData> trainer,
    IIdvFileService idvFileService,
    IServiceProvider serviceProvider,
    ILogger<AdvancedCallTrumpRegressionTrainerExecutor> logger,
    IOptions<MachineLearningOptions> options,
    MLContext mlContext) :
    RegressionTrainerExecutorBase<AdvancedCallTrumpTrainingData>(trainer, idvFileService, serviceProvider, logger, options, mlContext)
{
    public override string ModelType => "AdvancedCallTrump";

    public override DecisionType DecisionType => DecisionType.AdvancedCallTrump;

    protected override Type GetTrainerType()
    {
        return typeof(AdvancedCallTrumpRegressionModelTrainer);
    }
}
