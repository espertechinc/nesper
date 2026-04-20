// PERF_REVIEW: H5 — Head/Tail/Between each allocate an OrderedListDictionaryView + enumerator per call
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.compat.collections;

namespace NEsper.Benchmarks.Micro.FilterIndex;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class OrderedListDictionaryViewBenchmark
{
    private OrderedListDictionary<double, int> _dict = null!;
    private const double QueryValue = 105.5;

    [Params(10, 100)]
    public int EntryCount;

    [GlobalSetup]
    public void Setup()
    {
        _dict = new OrderedListDictionary<double, int>();
        for (int i = 0; i < EntryCount; i++)
            _dict[i * 10.0] = i;
    }

    [Benchmark(Baseline = true)]
    public int Current_HeadViewAlloc()
    {
        int sum = 0;
        var view = _dict.Head(QueryValue, false);
        foreach (var v in view.Values)
            sum += v;
        return sum;
    }

    [Benchmark]
    public int Improved_DirectIteration()
    {
        int sum = 0;
        foreach (var kvp in _dict)
        {
            if (kvp.Key >= QueryValue) break;
            sum += kvp.Value;
        }
        return sum;
    }
}
