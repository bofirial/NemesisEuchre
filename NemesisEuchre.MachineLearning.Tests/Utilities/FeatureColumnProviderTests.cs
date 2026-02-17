using FluentAssertions;

using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Utilities;

namespace NemesisEuchre.MachineLearning.Tests.Utilities;

public class FeatureColumnProviderTests
{
    [Fact]
    public void GetFeatureColumns_ForCallTrumpTrainingData_ExcludesLabelColumn()
    {
        var columns = FeatureColumnProvider.GetFeatureColumns<CallTrumpTrainingData>();

        columns.Should().NotContain("ExpectedDealPoints", "it has [ColumnName(\"Label\")]");
    }

    [Fact]
    public void GetFeatureColumns_ForCallTrumpTrainingData_WithChosenFilter_ExcludesChosenColumns()
    {
        var columns = FeatureColumnProvider.GetFeatureColumns<CallTrumpTrainingData>(
            col => !col.Contains("Chosen"));

        columns.Should().NotContain(col => col.Contains("Chosen"), "filter excludes them");
        columns.Should().NotContain("ExpectedDealPoints", "it has [ColumnName(\"Label\")]");
    }

    [Fact]
    public void GetFeatureColumns_ForCallTrumpTrainingData_ReturnsColumnsInOrder()
    {
        var columns = FeatureColumnProvider.GetFeatureColumns<CallTrumpTrainingData>();

        columns[0].Should().Be("Card1Rank", "it's LoadColumn(0)");
        columns[1].Should().Be("Card1Suit", "it's LoadColumn(1)");
    }

    [Fact]
    public void GetFeatureColumns_ForCallTrumpTrainingData_Returns17Columns()
    {
        var columns = FeatureColumnProvider.GetFeatureColumns<CallTrumpTrainingData>();

        columns.Should().HaveCount(17, "there are 18 LoadColumn attributes (0-17), minus 1 Label column");
    }

    [Fact]
    public void GetFeatureColumns_ForCallTrumpTrainingData_WithChosenFilter_Returns16Columns()
    {
        var columns = FeatureColumnProvider.GetFeatureColumns<CallTrumpTrainingData>(
            col => !col.Contains("Chosen"));

        columns.Should().HaveCount(16, "17 columns minus 1 ChosenDecision column");
    }
}
