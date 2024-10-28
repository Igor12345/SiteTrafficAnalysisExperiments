using System.Threading.Channels;

namespace RuntimeStatistic.TrafficProducer;

public sealed class TrafficSourceAsyncStart<T>
{
    private readonly IEventsGenerator<T> _generator;
    private readonly ChannelWriter<T> _writer;
    private readonly TimeSpan _period;
    private readonly TimeProvider _timeProvider;

    public TrafficSourceAsyncStart(IEventsGenerator<T> generator, Channel<T> channel, TimeSpan period, TimeProvider? timeProvider = null)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _period = period;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _writer = channel.Writer;
    }

    public async Task StartGenerationAsync(CancellationToken cancellationToken)
    {
        PeriodicTimer timer = new PeriodicTimer(_period, _timeProvider);

        while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            T eventRecord = _generator.Next();
            Console.WriteLine($"--> Source generator - Created event {eventRecord}, thread {Thread.CurrentThread.ManagedThreadId}");
            await _writer.WriteAsync(eventRecord, cancellationToken);
        }
        _writer.Complete();
    }
}