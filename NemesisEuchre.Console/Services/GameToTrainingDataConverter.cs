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

    Dictionary<string, TrainingDataBatch> ConvertByActor(IReadOnlyList<Game> games);
}

public partial class GameToTrainingDataConverter(
    IGameToEntityMapper gameToEntityMapper,
    IFeatureEngineer<PlayCardDecisionEntity, AllPlayCardTrainingData> playCardFeatureEngineer,
    IFeatureEngineer<CallTrumpDecisionEntity, AllCallTrumpTrainingData> callTrumpFeatureEngineer,
    IFeatureEngineer<DiscardCardDecisionEntity, AllDiscardCardTrainingData> discardCardFeatureEngineer,
    ILogger<GameToTrainingDataConverter> logger) : IGameToTrainingDataConverter
{
    public TrainingDataBatch Convert(IReadOnlyList<Game> games)
    {
        var byActor = ConvertByActor(games);

        var playCardData = new List<AllPlayCardTrainingData>();
        var callTrumpData = new List<AllCallTrumpTrainingData>();
        var discardCardData = new List<AllDiscardCardTrainingData>();
        var actors = new HashSet<Actor>();
        var gameCount = 0;
        var dealCount = 0;
        var trickCount = 0;

        foreach (var batch in byActor.Values)
        {
            playCardData.AddRange(batch.PlayCardData);
            callTrumpData.AddRange(batch.CallTrumpData);
            discardCardData.AddRange(batch.DiscardCardData);
            actors.UnionWith(batch.Stats.Actors);
            gameCount += batch.Stats.GameCount;
            dealCount += batch.Stats.DealCount;
            trickCount += batch.Stats.TrickCount;
        }

        var stats = new TrainingDataBatchStats(gameCount, dealCount, trickCount, actors);
        return new TrainingDataBatch(playCardData, callTrumpData, discardCardData, stats);
    }

    public Dictionary<string, TrainingDataBatch> ConvertByActor(IReadOnlyList<Game> games)
    {
        var results = new ActorGameConversionResult[games.Count];

        Parallel.For(0, games.Count, i => results[i] = ConvertSingleByActor(games[i]));

        var allActorKeys = new HashSet<string>();
        var totalDealCount = 0;
        var totalTrickCount = 0;
        var totalErrors = 0;

        foreach (var result in results)
        {
            allActorKeys.UnionWith(result.ActorData.Keys);
            totalDealCount += result.DealCount;
            totalTrickCount += result.TrickCount;
            totalErrors += result.ErrorCount;
        }

        if (totalErrors > 0)
        {
            var totalRows = results.Sum(r => r.ActorData.Values.Sum(
                a => a.PlayCardData.Count + a.CallTrumpData.Count + a.DiscardCardData.Count));
            LoggerMessages.LogTrainingDataLoadComplete(logger, totalRows, totalErrors);
        }

        var output = new Dictionary<string, TrainingDataBatch>(allActorKeys.Count);
        foreach (var actorKey in allActorKeys)
        {
            var playCard = new List<AllPlayCardTrainingData>();
            var callTrump = new List<AllCallTrumpTrainingData>();
            var discard = new List<AllDiscardCardTrainingData>();
            var actors = new HashSet<Actor>();

            foreach (var result in results)
            {
                if (result.ActorData.TryGetValue(actorKey, out var data))
                {
                    playCard.AddRange(data.PlayCardData);
                    callTrump.AddRange(data.CallTrumpData);
                    discard.AddRange(data.DiscardCardData);
                    actors.UnionWith(data.Actors);
                }
            }

            var stats = new TrainingDataBatchStats(games.Count, totalDealCount, totalTrickCount, actors);
            output[actorKey] = new TrainingDataBatch(playCard, callTrump, discard, stats);
        }

        return output;
    }

    private ActorGameConversionResult ConvertSingleByActor(Game game)
    {
        var actorKeyMap = new Dictionary<PlayerPosition, string>();
        var actorData = new Dictionary<string, ActorDecisionLists>();

        foreach (var (position, player) in game.Players)
        {
            var key = player.Actor.ToFileNameComponent();
            actorKeyMap[position] = key;

            if (!actorData.TryGetValue(key, out _))
            {
                actorData[key] = new ActorDecisionLists();
            }

            actorData[key].Actors.Add(player.Actor);
        }

        var errorCount = 0;
        var dealCount = game.CompletedDeals.Count;
        var trickCount = game.CompletedDeals.Sum(d => d.CompletedTricks.Count);

        var gameEntity = gameToEntityMapper.Map(game);

        foreach (var deal in gameEntity.Deals)
        {
            ProcessDecisions(deal.CallTrumpDecisions, callTrumpFeatureEngineer, actorKeyMap, actorData, (l, td) => l.CallTrumpData.Add(td), ref errorCount);
            ProcessDecisions(deal.DiscardCardDecisions, discardCardFeatureEngineer, actorKeyMap, actorData, (l, td) => l.DiscardCardData.Add(td), ref errorCount);

            foreach (var trick in deal.Tricks)
            {
                ProcessDecisions(trick.PlayCardDecisions, playCardFeatureEngineer, actorKeyMap, actorData, (l, td) => l.PlayCardData.Add(td), ref errorCount);
            }
        }

        return new ActorGameConversionResult(actorData, dealCount, trickCount, errorCount);
    }

    private void ProcessDecisions<TDecisionEntity, TTrainingData>(
        IEnumerable<TDecisionEntity> decisions,
        IFeatureEngineer<TDecisionEntity, TTrainingData> featureEngineer,
        Dictionary<PlayerPosition, string> actorKeyMap,
        Dictionary<string, ActorDecisionLists> actorData,
        Action<ActorDecisionLists, TTrainingData> addItem,
        ref int errorCount)
        where TDecisionEntity : class, IDecisionEntityWithPosition
        where TTrainingData : class, new()
    {
        foreach (var decision in decisions)
        {
            if (decision.RelativeDealPoints == null)
            {
                continue;
            }

            var actorKey = actorKeyMap[decision.PlayerPosition];

            try
            {
                addItem(actorData[actorKey], featureEngineer.Transform(decision));
            }
            catch (Exception ex)
            {
                errorCount++;
                LoggerMessages.LogFeatureEngineeringError(logger, ex);
            }
        }
    }
}
