using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace NEsper.Benchmarks.EndToEnd;

/// <summary>
/// Shared job matrix for all end-to-end benchmarks: 3 warmup + 10 measurement iterations,
/// composed from two environment variables (both set by run-benchmarks.ps1 flags). Do not
/// pass BenchmarkDotNet's --runtimes with this attribute — it does not filter these jobs,
/// it adds an extra default-configured job that pollutes the results table.
///
/// NESPER_BENCH_GC — GC mode:
///   (unset) / "workstation"  — Workstation GC jobs only (default; matches the
///                              committed baseline in PERF-BENCHMARKS.md)
///   "server"                 — Server GC jobs only (production-representative)
///   "both"                   — both GC modes, for side-by-side comparison
///
/// NESPER_BENCH_RUNTIMES — target runtimes:
///   (unset) / "both"         — net8.0 + net9.0 (default)
///   "net8.0" / "net9.0"      — single runtime
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class EndToEndJobsAttribute : Attribute, IConfigSource
{
    public IConfig Config { get; }

    public EndToEndJobsAttribute()
    {
        var gcMode = ReadEnv("NESPER_BENCH_GC", "workstation");
        var runtimeMode = ReadEnv("NESPER_BENCH_RUNTIMES", "both");

        if (gcMode is not ("workstation" or "server" or "both"))
            throw new InvalidOperationException(
                $"Unrecognized NESPER_BENCH_GC value '{gcMode}'. Use: workstation, server, or both.");
        if (runtimeMode is not ("net8.0" or "net9.0" or "both"))
            throw new InvalidOperationException(
                $"Unrecognized NESPER_BENCH_RUNTIMES value '{runtimeMode}'. Use: net8.0, net9.0, or both.");

        var config = ManualConfig.CreateEmpty();

        if (gcMode is "workstation" or "both")
        {
            AddJobs(config, runtimeMode, gcServer: false);
        }

        if (gcMode is "server" or "both")
        {
            AddJobs(config, runtimeMode, gcServer: true);
        }

        Config = config;
    }

    private static string ReadEnv(string name, string defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(name)?.Trim().ToLowerInvariant();
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }

    private static void AddJobs(ManualConfig config, string runtimeMode, bool gcServer)
    {
        var gcSuffix = gcServer ? "ServerGC" : "WorkstationGC";
        if (runtimeMode is "net8.0" or "both")
            config.AddJob(MakeJob(CoreRuntime.Core80, "net8.0", gcServer, gcSuffix));
        if (runtimeMode is "net9.0" or "both")
            config.AddJob(MakeJob(CoreRuntime.Core90, "net9.0", gcServer, gcSuffix));
    }

    private static Job MakeJob(
        Runtime runtime,
        string runtimeName,
        bool gcServer,
        string gcSuffix)
    {
        return Job.Default
            .WithRuntime(runtime)
            .WithWarmupCount(3)
            .WithIterationCount(10)
            .WithGcServer(gcServer)
            .WithId($"{runtimeName}-{gcSuffix}");
    }
}
