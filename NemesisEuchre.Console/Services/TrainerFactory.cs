using NemesisEuchre.Console.Services.TrainerExecutors;
using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Services;

public interface ITrainerFactory
{
    IEnumerable<ITrainerExecutor> GetTrainers(DecisionType decisionType);
}

public class TrainerFactory(IEnumerable<ITrainerExecutor> trainers) : ITrainerFactory
{
    private readonly Dictionary<DecisionType, ITrainerExecutor> _trainersByDecision = trainers
        .ToDictionary(t => t.DecisionType, t => t);

    public IEnumerable<ITrainerExecutor> GetTrainers(DecisionType decisionType)
    {
        if (decisionType == DecisionType.All)
        {
            return _trainersByDecision.Values;
        }

        if (_trainersByDecision.TryGetValue(decisionType, out var trainer))
        {
            return [trainer];
        }

        return [];
    }
}
