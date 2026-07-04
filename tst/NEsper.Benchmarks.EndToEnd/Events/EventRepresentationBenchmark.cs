using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Events;

/// <summary>
/// Compares the per-event cost of the three common event representations — POCO (bean),
/// IDictionary (map), and JSON string — through an identical filter + projection statement.
/// Each representation uses a different code-generated property-getter path and event sender;
/// bean is the baseline. JSON includes parse cost, which is the realistic boundary cost for
/// services ingesting JSON payloads.
/// </summary>
[MemoryDiagnoser]
[EndToEndJobs]
public class EventRepresentationBenchmark
{
    private EPRuntime                       _runtime    = null!;
    private TradeEvent[]                    _beanEvents = null!;
    private IDictionary<string, object>[]   _mapEvents  = null!;
    private string[]                        _jsonEvents = null!;

    [Params(10_000)]
    public int EventCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        config.Common.AddEventType("TradeMap", new Dictionary<string, object> {
            { "Symbol", typeof(string) },
            { "Price", typeof(double) },
            { "Volume", typeof(long) }
        });
        _runtime = EPRuntimeProvider.GetRuntime($"eventrep-{Guid.NewGuid()}", config);

        var module =
            "@public @buseventtype create json schema TradeJson(Symbol string, Price double, Volume long);\n" +
            "select Symbol, Price from TradeEvent where Price > 100;\n" +
            "select Symbol, Price from TradeMap where Price > 100;\n" +
            "select Symbol, Price from TradeJson where Price > 100;\n";
        var compiled = EPCompilerProvider.Compiler.Compile(module, new CompilerArguments(config));
        _runtime.DeploymentService.Deploy(compiled);

        _beanEvents = new TradeEvent[EventCount];
        _mapEvents  = new IDictionary<string, object>[EventCount];
        _jsonEvents = new string[EventCount];
        for (int i = 0; i < EventCount; i++)
        {
            var price = 100.0 + (i % 100);
            _beanEvents[i] = new TradeEvent { Symbol = "MSFT", Price = price, Volume = 1000L, Timestamp = i };
            _mapEvents[i] = new Dictionary<string, object> {
                { "Symbol", "MSFT" },
                { "Price", price },
                { "Volume", 1000L }
            };
            _jsonEvents[i] = $"{{\"Symbol\":\"MSFT\",\"Price\":{price},\"Volume\":1000}}";
        }
    }

    [Benchmark(Baseline = true)]
    public void SendBean()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
            svc.SendEventBean(_beanEvents[i], "TradeEvent");
    }

    [Benchmark]
    public void SendMap()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
            svc.SendEventMap(_mapEvents[i], "TradeMap");
    }

    [Benchmark]
    public void SendJson()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
            svc.SendEventJson(_jsonEvents[i], "TradeJson");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
