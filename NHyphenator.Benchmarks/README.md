# NHyphenator Benchmarks

This project contains performance benchmarks for the NHyphenator library using [BenchmarkDotNet](https://benchmarkdotnet.org/).

## Running Benchmarks

To run all benchmarks:

```bash
dotnet run -c Release
```

To run specific benchmarks:

```bash
dotnet run -c Release --filter '*SingleWord*'
```

To run quick benchmarks (useful during development):

```bash
dotnet run -c Release -- --job short
```

## Benchmark Results

### Current Performance (.NET 10 - Phase 3 Optimizations)

| Method              | Mean        | Allocated | vs Baseline Time | vs Baseline Memory |
|---------------------|-------------|-----------|------------------|--------------------|
| HyphenateSingleWord | ~54 μs      | 520 B     | -52.3%           | -96.0%             |
| HyphenateShortText  | ~203 μs     | 2.42 KB   | -50.4%           | -94.8%             |
| HyphenateLongText   | ~3.2 ms     | 27.34 KB  | -50.0%           | -96.4%             |

### Performance History

**Phase 3 (Span + ArrayPool):**
- Memory allocations reduced by **~95%** from Phase 2
- Execution time reduced by **~50%** from Phase 2
- Key optimizations: ReadOnlySpan<char> for pattern matching, ArrayPool for temporary arrays

**Phase 2 (string.Create + CollectionsMarshal):**
- Memory allocations reduced by **~17%** from Phase 1
- Key optimizations: string.Create, CollectionsMarshal.AsSpan

**Phase 1 (Basic Optimizations):**
- Memory allocations reduced by **~15%** from original
- Key optimizations: Array.Empty, direct digit parsing, StringBuilder capacity

**Overall Improvements from Original:**
- **~96% reduction in memory allocations**
- **~50% faster execution time**
- **All existing tests passing**

## Adding New Benchmarks

To add new benchmarks:

1. Add a new method to the `HyphenationBenchmarks` class in `HyphenationBenchmarks.cs`
2. Annotate it with the `[Benchmark]` attribute
3. Run the benchmarks to see the new results

Example:

```csharp
[Benchmark]
public string MyNewBenchmark()
{
    return _hyphenator!.HyphenateText("your test text");
}
```
