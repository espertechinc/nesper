using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Filter;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class RangeFilterBenchmark
{
    private EPRuntime    _runtime = null!;
    private TradeEvent[] _events  = null!;

    [Params(1_000, 10_000)]
    public int EventCount;

    [Params(1, 10, 100)]
    public int StatementCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        _runtime = EPRuntimeProvider.GetRuntime($"range-{Guid.NewGuid()}", config);

        var compiler = EPCompilerProvider.Compiler;
        var args     = new CompilerArguments(config);
        for (int s = 0; s < StatementCount; s++)
        {
            double lo = 80.0 + s;
            double hi = 120.0 + s;
            var compiled = compiler.Compile(
                $"select * from TradeEvent where Price between {lo} and {hi}", args);
            _runtime.DeploymentService.Deploy(compiled);
        }

        _events = new TradeEvent[EventCount];
        for (int i = 0; i < EventCount; i++)
            _events[i] = new TradeEvent { Symbol = "MSFT", Price = 90.0 + (i % 50), Volume = 1000L, Timestamp = i };
    }

    [Benchmark]
    public void SendN()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
            svc.SendEventBean(_events[i], "TradeEvent");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
