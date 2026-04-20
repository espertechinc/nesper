// PERF_REVIEW: M4 — FilterParamIndexEqualsBase stores keys as object, boxing every numeric value
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace NEsper.Benchmarks.Micro.FilterIndex;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class BoxingBenchmark
{
    private Dictionary<object, int> _objectDict = null!;
    private Dictionary<double, int> _typedDict  = null!;
    private const double LookupKey  = 100.0;
    private const int    EntryCount = 100;

    [GlobalSetup]
    public void Setup()
    {
        _objectDict = new Dictionary<object, int>(EntryCount);
        _typedDict  = new Dictionary<double, int>(EntryCount);
        for (int i = 0; i < EntryCount; i++)
        {
            _objectDict[(double)i] = i;
            _typedDict[(double)i]  = i;
        }
    }

    [Benchmark(Baseline = true)]
    public bool Current_ObjectDictLookup()
    {
        return _objectDict.TryGetValue(LookupKey, out _);
    }

    [Benchmark]
    public bool Improved_TypedDictLookup()
    {
        return _typedDict.TryGetValue(LookupKey, out _);
    }
}
