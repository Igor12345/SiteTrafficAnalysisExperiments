namespace Infrastructure.IOOperations;

public interface IBytesProducer<T> : IAsyncDisposable, IDisposable
{
    Task<DataChunkContainer<T>> WriteBytesToBufferAsync(DataChunkContainer<T> dataChunkPackage);
}