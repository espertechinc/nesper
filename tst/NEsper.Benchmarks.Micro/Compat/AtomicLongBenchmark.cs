// PERF_REVIEW: L2 — GetAndIncrement() uses a CAS spin loop; Interlocked.Increment - 1 is unconditional
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.compat;

namespace NEsper.Benchmarks.Micro.Compat;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class AtomicLongBenchmark
{
    private AtomicLong _atomicLong = null!;
    private long       _rawValue;

    [GlobalSetup]
    public void Setup()
    {
        _atomicLong = new AtomicLong(0);
        _rawValue   = 0;
    }

    [Benchmark(Baseline = true)]
    public long Current_CasSpinLoop()
    {
        return _atomicLong.GetAndIncrement();
    }

    [Benchmark]
    public long Improved_InterlockedIncrement()
    {
        return Interlocked.Increment(ref _rawValue) - 1;
    }
}
