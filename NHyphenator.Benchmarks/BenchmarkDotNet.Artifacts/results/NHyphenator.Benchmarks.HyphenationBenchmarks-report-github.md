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
| HyphenateSingleWord |   109.6 μs |   4.89 μs |  0.27 μs |   109.3 μs |   109.8 μs |   109.7 μs |  0.7324 |  13.02 KB |
| HyphenateShortText  |   392.4 μs |  36.29 μs |  1.99 μs |   390.5 μs |   394.5 μs |   392.3 μs |  2.4414 |  46.56 KB |
| HyphenateLongText   | 6,369.6 μs | 456.74 μs | 25.04 μs | 6,349.4 μs | 6,397.6 μs | 6,361.8 μs | 46.8750 | 768.52 KB |
