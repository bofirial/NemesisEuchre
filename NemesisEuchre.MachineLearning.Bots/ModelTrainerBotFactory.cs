using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Options;

namespace NemesisEuchre.MachineLearning.Bots;

public class ModelTrainerBotFactory(
    IPredictionEngineProvider engineProvider,
    ICallTrumpInferenceFeatureBuilder callTrumpFeatureBuilder,
    IDiscardCardInferenceFeatureBuilder discardCardFeatureBuilder,
    IPlayCardInferenceFeatureBuilder playCardFeatureBuilder,
    IRandomNumberGenerator random,
    IOptions<MachineLearningOptions> machineLearningOptions,
    ILogger<ModelTrainerBot> logger) : IPlayerActorFactory
{
    public ActorType ActorType => ActorType.ModelTrainer;

    public IPlayerActor CreatePlayerActor(Actor actor)
    {
        if (actor.ModelName is null)
        {
            throw new ArgumentException("Model name must be provided for ModelBot.");
        }

        return new ModelTrainerBot(engineProvider, callTrumpFeatureBuilder, discardCardFeatureBuilder, playCardFeatureBuilder, random, machineLearningOptions, logger, actor);
    }
}
