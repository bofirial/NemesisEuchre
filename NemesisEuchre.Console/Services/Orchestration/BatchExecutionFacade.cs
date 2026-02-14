namespace NemesisEuchre.Console.Services.Orchestration;

public interface IBatchExecutionFacade
{
    int CalculateEffectiveParallelism();

    bool ShouldUseSubBatches(int numberOfGames, int maxGamesPerSubBatch);
}

public sealed class BatchExecutionFacade(
    IParallelismCoordinator parallelismCoordinator,
    ISubBatchStrategy subBatchStrategy) : IBatchExecutionFacade
{
    public int CalculateEffectiveParallelism()
    {
        return parallelismCoordinator.CalculateEffectiveParallelism();
    }

    public bool ShouldUseSubBatches(int numberOfGames, int maxGamesPerSubBatch)
    {
        return subBatchStrategy.ShouldUseSubBatches(numberOfGames, maxGamesPerSubBatch);
    }
}
