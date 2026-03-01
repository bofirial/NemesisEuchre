using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public class CallTrumpRegressionTrainerExecutor(
    IModelTrainer<CallTrumpTrainingData> trainer,
    IIdvFileService idvFileService,
    IServiceProvider serviceProvider,
    ILogger<CallTrumpRegressionTrainerExecutor> logger,
    IOptions<MachineLearningOptions> options,
    MLContext mlContext) :
    RegressionTrainerExecutorBase<CallTrumpTrainingData>(trainer, idvFileService, serviceProvider, logger, options, mlContext)
{
    public override string ModelType => "CallTrump";

    public override DecisionType DecisionType => DecisionType.CallTrump;

    protected override Type GetTrainerType()
    {
        return typeof(CallTrumpRegressionModelTrainer);
    }
}
