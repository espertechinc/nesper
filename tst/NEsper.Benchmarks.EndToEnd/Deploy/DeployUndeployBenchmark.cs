// PERF_REVIEW: M5 — FilterServiceLockCoarse write-lock blocks all event evaluation during deploy/undeploy
using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Deploy;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class DeployUndeployBenchmark
{
    private EPRuntime  _runtime  = null!;
    private EPCompiled _compiled = null!;
    private TradeEvent _event    = null!;
    private string?    _deployId;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        _runtime  = EPRuntimeProvider.GetRuntime($"deploy-{Guid.NewGuid()}", config);
        _compiled = EPCompilerProvider.Compiler.Compile(
            "select * from TradeEvent where Price > 100.0",
            new CompilerArguments(config));
        _event = new TradeEvent { Symbol = "MSFT", Price = 105.0, Volume = 1000L, Timestamp = 0L };
    }

    [Benchmark]
    public void DeployProcessUndeploy()
    {
        var deployment = _runtime.DeploymentService.Deploy(_compiled);
        _deployId = deployment.DeploymentId;

        var svc = _runtime.EventService;
        for (int i = 0; i < 100; i++)
            svc.SendEventBean(_event, "TradeEvent");

        _runtime.DeploymentService.Undeploy(_deployId);
        _deployId = null;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (_deployId != null)
            _runtime.DeploymentService.Undeploy(_deployId);
        _runtime.Destroy();
    }
}
