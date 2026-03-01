using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public class PlayCardRegressionTrainerExecutor(
    IModelTrainer<PlayCardTrainingData> trainer,
    IIdvFileService idvFileService,
    IServiceProvider serviceProvider,
    ILogger<PlayCardRegressionTrainerExecutor> logger,
    IOptions<MachineLearningOptions> options,
    MLContext mlContext) :
    RegressionTrainerExecutorBase<PlayCardTrainingData>(trainer, idvFileService, serviceProvider, logger, options, mlContext)
{
    public override string ModelType => "PlayCard";

    public override DecisionType DecisionType => DecisionType.Play;

    protected override Type GetTrainerType()
    {
        return typeof(PlayCardRegressionModelTrainer);
    }
}
