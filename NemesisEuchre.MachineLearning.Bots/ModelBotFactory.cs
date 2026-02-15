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
        if (actor.ModelNames == null || actor.ModelNames.Count == 0)
        {
            throw new ArgumentException(
                "At least one model name must be provided for ModelBot. " +
                "Use --t1m <model> for all decision types, or " +
                "--t1m-play, --t1m-call, --t1m-discard for specific types.");
        }

        return new ModelBot(engineProvider, callTrumpFeatureBuilder, discardCardFeatureBuilder, playCardFeatureBuilder, random, logger, actor);
    }
}
