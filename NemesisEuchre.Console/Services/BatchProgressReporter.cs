using NemesisEuchre.Console.Models;

namespace NemesisEuchre.Console.Services;

public interface IBatchProgressReporter
{
    void ReportProgress(BatchProgressSnapshot snapshot);
}

internal sealed class LiveBatchProgressReporter : IBatchProgressReporter
{
    private volatile BatchProgressSnapshot? _latestSnapshot;

    public BatchProgressSnapshot? LatestSnapshot => _latestSnapshot;

    public void ReportProgress(BatchProgressSnapshot snapshot)
    {
        _latestSnapshot = snapshot;
    }
}

internal sealed class SubBatchProgressReporter(
    IBatchProgressReporter parentReporter,
    BatchProgressSnapshot cumulativeOffset) : IBatchProgressReporter
{
    public void ReportProgress(BatchProgressSnapshot snapshot)
    {
        parentReporter.ReportProgress(cumulativeOffset.Add(snapshot));
    }
}
