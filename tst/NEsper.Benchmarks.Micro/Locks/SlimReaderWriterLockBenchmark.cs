// PERF_REVIEW: M1 — SupportsRecursion forces per-thread TLS state tracking on every acquire/release
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace NEsper.Benchmarks.Micro.Locks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class SlimReaderWriterLockBenchmark
{
    private ReaderWriterLockSlim _withRecursion    = null!;
    private ReaderWriterLockSlim _withoutRecursion = null!;

    [GlobalSetup]
    public void Setup()
    {
        _withRecursion    = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        _withoutRecursion = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
    }

    [Benchmark(Baseline = true)]
    public void Current_SupportsRecursion()
    {
        _withRecursion.EnterReadLock();
        _withRecursion.ExitReadLock();
    }

    [Benchmark]
    public void Improved_NoRecursion()
    {
        _withoutRecursion.EnterReadLock();
        _withoutRecursion.ExitReadLock();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _withRecursion.Dispose();
        _withoutRecursion.Dispose();
    }
}
