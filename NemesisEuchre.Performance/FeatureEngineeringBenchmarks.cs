using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace NemesisEuchre.Performance;

[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
[MarkdownExporter]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3400:Methods should not return constants", Justification = "Placeholder benchmark until Phase 4 implementation")]
public class FeatureEngineeringBenchmarks
{
    private int _value;

    [Benchmark]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "BenchmarkDotNet requires instance methods")]
    public int Placeholder()
    {
        return ++_value;
    }
}
