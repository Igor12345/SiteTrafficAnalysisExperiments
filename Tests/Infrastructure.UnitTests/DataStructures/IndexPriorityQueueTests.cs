using Infrastructure.DataStructures;

namespace Infrastructure.UnitTests.DataStructures
{
    public class IndexPriorityQueueTests
    {
        [Test]
        public void QueueCanBeCreated()
        {
            IndexPriorityQueue<int> queue =
                new IndexPriorityQueue<int>(4);

            Assert.That(queue, Is.Not.Null);
            Assert.That(queue.Capacity, Is.EqualTo(4));
            Assert.That(queue.Any(), Is.False);
        }

        [Test]
        public void ShouldNotCreateQueueWithNegativeCapacity()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { _ = new IndexPriorityQueue<int>(-4); });
        }

        [Test]
        public void ShouldBeAbleToAddValuesToQueue()
        {
            IndexPriorityQueue<int> queue = CreateSampleQueue([(23, 4)]);

            Assert.That(queue.Any(), Is.True);
        }

        [Test]
        public void ShouldBeAbleToPeekValuesFromQueueMultipleTimes()
        {
            IndexPriorityQueue<int> queue = CreateSampleQueue([(23, 4)]);

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
            IndexPriorityQueue<int> queue = CreateSampleQueue([(23, 4)]);

            (int v, int k) = queue.Dequeue();
            Assert.Multiple(() =>
            {
                Assert.That(v, Is.EqualTo(23));
                Assert.That(k, Is.EqualTo(4));
            });

            Assert.False(queue.Any());
        }

        [Test]
        public void ShouldOrderValues()
        {
            IndexPriorityQueue<int> queue =
                new IndexPriorityQueue<int>(4);

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
        [TestCase(true)]
        [TestCase(false)]
        public void ShouldUseProvidedComparer(bool regularOrder)
        {
            (int, int)[] input =
            [
                (4, 2),
                (5, 4),
                (1, 7),
                (2, 3)
            ];
            IComparer<int> comparer = regularOrder ? Comparer<int>.Default : new ReverseComparer();
            IndexPriorityQueue<int> queue = CreateSampleQueue(input, comparer);

            List<int> sortedValues = new List<int>();
            while (queue.Any())
            {
                sortedValues.Add(queue.Dequeue().value);
            }

            Assert.That(sortedValues.Count, Is.EqualTo(4));

            int[] expected = regularOrder ? [1, 5, 2, 4] : [4, 2, 5, 1];
            CollectionAssert.AreEqual(expected, sortedValues);
        }
        
        [Test]
        public void ShouldReturnTopElementsFromEmptyQueue()
        {
            IndexPriorityQueue<int> queue = new IndexPriorityQueue<int>(42);

            int someNumber = 4;
            int[]? sortedValues = queue.Peek(someNumber)?.Select(t => t.value).ToArray();
            
            Assert.That(sortedValues, Is.Not.Null);
            Assert.That(sortedValues.Count, Is.EqualTo(0));
        }
        
        [Test]
        public void ShouldReturnTopKElements()
        {
            (int, int)[] input =
            [
                (4, 2),
                (5, 4),
                (1, 7),
                (2, 3),
                (3, 6),
            ];
            IndexPriorityQueue<int> queue = CreateSampleQueue(input);

            int lessThanSize = 4;
            var sortedValuesPartOf = queue.Peek(lessThanSize).Select(t => t.value).ToArray();
            int[] expected1 = [1, 3, 5, 2];
            Assert.That(sortedValuesPartOf.Count, Is.EqualTo(lessThanSize));
            CollectionAssert.AreEqual(expected1, sortedValuesPartOf);
            
            int moreThanSize = 7;
            var sortedValuesAllOf = queue.Peek(moreThanSize).Select(t => t.value).ToArray();
            int[] expected2 = [1, 3, 5, 2, 4];
            Assert.That(sortedValuesAllOf.Count, Is.EqualTo(queue.Count()));
            CollectionAssert.AreEqual(expected2, sortedValuesAllOf);
        }
        
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ShouldBeAbleUpdateItemPriority(bool regularOrder)
        {
            (int, int)[] input =
            [
                (4, 2),
                (5, 8),
                (1, 14),
                (2, 5),
                (3, 11)
            ];
            IComparer<int> comparer = regularOrder ? Comparer<int>.Default : new ReverseComparer();
            IndexPriorityQueue<int> queue = CreateSampleQueue(input, comparer);

            int moreThanSize = 7;
            var sortedValues = queue.Peek(moreThanSize).Select(t => t.value).ToArray();

            int[] result = [1, 3, 5, 2, 4];
            int[] expected = regularOrder ? result : result.Reverse().ToArray();
            CollectionAssert.AreEqual(expected, sortedValues);

            queue.Update(2, 6);
            queue.Update(5, 12);
            sortedValues = queue.Peek(moreThanSize).Select(t => t.value).ToArray();
            int[] updatedResult = [1, 5, 3, 2, 4];
            int[] updatedExpected = regularOrder ? updatedResult : updatedResult.Reverse().ToArray();
            CollectionAssert.AreEqual(updatedExpected, sortedValues);
            
            queue.Update(3, 4);
            sortedValues = queue.Peek(moreThanSize).Select(t => t.value).ToArray();
            updatedResult = [1, 5, 2, 3, 4];
            updatedExpected = regularOrder ? updatedResult : updatedResult.Reverse().ToArray();
            CollectionAssert.AreEqual(updatedExpected, sortedValues);
        }

        private static IndexPriorityQueue<T> CreateSampleQueue<T>((T, int)[] items,
            IComparer<int>? comparer = null) where T : notnull
        {
            comparer ??= Comparer<int>.Default;
            IndexPriorityQueue<T> queue =
                new IndexPriorityQueue<T>(items.Length * 2, comparer);

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