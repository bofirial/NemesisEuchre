using FluentAssertions;

using Microsoft.ML;
using Microsoft.ML.Data;

using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.MachineLearning.Tests.Services;

public class IdvFileServiceTests : IDisposable
{
    private readonly MLContext _mlContext;
    private readonly IdvFileService _service;
    private readonly string _tempDirectory;

    public IdvFileServiceTests()
    {
        _mlContext = new MLContext(seed: 42);
        _service = new IdvFileService(_mlContext);
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void Save_CreatesFile()
    {
        var testData = new List<TestData>
        {
            new() { Feature1 = 1.0f, Feature2 = 2.0f, Label = 1 },
            new() { Feature1 = 3.0f, Feature2 = 4.0f, Label = 2 },
        };
        var filePath = Path.Combine(_tempDirectory, "test.idv");

        _service.Save(testData, filePath);

        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public void Save_WithEmptyData_CreatesValidFile()
    {
        var testData = new List<TestData>();
        var filePath = Path.Combine(_tempDirectory, "empty.idv");

        _service.Save(testData, filePath);

        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public void Load_ReturnsValidDataView()
    {
        var testData = new List<TestData>
        {
            new() { Feature1 = 1.0f, Feature2 = 2.0f, Label = 1 },
            new() { Feature1 = 3.0f, Feature2 = 4.0f, Label = 2 },
        };
        var filePath = Path.Combine(_tempDirectory, "test-load.idv");
        _service.Save(testData, filePath);

        var dataView = _service.Load(filePath);

        dataView.Should().NotBeNull();
    }

    [Fact]
    public void Load_ReturnsDataViewWithCorrectSchema()
    {
        var testData = new List<TestData>
        {
            new() { Feature1 = 1.0f, Feature2 = 2.0f, Label = 1 },
        };
        var filePath = Path.Combine(_tempDirectory, "test-schema.idv");
        _service.Save(testData, filePath);

        var dataView = _service.Load(filePath);

        var schema = dataView.Schema;
        schema.Should().Contain(col => col.Name == nameof(TestData.Feature1));
        schema.Should().Contain(col => col.Name == nameof(TestData.Feature2));
        schema.Should().Contain(col => col.Name == nameof(TestData.Label));
    }

    [Fact]
    public void SaveAndLoad_Roundtrip_PreservesData()
    {
        var testData = new List<TestData>
        {
            new() { Feature1 = 1.5f, Feature2 = 2.5f, Label = 1 },
            new() { Feature1 = 3.5f, Feature2 = 4.5f, Label = 2 },
            new() { Feature1 = 5.5f, Feature2 = 6.5f, Label = 3 },
        };
        var filePath = Path.Combine(_tempDirectory, "roundtrip.idv");
        _service.Save(testData, filePath);

        var dataView = _service.Load(filePath);

        var loadedData = _mlContext.Data.CreateEnumerable<TestData>(dataView, reuseRowObject: false).ToList();
        loadedData.Should().HaveCount(3);
        loadedData[0].Feature1.Should().BeApproximately(1.5f, 0.001f);
        loadedData[0].Feature2.Should().BeApproximately(2.5f, 0.001f);
        loadedData[0].Label.Should().Be(1);
        loadedData[1].Feature1.Should().BeApproximately(3.5f, 0.001f);
        loadedData[1].Feature2.Should().BeApproximately(4.5f, 0.001f);
        loadedData[1].Label.Should().Be(2);
        loadedData[2].Feature1.Should().BeApproximately(5.5f, 0.001f);
        loadedData[2].Feature2.Should().BeApproximately(6.5f, 0.001f);
        loadedData[2].Label.Should().Be(3);
    }

    [Fact]
    public void Save_WithNullData_ThrowsArgumentNullException()
    {
        var filePath = Path.Combine(_tempDirectory, "null-data.idv");

        var act = () => _service.Save<TestData>(null!, filePath);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Save_WithNullFilePath_ThrowsArgumentException()
    {
        var testData = new List<TestData> { new() { Feature1 = 1.0f, Feature2 = 2.0f, Label = 1 } };

        var act = () => _service.Save(testData, null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Save_WithEmptyFilePath_ThrowsArgumentException()
    {
        var testData = new List<TestData> { new() { Feature1 = 1.0f, Feature2 = 2.0f, Label = 1 } };

        var act = () => _service.Save(testData, string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Load_WithNullFilePath_ThrowsArgumentException()
    {
        var act = () => _service.Load(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Load_WithEmptyFilePath_ThrowsArgumentException()
    {
        var act = () => _service.Load(string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && Directory.Exists(_tempDirectory))
        {
#pragma warning disable S1215
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
#pragma warning restore S1215

            try
            {
                Directory.Delete(_tempDirectory, true);
            }
#pragma warning disable S108
            catch (IOException)
            {
            }
#pragma warning restore S108
        }
    }

    private sealed class TestData
    {
        [LoadColumn(0)]
        public float Feature1 { get; set; }

        [LoadColumn(1)]
        public float Feature2 { get; set; }

        [LoadColumn(2)]
        public int Label { get; set; }
    }
}
