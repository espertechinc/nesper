// PERF_REVIEW: L4 — ENABLED is a settable property; JIT cannot eliminate the dead-branch guard
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace NEsper.Benchmarks.Micro.Instrumentation;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class InstrumentationHelperBenchmark
{
    private const bool         ConstFalse    = false;
    private static readonly bool ReadonlyFalse = false;

    [Benchmark(Baseline = true)]
    public int Current_PropertyCheck()
    {
        int count = 0;
        if (InstrumentationHelper.ENABLED) count++;
        if (InstrumentationHelper.ENABLED) count++;
        if (InstrumentationHelper.ENABLED) count++;
        if (InstrumentationHelper.ENABLED) count++;
        return count;
    }

    [Benchmark]
    public int Improved_ConstCheck()
    {
        int count = 0;
        if (ConstFalse) count++;
        if (ConstFalse) count++;
        if (ConstFalse) count++;
        if (ConstFalse) count++;
        return count;
    }

    [Benchmark]
    public int Improved_ReadonlyFieldCheck()
    {
        int count = 0;
        if (ReadonlyFalse) count++;
        if (ReadonlyFalse) count++;
        if (ReadonlyFalse) count++;
        if (ReadonlyFalse) count++;
        return count;
    }
}
