using Infrastructure.DataStructures;

namespace Infrastructure.UnitTests.DataStructures
{
    public class IndexPriorityQueueTests
    {
        [Test]
        public void QueueCanBeCreated()
        {
            IndexPriorityQueue<int, Comparer<int>> queue =
                new IndexPriorityQueue<int, Comparer<int>>(4, Comparer<int>.Default);

            Assert.That(queue, Is.Not.Null);
            Assert.That(queue.Capacity, Is.EqualTo(4));
            Assert.That(queue.Any(), Is.False);
        }

        [Test]
        public void ShouldNotCreateQueueWithNegativeCapacity()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new IndexPriorityQueue<int, Comparer<int>>(-4, Comparer<int>.Default));
        }

        [Test]
        public void ShouldBeAbleToAddValuesToQueue()
        {
            IndexPriorityQueue<int, IComparer<int>> queue = CreateSampleQueue(new[] { (23, 4) });

            Assert.That(queue.Any(), Is.True);
        }

        [Test]
        public void ShouldBeAbleToPeekValuesFromQueueMultipleTimes()
        {
            IndexPriorityQueue<int, IComparer<int>> queue = CreateSampleQueue(new[] { (23, 4) });

            (int v, int k) = queue.Peek();
            Assert.Multiple(() =>
            {
                Assert.That(v, Is.EqualTo(23));
                Assert.That(k, Is.EqualTo(4));
            });

            (int v1, int k1) = queue.Peek();
            Assert.Multiple(() =>
            {
                Assert.That(v1, Is.EqualTo(23));
                Assert.That(k1, Is.EqualTo(4));
            });
        }

        [Test]
        public void ShouldBeAbleDequeueFromQueueOnlyOnce()
        {
            IndexPriorityQueue<int, IComparer<int>> queue = CreateSampleQueue(new[] { (23, 4) });

            (int v, int k) = queue.Dequeue();
            Assert.Multiple(() =>
            {
                Assert.That(v, Is.EqualTo(23));
                Assert.That(k, Is.EqualTo(4));
            });

            Assert.False(queue.Any());
        }

        [Test]
        public void TheQueueShouldOrderValues()
        {
            IComparer<int> reverseComparer = new ReverseComparer();
            IndexPriorityQueue<int, IComparer<int>> queue =
                new IndexPriorityQueue<int, IComparer<int>>(4, reverseComparer);

            int value1 = 23;
            int value2 = 34;
            int key1 = 2;
            int key2 = 16;
            queue.Enqueue(value1, key1);
            queue.Enqueue(value2, key2);

            (int v, int k) = queue.Peek();
            Assert.That(v, Is.EqualTo(value2));
            Assert.That(k, Is.EqualTo(key2));

            (v, k) = queue.Dequeue();
            Assert.That(v, Is.EqualTo(value2));
            Assert.That(k, Is.EqualTo(key2));

            Assert.True(queue.Any());

            (v, k) = queue.Dequeue();
            Assert.That(v, Is.EqualTo(value1));
            Assert.That(k, Is.EqualTo(key1));

            Assert.False(queue.Any());
        }

        [Test]
        public void TheQueueShouldUseProvidedComparer()
        {
            (int, int)[] input =
            {
                (4, 2),
                (5, 3),
                (1, 4),
                (2, 3)
            };
            IndexPriorityQueue<int, IComparer<int>> queue = CreateSampleQueue(input, Comparer<int>.Default);

            List<int> sortedValues = new List<int>();
            while (queue.Any())
            {
                sortedValues.Add(queue.Dequeue().Item1);
            }

            Assert.That(sortedValues.Count, Is.EqualTo(4));

            int[] expected = [1,2,4,5];
            int i = 0;
            foreach (int item in sortedValues)
            {
                Assert.That(item, Is.EqualTo(expected[i++]));
            }
        }

        private static IndexPriorityQueue<T, IComparer<T>> CreateSampleQueue<T>((T, int)[] items,
            IComparer<T>? comparer = null)
        {
            comparer ??= Comparer<T>.Default;
            IndexPriorityQueue<T, IComparer<T>> queue =
                new IndexPriorityQueue<T, IComparer<T>>(4, comparer);

            foreach ((T, int) item in items)
            {
                queue.Enqueue(item);
            }

            return queue;
        }

        private class ReverseComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return -1 * Comparer<int>.Default.Compare(x, y);
            }
        }
    }
}