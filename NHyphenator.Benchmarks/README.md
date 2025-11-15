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

### Current Performance (.NET 10)

| Method              | Mean       | Allocated |
|---------------------|------------|-----------|
| HyphenateSingleWord | ~110 μs    | 10.96 KB  |
| HyphenateShortText  | ~400 μs    | 39.6 KB   |
| HyphenateLongText   | ~6.4 ms    | 651.27 KB |

### Improvements from Previous Version

- Memory allocations reduced by approximately **15%** across all workloads
- Performance remains consistent (no regressions)

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
