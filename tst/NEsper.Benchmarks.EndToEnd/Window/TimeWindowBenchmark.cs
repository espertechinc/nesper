using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Window;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class TimeWindowBenchmark
{
    private EPRuntime    _runtime = null!;
    private TradeEvent[] _events  = null!;

    [Params(1_000, 10_000)]
    public int EventCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        _runtime = EPRuntimeProvider.GetRuntime($"timewin-{Guid.NewGuid()}", config);

        var compiled = EPCompilerProvider.Compiler.Compile(
            "select * from TradeEvent#time(5 sec)", new CompilerArguments(config));
        _runtime.DeploymentService.Deploy(compiled);

        _events = new TradeEvent[EventCount];
        for (int i = 0; i < EventCount; i++)
            _events[i] = new TradeEvent { Symbol = "MSFT", Price = 100.0 + i, Volume = 1000L, Timestamp = i };
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
