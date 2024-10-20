namespace Infrastructure.IOOperations;

public class BytesProducer : IBytesProducer
{
    private readonly CancellationToken _cancellationToken;
    private FileStream? _stream;
    private readonly string _filePath;
    private readonly int _bufferSize = 4096;

    public BytesProducer(string fullFileName, CancellationToken cancellationToken = new())
    {
        _cancellationToken = cancellationToken;
        _filePath = Guard.FileExist(fullFileName);
        // _stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.None,
        //     bufferSize: _bufferSize, useAsync: true);
    }

    public void Dispose()
    {
        _stream?.Dispose();
    }

    public async Task<byte[]> WriteBytesToBufferAsync()
    {

        return await File.ReadAllBytesAsync(_filePath, _cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_stream != null) await _stream.DisposeAsync();
    }
}