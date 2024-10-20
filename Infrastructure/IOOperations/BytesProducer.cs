using Infrastructure.Concurrency;

namespace Infrastructure.IOOperations;

public class BytesProducer : IBytesProducer
{
    private readonly CancellationToken _cancellationToken;
    private readonly FileStream _stream;
    private readonly string _filePath;
    private readonly int _bufferSize = 4096;
    private readonly AsyncLock _lock;

    public BytesProducer(string fullFileName, CancellationToken cancellationToken = new())
    {
        _cancellationToken = cancellationToken;
        _filePath = Guard.FileExist(fullFileName);
        _stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.None,
            bufferSize: _bufferSize, useAsync: true);
        _lock = new AsyncLock();
    }

    public async Task<byte[]> WriteBytesToBufferAsync()
    {
        return await File.ReadAllBytesAsync(_filePath, _cancellationToken);
    }

    public async Task<DataChunkContainer> WriteBytesToBufferAsync(DataChunkContainer dataChunkPackage)
    {
        await Task.Yield();
        Result<(int Size, int ActuallyRead)> result;

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
        if (_stream != null) await _stream.DisposeAsync();
    }

    private async Task<Result<(int Size, int ActuallyRead)>> ReadBytesAsync(byte[] buffer, int offset,
        CancellationToken cancellationToken)
    {
        int length = await _stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken);
        return Result<(int Size, int ActuallyRead)>.Ok((length + offset, length));
    }
}