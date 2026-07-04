using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Threading;

/// <summary>
/// Measures contended event send: N threads concurrently sending into the same runtime
/// with statements that match most events. Exercises the shared filter-service read lock
/// and the per-statement agent-instance write lock — the paths single-threaded benchmarks
/// never stress. ThreadCount=1 is the baseline; the ratio column shows the contention cost.
/// Thread start/join overhead is included in every variant, so ratios remain comparable.
/// </summary>
[MemoryDiagnoser]
[EndToEndJobs]
public class ContendedSendBenchmark
{
    private EPRuntime      _runtime = null!;
    private TradeEvent[][] _eventsPerThread = null!;

    [Params(1, 2, 4, 8)]
    public int ThreadCount;

    [Params(10_000)]
    public int EventCount;

    [Params(1, 10)]
    public int StatementCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        _runtime = EPRuntimeProvider.GetRuntime($"contended-{Guid.NewGuid()}", config);

        var compiler = EPCompilerProvider.Compiler;
        var args     = new CompilerArguments(config);
        for (int s = 0; s < StatementCount; s++)
        {
            // Stateful statements matched by every event: each match acquires the
            // statement agent-instance write lock and updates window + aggregation state.
            var compiled = compiler.Compile(
                $"select avg(Price) from TradeEvent#length(1000) where Price > {s}", args);
            _runtime.DeploymentService.Deploy(compiled);
        }

        var perThread = EventCount / ThreadCount;
        _eventsPerThread = new TradeEvent[ThreadCount][];
        for (int t = 0; t < ThreadCount; t++)
        {
            _eventsPerThread[t] = new TradeEvent[perThread];
            for (int i = 0; i < perThread; i++)
                _eventsPerThread[t][i] = new TradeEvent { Symbol = "MSFT", Price = 100.0 + i, Volume = 1000L, Timestamp = i };
        }
    }

    [Benchmark]
    public void SendNConcurrent()
    {
        var svc     = _runtime.EventService;
        var barrier = new Barrier(ThreadCount);
        var threads = new Thread[ThreadCount];

        for (int t = 0; t < ThreadCount; t++)
        {
            var events = _eventsPerThread[t];
            threads[t] = new Thread(() =>
            {
                barrier.SignalAndWait();
                for (int i = 0; i < events.Length; i++)
                    svc.SendEventBean(events[i], "TradeEvent");
            });
            threads[t].Start();
        }

        for (int t = 0; t < ThreadCount; t++)
            threads[t].Join();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
