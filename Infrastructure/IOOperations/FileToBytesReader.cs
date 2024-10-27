using Infrastructure.Concurrency;

namespace Infrastructure.IOOperations;

//There is Microsoft.IO.RecyclableMemoryStream (https://github.com/Microsoft/Microsoft.IO.RecyclableMemoryStream) and possibly a bunch of other options.
//This implementation was mainly done for fun and some experiments.
//See also Pro .NET Memory Management, by Konrad Kokosa. pp.466: Creating Streams - Use RecyclableMemoryStream
//Writing High-Performance .NET Code 2nd Edition, by Ben Watson (Author)
public class FileToBytesReader<T> : IBytesProducer<T>
{
    private readonly CancellationToken _cancellationToken;
    private readonly FileStream _stream;
    private readonly int _bufferSize = 4096;
    private readonly AsyncLock _lock;

    public static FileToBytesReader<T> Create(string fullFileName, CancellationToken cancellationToken = new())
    {
        _ = Guard.FileExist(fullFileName);
        return new FileToBytesReader<T>(fullFileName, cancellationToken);
    }

    private FileToBytesReader(string fullFileName, CancellationToken cancellationToken = new())
    {
        _cancellationToken = cancellationToken;
        _stream = new FileStream(fullFileName, FileMode.Open, FileAccess.Read, FileShare.None,
            bufferSize: _bufferSize, useAsync: true);
        _lock = new AsyncLock();
    }

    public async Task<DataChunkContainer<T>> WriteBytesToBufferAsync(DataChunkContainer<T> dataChunkPackage)
    {
        Result<(int Size, int ActuallyRead)> result;

        //There are plans to use it from different threads
        using (var _ = await _lock.LockAsync())
        {
            result = await ReadBytesAsync(dataChunkPackage.RowData, dataChunkPackage.PrePopulatedBytesLength,
                _cancellationToken);
        }

        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);

        var nextPackage = result.Value.ActuallyRead == 0
            ? dataChunkPackage with { IsLastPart = true, WrittenBytesLength = result.Value.Size }
            : dataChunkPackage with { WrittenBytesLength = result.Value.Size };
        return nextPackage;
    }

    public void Dispose()
    {
        _stream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
       await _stream.DisposeAsync();
    }

    private async Task<Result<(int Size, int ActuallyRead)>> ReadBytesAsync(byte[] buffer, int offset,
        CancellationToken cancellationToken)
    {
        int length = await _stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken);
        return Result<(int Size, int ActuallyRead)>.Ok((length + offset, length));
    }
}