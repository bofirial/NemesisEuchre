namespace NemesisEuchre.Console.Services.Orchestration;

public interface ISubBatchStrategy
{
    bool ShouldUseSubBatches(int totalGames, int maxPerBatch);

    IEnumerable<int> CalculateSubBatchSizes(int totalGames, int maxPerBatch);
}

public class SubBatchStrategy : ISubBatchStrategy
{
    public bool ShouldUseSubBatches(int totalGames, int maxPerBatch)
    {
        return totalGames > maxPerBatch;
    }

    public IEnumerable<int> CalculateSubBatchSizes(int totalGames, int maxPerBatch)
    {
        var remaining = totalGames;

        while (remaining > 0)
        {
            var batchSize = Math.Min(maxPerBatch, remaining);
            yield return batchSize;
            remaining -= batchSize;
        }
    }
}
