using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace dotnet_filesystem_benchmark;

[MemoryDiagnoser]
[RankColumn]
public class FileReadAllTest
{
    private readonly string _filePath = Path.GetTempFileName();

    [GlobalSetup]
    public void Setup()
    {
        File.WriteAllText(_filePath, string.Join("\n", Enumerable.Repeat("Hello World!!!!", 10000000)));
        ThreadPool.SetMinThreads(4, 4);
        ThreadPool.SetMaxThreads(4, 4);
    }

    [GlobalCleanup]
    public void Cleanup() => File.Delete(_filePath);
    [Benchmark]
    public byte[] ReadAll() => File.ReadAllBytes(_filePath);

    [Benchmark]
    public Task<byte[]> ReadAllAsync()
#if NETCOREAPP2_0_OR_GREATER
        => File.ReadAllBytesAsync(_filePath);
#else
        => throw new NotSupportedException("File.ReadAllBytesAsync is not supported");
#endif

    [Benchmark]
    public Task<byte[][]> ReadAllParallel()
        => Task.WhenAll(Enumerable.Range(0, 32).Select(_ => Task.Run(() => File.ReadAllBytes(_filePath))));

    [Benchmark]
    public Task<byte[][]> ReadAllAsyncParallel()
#if NETCOREAPP2_0_OR_GREATER
        => Task.WhenAll(Enumerable.Range(0, 32).Select(_ => Task.Run(() => File.ReadAllBytesAsync(_filePath))));
#else
        => throw new NotSupportedException("File.ReadAllBytesAsync is not supported");
#endif
}
