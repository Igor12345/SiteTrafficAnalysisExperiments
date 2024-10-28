using System.Buffers;

namespace Infrastructure.DataStructures
{
    //can be internal if make this assembly friendly for tests
    //There is an "official" implementation: System.Collections.Generic.PriorityQueue
    //but in this case it was easier to use custom
    //Algorithms (4th Edition) by Robert Sedgewick, Kevin Wayne
    public sealed class IndexPriorityQueue<T>
        where T : notnull
    {
        private readonly IComparer<int> _comparer;
        private readonly int _capacity;
        private readonly (T value, int priority)[] _items;
        private readonly Dictionary<T, int> _references = new();
        private readonly bool _rented;
        private int _currentSize = -1;
        private readonly object _locker = new();

        public IndexPriorityQueue(int capacity, IComparer<int>? comparer = null)
        {
            _capacity = Guard.Positive(capacity);
            _items = new (T value, int priority)[Capacity];
            _comparer = comparer ?? Comparer<int>.Default;
        }

        private IndexPriorityQueue((T value, int priority)[] items, int currentSize, IComparer<int> comparer)
        {
            _items = items;
            _rented = true;
            _comparer = comparer;
            _currentSize = currentSize;
        }

        public int Capacity => _capacity;

        public int Count() => _currentSize + 1;

        public void Enqueue(T item, int key)
        {
            lock (_locker)
            {
                if (_references.ContainsKey(item))
                {
                    Update(item, key);
                    return;
                }

                _items[++_currentSize] = (item, key);
                _references.Add(item, _currentSize);
                Swim(_currentSize);
            }
        }

        public void Enqueue((T value, int priority) value)
        {
            Enqueue(value.value, value.priority);
        }

        public (T value, int priority) Dequeue()
        {
            if (!Any())
                throw new InvalidOperationException("There are no items in the queue.");
            (T value, int priority) top;
            lock (_locker)
            {
                top = _items[0];
                Exchange(0, _currentSize);
                _items[_currentSize--] = default;
                Sink(0);
            }
            return top;
        }

        public (T value, int priority) Peek()
        {
            if (!Any())
                throw new InvalidOperationException("There are no items in the queue.");
            return _items[0];
        }

        public (T value, int priority)[] Peek(int k)
        {
            IndexPriorityQueue<T> copyOfQueue = CreateCopy();
            int length = Math.Min(k, _currentSize + 1);

            var result = new (T value, int priority)[length];
            for (int i = 0; i < length; i++)
            {
                var top = copyOfQueue.Dequeue();
                result[i] = top;
            }

            copyOfQueue.Release();
            return result;
        }

        public bool Any() => _currentSize >= 0;

        public void Update(T item, int priority)
        {
            if (!_references.TryGetValue(item, out var ind))
                throw new InvalidOperationException($"The item {item} does not exist in the queue.");

            lock (_locker)
            {
                int prevPriority = _items[ind].priority;
                if (_comparer.Compare(prevPriority, priority) == 0)
                    return;

                _items[ind] = (item, priority);
                bool moveUp = _comparer.Compare(priority, prevPriority) > 0;
                if (moveUp)
                    Swim(ind);
                else
                    Sink(ind);
            }
        }

        private IndexPriorityQueue<T> CreateCopy()
        {
            (T, int)[] copyOfItems = ArrayPool<(T, int)>.Shared.Rent(_items.Length);

            //todo should be used in every operation
            lock (_locker)
            {
                _items.CopyTo(copyOfItems, 0);
            }

            return new IndexPriorityQueue<T>(copyOfItems, _currentSize, _comparer);
        }

        private void Exchange(int left, int right)
        {
            (_items[left], _items[right]) = (_items[right], _items[left]);
            _references[_items[left].value] = left;
            _references[_items[right].value] = right;
        }

        private bool Less(int i, int j)
        {
            return _comparer.Compare(_items[i].priority, _items[j].priority) < 0;
        }

        private void Swim(int k)
        {
            while (k > 0 && Less(k / 2, k))
            {
                Exchange(k / 2, k);
                k = k / 2;
            }
        }

        private void Sink(int k)
        {
            while (2 * k <= _currentSize)
            {
                int j = 2 * k;
                if (j < _currentSize && Less(j, j + 1))
                    j++;
                if (!Less(k, j))
                    break;
                Exchange(k, j);
                k = j;
            }
        }

        private void Release()
        {
            if (_rented)
                ArrayPool<(T, int)>.Shared.Return(_items);
        }
    }
}