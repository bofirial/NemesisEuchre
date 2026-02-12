using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Models;
using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.Console.Services;

public interface IGameToTrainingDataConverter
{
    TrainingDataBatch Convert(IReadOnlyList<Game> games);
}

public partial class GameToTrainingDataConverter(
    IGameToEntityMapper gameToEntityMapper,
    IFeatureEngineer<PlayCardDecisionEntity, PlayCardTrainingData> playCardFeatureEngineer,
    IFeatureEngineer<CallTrumpDecisionEntity, CallTrumpTrainingData> callTrumpFeatureEngineer,
    IFeatureEngineer<DiscardCardDecisionEntity, DiscardCardTrainingData> discardCardFeatureEngineer,
    ILogger<GameToTrainingDataConverter> logger) : IGameToTrainingDataConverter
{
    public TrainingDataBatch Convert(IReadOnlyList<Game> games)
    {
        var results = new GameConversionResult[games.Count];

        Parallel.For(0, games.Count, i => results[i] = ConvertSingle(games[i]));

        var totalPlay = 0;
        var totalCallTrump = 0;
        var totalDiscard = 0;
        var totalErrors = 0;
        var dealCount = 0;
        var trickCount = 0;

        foreach (var r in results)
        {
            totalPlay += r.PlayCardData.Count;
            totalCallTrump += r.CallTrumpData.Count;
            totalDiscard += r.DiscardCardData.Count;
            totalErrors += r.ErrorCount;
            dealCount += r.DealCount;
            trickCount += r.TrickCount;
        }

        var playCardData = new List<PlayCardTrainingData>(totalPlay);
        var callTrumpData = new List<CallTrumpTrainingData>(totalCallTrump);
        var discardCardData = new List<DiscardCardTrainingData>(totalDiscard);
        var actors = new HashSet<Actor>();

        foreach (var r in results)
        {
            playCardData.AddRange(r.PlayCardData);
            callTrumpData.AddRange(r.CallTrumpData);
            discardCardData.AddRange(r.DiscardCardData);
            actors.UnionWith(r.Actors);
        }

        if (totalErrors > 0)
        {
            LoggerMessages.LogTrainingDataLoadComplete(logger, playCardData.Count + callTrumpData.Count + discardCardData.Count, totalErrors);
        }

        var stats = new TrainingDataBatchStats(games.Count, dealCount, trickCount, actors);
        return new TrainingDataBatch(playCardData, callTrumpData, discardCardData, stats);
    }

    private GameConversionResult ConvertSingle(Game game)
    {
        var playCardData = new List<PlayCardTrainingData>();
        var callTrumpData = new List<CallTrumpTrainingData>();
        var discardCardData = new List<DiscardCardTrainingData>();
        var errorCount = 0;

        var dealCount = game.CompletedDeals.Count;
        var trickCount = game.CompletedDeals.Sum(d => d.CompletedTricks.Count);

        var actors = new HashSet<Actor>();
        foreach (var player in game.Players.Values)
        {
            actors.Add(player.Actor);
        }

        var gameEntity = gameToEntityMapper.Map(game);

        foreach (var deal in gameEntity.Deals)
        {
            foreach (var decision in deal.CallTrumpDecisions)
            {
                if (decision.RelativeDealPoints == null)
                {
                    continue;
                }

                try
                {
                    callTrumpData.Add(callTrumpFeatureEngineer.Transform(decision));
                }
                catch (Exception ex)
                {
                    errorCount++;
                    LoggerMessages.LogFeatureEngineeringError(logger, ex);
                }
            }

            foreach (var decision in deal.DiscardCardDecisions)
            {
                if (decision.RelativeDealPoints == null)
                {
                    continue;
                }

                try
                {
                    discardCardData.Add(discardCardFeatureEngineer.Transform(decision));
                }
                catch (Exception ex)
                {
                    errorCount++;
                    LoggerMessages.LogFeatureEngineeringError(logger, ex);
                }
            }

            foreach (var decision in deal.PlayCardDecisions)
            {
                if (decision.RelativeDealPoints == null)
                {
                    continue;
                }

                try
                {
                    playCardData.Add(playCardFeatureEngineer.Transform(decision));
                }
                catch (Exception ex)
                {
                    errorCount++;
                    LoggerMessages.LogFeatureEngineeringError(logger, ex);
                }
            }
        }

        return new GameConversionResult(playCardData, callTrumpData, discardCardData, dealCount, trickCount, actors, errorCount);
    }
}
