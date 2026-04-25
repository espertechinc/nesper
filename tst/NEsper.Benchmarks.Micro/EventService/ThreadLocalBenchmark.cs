// PERF_REVIEW: L5 — _threadLocals.GetOrCreate() called ~10 times per event dispatch on same thread
using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace NEsper.Benchmarks.Micro.EventService;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class ThreadLocalBenchmark
{
    [ThreadStatic]
    private static object? _tlsEntry;

    [GlobalSetup]
    public void Setup() => _tlsEntry = new object();

    [Benchmark(Baseline = true)]
    public int Current_TenTlsLookups()
    {
        var e1  = _tlsEntry; var e2  = _tlsEntry; var e3  = _tlsEntry;
        var e4  = _tlsEntry; var e5  = _tlsEntry; var e6  = _tlsEntry;
        var e7  = _tlsEntry; var e8  = _tlsEntry; var e9  = _tlsEntry;
        var e10 = _tlsEntry;
        return e1 == e10 ? 1 : 0;
    }

    [Benchmark]
    public int Improved_OneTlsLookupCached()
    {
        var entry = _tlsEntry;
        var e1 = entry; var e2 = entry; var e3 = entry; var e4 = entry;
        var e5 = entry; var e6 = entry; var e7 = entry; var e8 = entry;
        var e9 = entry; var e10 = entry;
        return e1 == e10 ? 1 : 0;
    }
}
