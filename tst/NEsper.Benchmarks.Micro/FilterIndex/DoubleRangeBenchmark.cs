// PERF_REVIEW: H3 — FilterParamIndexDoubleRange allocates two DoubleRange class objects per event
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.@internal.filterspec;

namespace NEsper.Benchmarks.Micro.FilterIndex;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class DoubleRangeBenchmark
{
    private const double AttributeValue = 105.5;
    private const double LargestRangeValue = 1000.0;

    [Benchmark(Baseline = true)]
    public (DoubleRange, DoubleRange) Current_AllocateTwoRanges()
    {
        var rangeStart = new DoubleRange(AttributeValue - LargestRangeValue, AttributeValue);
        var rangeEnd   = new DoubleRange(AttributeValue, double.MaxValue);
        return (rangeStart, rangeEnd);
    }

    [Benchmark]
    public (double, double, double, double) Improved_StructBounds()
    {
        double lo = AttributeValue - LargestRangeValue;
        double hi = AttributeValue;
        return (lo, hi, hi, double.MaxValue);
    }
}
