using Infrastructure.DataStructures;

namespace Infrastructure.UnitTests.DataStructures
{
    public class ExpandableStorageTests
    {
        [Test]
        public void ShouldBeAbleToAddItems()
        {
            ExpandableStorage<int> storage = new ExpandableStorage<int>();

            for (int i = 0; i < 4; i++)
            {
                storage.Add(i);
                Assert.That(storage.Count, Is.EqualTo(i + 1));
            }
        }

        [Test]
        public void ShouldBeAbleToAddMoreItemsThanChunkSize()
        {
            ExpandableStorage<int> storage = new ExpandableStorage<int>(2);

            for (int i = 0; i < 4000; i++)
            {
                storage.Add(i);
            }

            Assert.That(storage.Count, Is.EqualTo(4000));
        }

        [Test]
        public void ShouldKeepAndReturnsItems()
        {
            ExpandableStorage<int> storage = new ExpandableStorage<int>();

            for (int i = 0; i < 4; i++)
            {
                storage.Add(2 * i);
            }

            for (int i = 0; i < 4; i++)
            {
                int value = storage[i];
                Assert.That(value, Is.EqualTo(i * 2));
            }
        }

        [Test]
        public void ShouldCopyItemsToDestination()
        {
            ExpandableStorage<int> storage = new ExpandableStorage<int>();

            int itemsToKeep = 40;
            for (int i = 0; i < itemsToKeep; i++)
            {
                storage.Add(2 * i);
            }

            int[] buffer = new int[itemsToKeep];
            int itemsToCopy = 19;
            storage.CopyTo(buffer, itemsToCopy);

            for (int i = 0; i < itemsToKeep; i++)
            {
                Assert.That(buffer[i], Is.EqualTo(i < itemsToCopy ? i * 2 : 0));
            }
        }

        [Test]
        public void ShouldClean()
        {
            ExpandableStorage<int> storage = new ExpandableStorage<int>();

            int itemsToKeep = 40;
            for (int i = 0; i < itemsToKeep; i++)
            {
                storage.Add(2 * i);
            }

            storage.Clear();

            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = storage[0];
            });

            storage.Add(5);
            Assert.That(storage[0], Is.EqualTo(5));
        }
    }
}
