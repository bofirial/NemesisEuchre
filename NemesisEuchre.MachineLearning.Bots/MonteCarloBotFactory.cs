using Microsoft.Extensions.Options;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;
using NemesisEuchre.MachineLearning.Bots.Simulation;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Options;

namespace NemesisEuchre.MachineLearning.Bots;

public class MonteCarloBotFactory(
    IPredictionEngineProvider engineProvider,
    ICallTrumpInferenceFeatureBuilder callTrumpFeatureBuilder,
    IDiscardCardInferenceFeatureBuilder discardCardFeatureBuilder,
    IPlayCardInferenceFeatureBuilder playCardFeatureBuilder,
    ISimplePlayCardInferenceFeatureBuilder simplePlayCardFeatureBuilder,
    IAdvancedPlayCardInferenceFeatureBuilder advancedPlayCardFeatureBuilder,
    IAdvancedCallTrumpInferenceFeatureBuilder advancedCallTrumpFeatureBuilder,
    IAdvancedDiscardCardInferenceFeatureBuilder advancedDiscardCardFeatureBuilder,
    IRandomNumberGenerator random,
    IOptions<MachineLearningOptions> machineLearningOptions,
    IDealSimulator dealSimulator,
    IHiddenCardDistributor hiddenCardDistributor) : IPlayerActorFactory
{
    private const int DefaultSimulationCount = 50;

    public ActorType ActorType => ActorType.MonteCarlo;

    public IPlayerActor CreatePlayerActor(Actor actor)
    {
        if (actor.ModelNames == null || actor.ModelNames.Count == 0)
        {
            throw new ArgumentException(
                "At least one model name must be provided for MonteCarloBot. " +
                "Use --t1m <model> for all decision types, or " +
                "--t1m-play, --t1m-call, --t1m-discard for specific types.");
        }

        IPlayerActor innerBot = CreateInnerBot(actor, engineProvider);

        int simulationCount = actor.SimulationCount > 0 ? actor.SimulationCount : DefaultSimulationCount;

        return new MonteCarloBot([innerBot], dealSimulator, hiddenCardDistributor, [random], simulationCount);
    }

    private IPlayerActor CreateInnerBot(Actor actor, IPredictionEngineProvider provider)
    {
        if (actor.ExplorationTemperature > 0)
        {
            return new ModelTrainerBot(
                provider,
                callTrumpFeatureBuilder,
                discardCardFeatureBuilder,
                playCardFeatureBuilder,
                random,
                machineLearningOptions,
                actor,
                simplePlayCardFeatureBuilder);
        }

        return new ModelBot(
            provider,
            callTrumpFeatureBuilder,
            discardCardFeatureBuilder,
            playCardFeatureBuilder,
            random,
            actor,
            simplePlayCardFeatureBuilder,
            advancedPlayCardFeatureBuilder,
            advancedCallTrumpFeatureBuilder,
            advancedDiscardCardFeatureBuilder);
    }
}
