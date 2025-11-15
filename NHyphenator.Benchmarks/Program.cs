using BenchmarkDotNet.Running;

namespace NHyphenator.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<HyphenationBenchmarks>(args: args);
    }
}
