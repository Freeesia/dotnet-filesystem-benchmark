using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace dotnet_filesystem_benchmark;

[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[SimpleJob(RuntimeMoniker.HostProcess)]
[MemoryDiagnoser]
[RankColumn]
public class FileReadTest
{
    private const int OneKibibyte = 1 << 10; // 1024
    private const int FourKibibytes = OneKibibyte << 2; // default Stream buffer size
    private const int OneMibibyte = OneKibibyte << 10;
    private const int HundredMibibytes = OneMibibyte * 100;

    private readonly string _filePath = Path.GetTempFileName();

    [GlobalSetup]
    public void Setup() => File.WriteAllText(_filePath, string.Join("\n", Enumerable.Repeat("Hello World!!!!", 10000000)));

    [GlobalCleanup]
    public void Cleanup() => File.Delete(_filePath);
    [Benchmark]
    public byte[] ReadAll() => File.ReadAllBytes(_filePath);

#if NETCOREAPP2_0_OR_GREATER
    [Benchmark]
    public Task<byte[]> ReadAllAsync() => File.ReadAllBytesAsync(_filePath);
#endif

#if NET6_0_OR_GREATER
    [Benchmark]
    [Arguments(FourKibibytes, FileOptions.None, FourKibibytes)]
    [Arguments(FourKibibytes, FileOptions.None, 1)]
    [Arguments(FourKibibytes, FileOptions.Asynchronous, FourKibibytes)]
    [Arguments(FourKibibytes, FileOptions.Asynchronous, 1)]
    [Arguments(HundredMibibytes, FileOptions.None, FourKibibytes)]
    [Arguments(HundredMibibytes, FileOptions.None, 1)]
    [Arguments(HundredMibibytes, FileOptions.Asynchronous, FourKibibytes)]
    [Arguments(HundredMibibytes, FileOptions.Asynchronous, 1)]
    public async Task<long> ReadAsync(int userBufferSize, FileOptions options, int streamBufferSize)
    {
        var rootBuffer = ArrayPool<byte>.Shared.Rent(userBufferSize);
        var userBuffer = new Memory<byte>(rootBuffer)[..userBufferSize];
        long bytesRead = 0;
        using var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, streamBufferSize, options);
        int count;
        do
        {
            count = await fileStream.ReadAsync(userBuffer);
            bytesRead += count;
        } while (count > 0);
        ArrayPool<byte>.Shared.Return(rootBuffer);
        return bytesRead;
    }
#endif
}
