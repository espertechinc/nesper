using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Join;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class TwoStreamJoinBenchmark
{
    private static readonly string[] Symbols = { "AAPL", "GOOG", "MSFT", "AMZN", "NFLX" };

    private EPRuntime    _runtime     = null!;
    private TradeEvent[] _tradeEvents = null!;
    private QuoteEvent[] _quoteEvents = null!;

    [Params(1_000, 10_000)]
    public int EventCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        config.Common.AddEventType(typeof(QuoteEvent));
        _runtime = EPRuntimeProvider.GetRuntime($"join-{Guid.NewGuid()}", config);

        var compiled = EPCompilerProvider.Compiler.Compile(
            "select t.Symbol, t.Price, q.BidPrice, q.AskPrice " +
            "from TradeEvent#length(100) as t, QuoteEvent#length(100) as q " +
            "where t.Symbol = q.Symbol",
            new CompilerArguments(config));
        _runtime.DeploymentService.Deploy(compiled);

        _tradeEvents = new TradeEvent[EventCount];
        _quoteEvents = new QuoteEvent[EventCount];
        for (int i = 0; i < EventCount; i++)
        {
            var sym = Symbols[i % Symbols.Length];
            _tradeEvents[i] = new TradeEvent { Symbol = sym, Price = 100.0 + i, Volume = 1000L, Timestamp = i };
            _quoteEvents[i] = new QuoteEvent { Symbol = sym, BidPrice = 99.5 + i, AskPrice = 100.5 + i };
        }
    }

    [Benchmark]
    public void SendN()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
        {
            svc.SendEventBean(_tradeEvents[i], "TradeEvent");
            svc.SendEventBean(_quoteEvents[i], "QuoteEvent");
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
