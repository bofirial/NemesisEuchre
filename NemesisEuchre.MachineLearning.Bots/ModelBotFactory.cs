using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;

namespace NemesisEuchre.MachineLearning.Bots;

public class ModelBotFactory(
    IPredictionEngineProvider engineProvider,
    ICallTrumpInferenceFeatureBuilder callTrumpFeatureBuilder,
    IDiscardCardInferenceFeatureBuilder discardCardFeatureBuilder,
    IPlayCardInferenceFeatureBuilder playCardFeatureBuilder,
    IRandomNumberGenerator random,
    ILogger<ModelBot> logger) : IPlayerActorFactory
{
    public ActorType ActorType => ActorType.Model;

    public IPlayerActor CreatePlayerActor(Actor actor)
    {
        if (actor.ModelName is null)
        {
            throw new ArgumentException("Model name must be provided for ModelBot.");
        }

        return new ModelBot(engineProvider, callTrumpFeatureBuilder, discardCardFeatureBuilder, playCardFeatureBuilder, random, logger, actor);
    }
}
