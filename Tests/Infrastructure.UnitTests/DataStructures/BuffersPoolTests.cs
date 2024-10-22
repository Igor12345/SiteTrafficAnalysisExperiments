using Infrastructure.DataStructures;
using Infrastructure.IOOperations;

namespace Infrastructure.UnitTests.DataStructures
{
   public class BuffersPoolTests
   {
      [Test]
      public async Task ShouldProvideExactlyRequiredNumberOfBuffers()
      {
         int poolSize = 3;
         BuffersPool<int> pool = new BuffersPool<int>(poolSize);
         HashSet<DataChunkContainer<int>> containers = new HashSet<DataChunkContainer<int>>();
         for (int i = 0; i < poolSize; i++)
         {
            DataChunkContainer<int> conteiner = await pool.GetNextAsync();
            containers.Add(conteiner);
         }
         
         Assert.That(containers.Count, Is.EqualTo(poolSize));

         CancellationTokenSource cts = new CancellationTokenSource(100);
         Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
         Task[] tasks = [pool.GetNextAsync(), timeoutTask];

         var onlyOne = await Task.WhenAny(tasks);
         Assert.That(onlyOne, Is.EqualTo(tasks[1]));
      }

      [Test]
      public async Task ShouldReturnTheSameBufferAfterReleaseIt()
      {
         int poolSize = 3;
         BuffersPool<int> pool = new BuffersPool<int>(poolSize);
         HashSet<DataChunkContainer<int>> containers = new HashSet<DataChunkContainer<int>>();
         for (int i = 0; i < poolSize; i++)
         {
            DataChunkContainer<int> container = await pool.GetNextAsync();
            containers.Add(container);
         }

         Assert.That(containers.Count, Is.EqualTo(poolSize));

         pool.ReleaseContainer(containers.First());
         var anotherContainer = await pool.GetNextAsync();
         
         Assert.That(anotherContainer, Is.Not.Null);
         
         bool oneOf = false;
         foreach (DataChunkContainer<int> container in containers)
         {
            if (object.ReferenceEquals(anotherContainer.RowData, container.RowData))
            {
               oneOf = true;
               break;
            }
         }
         Assert.That(oneOf, Is.True);
      }

      [Test]
      public void ShouldAllowOnlyPositivePoolSize()
      {
         Assert.Throws<ArgumentOutOfRangeException>(() => new BuffersPool<int>(-3));
      }
   }
}
