// PERF_REVIEW: H4 — EvaluateInternal(statementId) allocates a new 256-capacity ArrayDeque per call
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.compat.collections;

namespace NEsper.Benchmarks.Micro.FilterIndex;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class FilterServiceBaseEvalBenchmark
{
    private ArrayDeque<object> _reusable = null!;

    [GlobalSetup]
    public void Setup() => _reusable = new ArrayDeque<object>();

    [Benchmark(Baseline = true)]
    public int Current_AllocatePerCall()
    {
        var matches = new ArrayDeque<object>();
        matches.Add(new object());
        int count = matches.Count;
        return count;
    }

    [Benchmark]
    public int Improved_ReuseInstance()
    {
        _reusable.Clear();
        _reusable.Add(new object());
        int count = _reusable.Count;
        _reusable.Clear();
        return count;
    }
}
