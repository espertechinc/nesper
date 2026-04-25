// PERF_REVIEW: H6 — FilterParamIndexStringRange allocates two StringRange class objects per event
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.@internal.filterspec;

namespace NEsper.Benchmarks.Micro.FilterIndex;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class StringRangeBenchmark
{
    private const string AttributeValue = "MSFT";

    [Benchmark(Baseline = true)]
    public (string?, string?, string?, string?) Current_AllocateTwoRanges()
    {
        var rangeStart = new StringRange(null!, AttributeValue);
        var rangeEnd   = new StringRange(AttributeValue, null!);
        return (rangeStart.Min, rangeStart.Max, rangeEnd.Min, rangeEnd.Max);
    }

    [Benchmark]
    public (string?, string?, string?, string?) Improved_InlineBounds()
    {
        return (null, AttributeValue, AttributeValue, null);
    }
}
