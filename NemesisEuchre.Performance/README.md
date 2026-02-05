# NemesisEuchre.Performance

Performance benchmarking project using BenchmarkDotNet.

## Running Benchmarks

```bash
cd NemesisEuchre.Performance
dotnet run -c Release
```

## Benchmark Categories

### FeatureEngineeringBenchmarks
Measures performance of ML feature engineering:
- JSON deserialization
- Feature computation
- Array pooling effectiveness

### Current Baseline Metrics
To be established in Phase 1 and updated throughout refactoring phases.

## Performance Targets (Phase 4)
- Feature Engineering: 30-40% improvement
- GC Pressure: 20-30% reduction in Gen0/Gen1 collections
- Training Data Load: 80%+ faster row counting
- Inference Latency: 10-20% faster in parallel workloads
