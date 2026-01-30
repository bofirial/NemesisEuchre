using Spectre.Console;

namespace NemesisEuchre.Console.Services;

public interface IBatchProgressReporter
{
    void ReportGameCompleted(int count);

    void ReportGamesSaved(int count);
}

internal sealed class BatchProgressReporter(ProgressTask playingTask, ProgressTask savingTask) : IBatchProgressReporter
{
    public void ReportGameCompleted(int count)
    {
        playingTask.Value = count;
    }

    public void ReportGamesSaved(int count)
    {
        savingTask.Value = count;
    }
}

internal sealed class SubBatchProgressReporter(
    IBatchProgressReporter parentReporter,
    int completedOffset,
    int savedOffset) : IBatchProgressReporter
{
    public void ReportGameCompleted(int count)
    {
        parentReporter.ReportGameCompleted(completedOffset + count);
    }

    public void ReportGamesSaved(int count)
    {
        parentReporter.ReportGamesSaved(savedOffset + count);
    }
}
