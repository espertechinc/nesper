// PERF_REVIEW: H1 — every Acquire() allocates a new TrackedDisposable on the heap
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.compat.threading.locks;

namespace NEsper.Benchmarks.Micro.Locks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class TrackedDisposableBenchmark
{
    private SlimReaderWriterLock _rwLock = null!;

    [GlobalSetup]
    public void Setup() => _rwLock = new SlimReaderWriterLock();

    [Benchmark(Baseline = true)]
    public void Current_AcquireDisposable()
    {
        using (_rwLock.ReadLock.Acquire()) { }
    }

    [Benchmark]
    public void Improved_DirectAcquireRelease()
    {
        _rwLock.AcquireReaderLock(LockConstants.DefaultTimeout);
        try { }
        finally { _rwLock.ReleaseReaderLock(); }
    }
}
