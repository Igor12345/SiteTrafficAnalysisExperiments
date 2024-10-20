using System.Collections.Concurrent;
using Infrastructure.DataStructures;

namespace Infrastructure.IOOperations;

public sealed class BuffersPool
{
    private readonly int _bufferSize = 1024 * 1024;//bytes
    private readonly int _poolSize = 4;
    private readonly ConcurrentStack<byte[]> _buffers = new();
    private readonly SemaphoreSlim _semaphore;
    private int _packageNumber = -1;
    private readonly int _recordChunksLength;
    static readonly object _locker = new();

    //In the next iteration of the implementation, free buffers will be filled from a file in a background process.
    //Therefore, a ConcurrentStack with several of them has already been created.This is only an experiment, not an industrial development.
    private readonly ConcurrentStack<ExpandableStorage<LogEntry>> _logEntriesStorages;


    public BuffersPool(int poolSize = 4)
    {
        _poolSize = Guard.Positive(poolSize);
        _semaphore = new SemaphoreSlim(_poolSize, _poolSize);
        _recordChunksLength = 500;
        //The most likely scenario is that storages for recognized lines will be returned
        //much faster than the buffers for the row bytes. In any case,
        //their size is much smaller than the size of the bytes array,
        //so creating a few extra storages won't be much harm.
        _logEntriesStorages = new ConcurrentStack<ExpandableStorage<LogEntry>>();
        Initialize();
    }

    private void Initialize()
    {
        //creating them all at the same time so that they are next to each other in LOH
        //another strategy can be creating them on demand in the method GetNextAsync 
        for (int i = 0; i < _poolSize; i++)
        {
            byte[] buffer = new byte[_bufferSize];
            _buffers.Push(buffer);
        }
    }

    public async Task<DataChunkContainer> GetNextAsync()
    {
        await _semaphore.WaitAsync();

        bool lockTaken = false;
        try
        {
            Monitor.Enter(_locker, ref lockTaken);
            bool bufferExists = _buffers.TryPop(out byte[]? buffer);
            if (!bufferExists)
            {
                //The semaphore ensures that we should not be here.
                throw new InvalidOperationException("Attempt to get more buffer instances than are in the pool.");
            }
            ExpandableStorage<LogEntry> linesStorage = RentLinesStorage();

            return new DataChunkContainer(buffer!, linesStorage, Interlocked.Increment(ref _packageNumber), false);
        }
        finally
        {
            if (lockTaken) Monitor.Exit(_locker);
        }
    }

    public void ReleaseContainer(DataChunkContainer container)
    {
        bool lockTaken = false;
        try
        {
            Monitor.Enter(_locker, ref lockTaken);

            _logEntriesStorages.Push(container.ParsedRecords);
            _buffers.Push(container.RowData);
            _semaphore.Release();
        }
        finally
        {
            if (lockTaken) Monitor.Exit(_locker);
        }
    }

    private ExpandableStorage<LogEntry> RentLinesStorage()
    {
        ExpandableStorage<LogEntry> storage;
        while (!_logEntriesStorages.TryPop(out storage!))
        {
            _logEntriesStorages.Push(new ExpandableStorage<LogEntry>(_recordChunksLength));
        }
        storage.Clear();
        return storage;
    }
}