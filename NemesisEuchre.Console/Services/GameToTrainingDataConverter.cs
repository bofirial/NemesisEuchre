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

public class GameToTrainingDataConverter(
    IGameToEntityMapper gameToEntityMapper,
    IFeatureEngineer<PlayCardDecisionEntity, PlayCardTrainingData> playCardFeatureEngineer,
    IFeatureEngineer<CallTrumpDecisionEntity, CallTrumpTrainingData> callTrumpFeatureEngineer,
    IFeatureEngineer<DiscardCardDecisionEntity, DiscardCardTrainingData> discardCardFeatureEngineer,
    ILogger<GameToTrainingDataConverter> logger) : IGameToTrainingDataConverter
{
    public TrainingDataBatch Convert(IReadOnlyList<Game> games)
    {
        var playCardData = new List<PlayCardTrainingData>();
        var callTrumpData = new List<CallTrumpTrainingData>();
        var discardCardData = new List<DiscardCardTrainingData>();
        var errorCount = 0;

        var dealCount = 0;
        var trickCount = 0;
        var actors = new HashSet<Actor>();

        foreach (var game in games)
        {
            dealCount += game.CompletedDeals.Count;
            trickCount += game.CompletedDeals.Sum(d => d.CompletedTricks.Count);

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
        }

        if (errorCount > 0)
        {
            LoggerMessages.LogTrainingDataLoadComplete(logger, playCardData.Count + callTrumpData.Count + discardCardData.Count, errorCount);
        }

        var stats = new TrainingDataBatchStats(games.Count, dealCount, trickCount, actors);
        return new TrainingDataBatch(playCardData, callTrumpData, discardCardData, stats);
    }
}
