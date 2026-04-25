// PERF_REVIEW: H2 — GetEnumerator() returns a LINQ SelectEnumerableIterator heap object per foreach
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.compat.collections;

namespace NEsper.Benchmarks.Micro.Collections;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class LinkedHashMapBenchmark
{
    private LinkedHashMap<string, int> _map = null!;

    [Params(10, 100)]
    public int ItemCount;

    [GlobalSetup]
    public void Setup()
    {
        _map = new LinkedHashMap<string, int>();
        for (int i = 0; i < ItemCount; i++)
            _map[$"key{i}"] = i;
    }

    [Benchmark(Baseline = true)]
    public int Current_LinqEnumerator()
    {
        int sum = 0;
        foreach (var pair in _map)
            sum += pair.Value;
        return sum;
    }
}
