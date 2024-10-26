using System.Reactive.Disposables;
using System.Threading.Channels;

namespace RuntimeStatistic.TrafficProducer
{
   public sealed class TrafficSource<T>
   {
      private readonly EventSourceUsingRx<T> _eventSource;
      private readonly ChannelWriter<T> _writer;

      public TrafficSource(EventSourceUsingRx<T> eventSource, Channel<T> channel)
      {
         _eventSource = eventSource ?? throw new ArgumentNullException(nameof(eventSource));
         _writer = channel.Writer;
      }

      public IDisposable StartGeneration()
      {
         var subscription = _eventSource.Run().Subscribe(WriteToChannel);
         CompositeDisposable disposable = new CompositeDisposable(
            Disposable.Create(subscription, sbscr => sbscr.Dispose()),
            Disposable.Create(_writer, wrt => wrt.Complete())
         );
         return disposable;
      }

      private void WriteToChannel(T obj)
      {
         _writer.TryWrite(obj);
      }
   }
}