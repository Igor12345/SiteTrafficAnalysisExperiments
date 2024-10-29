using System.Diagnostics;
using System.Threading.Channels;

namespace RuntimeStatistic.TrafficConsumer
{
   public sealed class TrafficReader<T>
   {
      private readonly IConsumer<T> _consumer;
      private readonly Stopwatch _sw;
      private readonly ChannelReader<T> _reader;

      public TrafficReader(Channel<T> channel, IConsumer<T> consumer, Stopwatch? sw = null)
      {
         _reader = channel.Reader;
         _consumer = consumer;
         _sw = sw ?? new Stopwatch();
      }

      public async Task ReadAsync(CancellationToken cancellationToken)
      {
         while (true)
         {
            if (!await _reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
               break;

            //todo think TryRead
            T item = await _reader.ReadAsync(cancellationToken);
            Console.WriteLine($"ReadAsync {_sw.ElapsedMilliseconds} ticks for {item}, Thread: {Thread.CurrentThread.ManagedThreadId}");
            _consumer.Consume(item);
         }
      }
   }
}
