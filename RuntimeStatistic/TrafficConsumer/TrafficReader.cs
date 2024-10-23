using System.Threading.Channels;

namespace RuntimeStatistic.TrafficConsumer
{
   public sealed class TrafficReader<T>
   {
      private readonly IConsumer<T> _consumer;
      private readonly ChannelReader<T> _reader;

      public TrafficReader(Channel<T> channel, IConsumer<T> consumer)
      {
         _reader = channel.Reader;
         _consumer = consumer;
      }

      public async Task ReadAsync(CancellationToken cancellationToken)
      {
         while (true)
         {
            if (!await _reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
               break;

            //todo think TryRead
            T item = await _reader.ReadAsync(cancellationToken);
            _consumer.Consume(item);
         }
      }
   }
}
