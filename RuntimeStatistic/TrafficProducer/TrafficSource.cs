using System.Threading.Channels;

namespace RuntimeStatistic.TrafficProducer
{
   public sealed class TrafficSource<T>
   {
      private readonly IEventsGenerator<T> _generator;
      private readonly ChannelWriter<T> _writer;


      public TrafficSource(IEventsGenerator<T> generator, Channel<T> channel)
      {
         _generator = generator;
         _writer = channel.Writer;
      }
      public async Task StartGenerationAsync(CancellationToken cancellationToken)
      {
         PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

         while (await timer.WaitForNextTickAsync(cancellationToken))
         {
            T eventRecord = _generator.Next();
            if (await _writer.WaitToWriteAsync(cancellationToken).ConfigureAwait(false))
               await _writer.WriteAsync(eventRecord, cancellationToken);
         }
         _writer.Complete();
      }
   }
}
