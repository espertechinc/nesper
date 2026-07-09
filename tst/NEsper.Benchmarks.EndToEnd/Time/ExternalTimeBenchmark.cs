using System;
using BenchmarkDotNet.Attributes;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Time;

/// <summary>
/// Measures the external-time (AdvanceTime) path used by market-replay style applications:
/// internal timer disabled, one AdvanceTime call interleaved per event. Every advance runs
/// SchedulingServiceImpl.Evaluate, so this benchmark exposes per-advance allocations
/// (head-map view + remove-keys list) and time-window expiry cost that internal-timer
/// benchmarks never show. Time advances monotonically across iterations; window state stays
/// bounded by the 5-second window.
/// </summary>
[MemoryDiagnoser]
[EndToEndJobs]
public class ExternalTimeBenchmark
{
    private EPRuntime    _runtime = null!;
    private TradeEvent[] _events  = null!;
    private long         _currentTime;

    [Params(1_000, 10_000)]
    public int EventCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        config.Runtime.Threading.IsInternalTimerEnabled = false;
        _runtime = EPRuntimeProvider.GetRuntime($"exttime-{Guid.NewGuid()}", config);

        var compiled = EPCompilerProvider.Compiler.Compile(
            "select avg(Price) from TradeEvent#time(5 sec)", new CompilerArguments(config));
        _runtime.DeploymentService.Deploy(compiled);

        _events = new TradeEvent[EventCount];
        for (int i = 0; i < EventCount; i++)
            _events[i] = new TradeEvent { Symbol = "MSFT", Price = 100.0 + i, Volume = 1000L, Timestamp = i };

        _currentTime = 0;
        _runtime.EventService.AdvanceTime(_currentTime);
    }

    [Benchmark]
    public void AdvanceAndSendN()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
        {
            _currentTime += 1; // 1 ms per event => ~5000 events resident in the 5-sec window
            svc.AdvanceTime(_currentTime);
            svc.SendEventBean(_events[i], "TradeEvent");
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
