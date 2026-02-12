using NemesisEuchre.Console.Models;

namespace NemesisEuchre.Console.Services;

public interface IBatchProgressReporter
{
    void ReportProgress(BatchProgressSnapshot snapshot);
}

internal sealed class LiveBatchProgressReporter : IBatchProgressReporter
{
    private volatile BatchProgressSnapshot? _latestSnapshot;
    private volatile string? _statusMessage;

    public BatchProgressSnapshot? LatestSnapshot => _latestSnapshot;

    public string? StatusMessage
    {
        get => _statusMessage;
        set => _statusMessage = value;
    }

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
