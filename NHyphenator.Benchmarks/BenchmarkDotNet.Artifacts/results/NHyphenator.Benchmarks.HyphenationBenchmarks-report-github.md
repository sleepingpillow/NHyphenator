```

BenchmarkDotNet v0.14.0, Ubuntu 24.04.3 LTS (Noble Numbat)
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.100
  [Host]   : .NET 10.0.0 (10.0.25.52411), X64 RyuJIT AVX2
  ShortRun : .NET 10.0.0 (10.0.25.52411), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method              | Mean       | Error     | StdDev   | Min        | Max        | Median     | Gen0    | Allocated |
|-------------------- |-----------:|----------:|---------:|-----------:|-----------:|-----------:|--------:|----------:|
| HyphenateSingleWord |   110.2 μs |   8.27 μs |  0.45 μs |   109.9 μs |   110.7 μs |   109.9 μs |  0.6104 |  10.96 KB |
| HyphenateShortText  |   401.0 μs |  27.03 μs |  1.48 μs |   399.6 μs |   402.5 μs |   400.9 μs |  1.9531 |   39.6 KB |
| HyphenateLongText   | 6,448.4 μs | 533.55 μs | 29.25 μs | 6,431.1 μs | 6,482.1 μs | 6,431.8 μs | 39.0625 | 651.27 KB |
