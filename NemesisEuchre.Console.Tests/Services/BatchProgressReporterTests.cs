using FluentAssertions;

using Moq;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;

namespace NemesisEuchre.Console.Tests.Services;

public class BatchProgressReporterTests
{
    [Fact]
    public void LiveBatchProgressReporter_LatestSnapshot_InitiallyNull()
    {
        var reporter = new LiveBatchProgressReporter();

        reporter.LatestSnapshot.Should().BeNull();
    }

    [Fact]
    public void LiveBatchProgressReporter_ReportProgress_StoresSnapshot()
    {
        var reporter = new LiveBatchProgressReporter();
        var snapshot = new BatchProgressSnapshot(5, 3, 1, 1, 25, 100, 40, 5, 300);

        reporter.ReportProgress(snapshot);

        reporter.LatestSnapshot.Should().Be(snapshot);
    }

    [Fact]
    public void LiveBatchProgressReporter_ReportProgress_ReturnsLatestSnapshot()
    {
        var reporter = new LiveBatchProgressReporter();
        var snapshot1 = new BatchProgressSnapshot(1, 1, 0, 0, 5, 20, 8, 1, 60);
        var snapshot2 = new BatchProgressSnapshot(2, 1, 1, 0, 10, 40, 16, 2, 120);

        reporter.ReportProgress(snapshot1);
        reporter.ReportProgress(snapshot2);

        reporter.LatestSnapshot.Should().Be(snapshot2);
    }

    [Fact]
    public void SubBatchProgressReporter_ReportProgress_AppliesOffsetViaAdd()
    {
        var parentMock = new Mock<IBatchProgressReporter>();
        BatchProgressSnapshot? reportedSnapshot = null;
        parentMock.Setup(x => x.ReportProgress(It.IsAny<BatchProgressSnapshot>()))
            .Callback<BatchProgressSnapshot>(s => reportedSnapshot = s);

        var offset = new BatchProgressSnapshot(100, 60, 30, 10, 500, 2000, 800, 100, 6000);
        var subReporter = new SubBatchProgressReporter(parentMock.Object, offset);
        var currentBatchSnapshot = new BatchProgressSnapshot(5, 3, 1, 1, 25, 100, 40, 5, 300);

        subReporter.ReportProgress(currentBatchSnapshot);

        reportedSnapshot.Should().NotBeNull();
        reportedSnapshot!.CompletedGames.Should().Be(105);
        reportedSnapshot.Team1Wins.Should().Be(63);
        reportedSnapshot.Team2Wins.Should().Be(31);
        reportedSnapshot.FailedGames.Should().Be(11);
        reportedSnapshot.TotalDeals.Should().Be(525);
        reportedSnapshot.TotalTricks.Should().Be(2100);
        reportedSnapshot.TotalCallTrumpDecisions.Should().Be(840);
        reportedSnapshot.TotalDiscardCardDecisions.Should().Be(105);
        reportedSnapshot.TotalPlayCardDecisions.Should().Be(6300);
    }
}
