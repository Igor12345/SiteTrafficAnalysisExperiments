﻿using System.Buffers;

namespace Infrastructure.DataStructures
{
    //not thread safe!
    public sealed class ExpandableStorage<T> : IDisposable
    {
        private int _chunkSize;
        private readonly int _minChunkSize;
        private int _lastBuffer = -1;
        private long _currentIndex;
        private readonly List<T[]> _buffers;
        private int _itemsNumber;

        public static ExpandableStorage<T> Empty => new(1);

        public ExpandableStorage(int chunkSize = 16)
        {
            _minChunkSize = Guard.Positive(chunkSize);
            _buffers = new List<T[]>();
        }

        public T this[long i]
        {
            get
            {
                //use resources for strings,
                //depends on the teams agreements
                if (_lastBuffer < 0)
                    throw new InvalidOperationException("The storage does not contain data.");
                //todo possible error, but I'm not expect it in real cases
                int buffer = (int)(i / _chunkSize);
                int position = (int)(i % _chunkSize);
                return _buffers[buffer][position];
            }
        }

        public int Count => _itemsNumber;

        public void CopyTo(T[] destination, int length)
        {
            for (int i = 0; i <= _lastBuffer; i++)
            {
                int from = i * _chunkSize;
                int right = (i + 1) * _chunkSize;
                int to = Math.Min(right, length);
                _buffers[i].AsSpan(..(to - from)).CopyTo(destination.AsSpan(from..to));
                if (right >= length)
                    break;
            }
        }

        public void Add(T item)
        {
            if (_lastBuffer < 0 || _currentIndex >= _chunkSize)
                RentSpace();
            _buffers[_lastBuffer][_currentIndex++] = item;
            _itemsNumber++;
        }

        public void Clear()
        {
            Dispose();
        }

        public void Dispose()
        {
            foreach (var buffer in _buffers)
            {
                ArrayPool<T>.Shared.Return(buffer);
            }
            _buffers.Clear();
            _lastBuffer = -1;
            _currentIndex = 0;
            _itemsNumber = 0;
        }

        private void RentSpace()
        {
            T[] array = ArrayPool<T>.Shared.Rent(_minChunkSize);
            _chunkSize = array.Length;
            Interlocked.Increment(ref _lastBuffer);
            _buffers.Add(array);
            _currentIndex = 0;
        }
    }
}
