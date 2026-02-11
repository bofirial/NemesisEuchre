using Spectre.Console;

namespace NemesisEuchre.Console.Services;

public interface IBatchProgressReporter
{
    void ReportGameCompleted(int count);
}

internal sealed class BatchProgressReporter(ProgressTask playingTask) : IBatchProgressReporter
{
    public void ReportGameCompleted(int count)
    {
        playingTask.Value = count;
    }
}

internal sealed class SubBatchProgressReporter(
    IBatchProgressReporter parentReporter,
    int completedOffset) : IBatchProgressReporter
{
    public void ReportGameCompleted(int count)
    {
        parentReporter.ReportGameCompleted(completedOffset + count);
    }
}
