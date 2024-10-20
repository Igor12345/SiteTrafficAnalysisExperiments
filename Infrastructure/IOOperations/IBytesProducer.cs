namespace Infrastructure.IOOperations;

public interface IBytesProducer : IAsyncDisposable, IDisposable
{
    Task<byte[]> WriteBytesToBufferAsync();
}