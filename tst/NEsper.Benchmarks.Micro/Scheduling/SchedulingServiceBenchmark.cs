// PERF_REVIEW: M2 — new List<long>() allocated inside lock(this) on every Evaluate() call
// PERF_REVIEW: M3 — Keys.Last() is a LINQ scan; OrderedListDictionary.LastEntry.Key is O(1)
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.compat.collections;

namespace NEsper.Benchmarks.Micro.Scheduling;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class SchedulingServiceBenchmark
{
    private List<long>                        _reusableList = null!;
    private OrderedListDictionary<long, long> _timeMap      = null!;

    [Params(10, 100)]
    public int EntryCount;

    [GlobalSetup]
    public void Setup()
    {
        _reusableList = new List<long>(EntryCount);
        _timeMap      = new OrderedListDictionary<long, long>();
        for (int i = 0; i < EntryCount; i++)
            _timeMap[(long)i * 1000L] = i;
    }

    [Benchmark(Baseline = true)]
    public int M2_Current_AllocateList()
    {
        IList<long> removeKeys = new List<long>();
        foreach (var entry in _timeMap)
            removeKeys.Add(entry.Key);
        return removeKeys.Count;
    }

    [Benchmark]
    public int M2_Improved_ReuseList()
    {
        _reusableList.Clear();
        foreach (var entry in _timeMap)
            _reusableList.Add(entry.Key);
        return _reusableList.Count;
    }

    [Benchmark]
    public long M3_Current_LinqLast()
    {
        return _timeMap.Keys.Last();
    }

    [Benchmark]
    public long M3_Improved_LastEntry()
    {
        return _timeMap.LastEntry.Key;
    }
}
