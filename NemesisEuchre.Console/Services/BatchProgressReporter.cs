using Spectre.Console;

namespace NemesisEuchre.Console.Services;

public interface IBatchProgressReporter
{
    void ReportGameCompleted(int count);

    void ReportGamesSaved(int count);

    void ReportIdvSaveStarted()
    {
    }

    void ReportIdvSaveCompleted()
    {
    }
}

internal sealed class BatchProgressReporter(ProgressTask playingTask, ProgressTask savingTask, ProgressTask? idvSaveTask = null) : IBatchProgressReporter
{
    private bool _idvSaveStarted;

    public void ReportGameCompleted(int count)
    {
        playingTask.Value = count;
    }

    public void ReportGamesSaved(int count)
    {
        savingTask.Value = count;
    }

    public void ReportIdvSaveStarted()
    {
        if (idvSaveTask != null && !_idvSaveStarted)
        {
            idvSaveTask.StartTask();
            _idvSaveStarted = true;
        }
    }

    public void ReportIdvSaveCompleted()
    {
        if (idvSaveTask is { } task)
        {
            task.Value = task.MaxValue;
        }
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

    public void ReportIdvSaveStarted()
    {
        parentReporter.ReportIdvSaveStarted();
    }

    public void ReportIdvSaveCompleted()
    {
        parentReporter.ReportIdvSaveCompleted();
    }
}
