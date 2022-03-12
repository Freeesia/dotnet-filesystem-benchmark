using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

#if DEBUG
var config = new DebugInProcessConfig();
#else
var job = Job.Dry;
var config = ManualConfig.Create(DefaultConfig.Instance)
    .AddJob(job.WithRuntime(ClrRuntime.Net48).AsBaseline())
    .AddJob(job.WithRuntime(CoreRuntime.Core31))
    .AddJob(job.WithRuntime(CoreRuntime.Core60));
#endif

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
