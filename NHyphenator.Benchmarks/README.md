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

### Current Performance (.NET 10 - Phase 4 Optimizations)

| Method              | Mean        | Allocated | vs Baseline Time | vs Baseline Memory |
|---------------------|-------------|-----------|------------------|--------------------|
| HyphenateSingleWord | ~53 μs      | 416 B     | -52.9%           | -96.8%             |
| HyphenateShortText  | ~198 μs     | 2.06 KB   | -51.7%           | -95.6%             |
| HyphenateLongText   | ~3.2 ms     | 27.63 KB  | -49.5%           | -96.6%             |

### Performance History

**Phase 4 (Micro-optimizations):**
- Memory allocations reduced by additional **1-20%** from Phase 3
- Execution time improved by **0-3%** from Phase 3
- Key optimizations: FindLastWord method optimization, guard clauses

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
- **~97% reduction in memory allocations**
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
