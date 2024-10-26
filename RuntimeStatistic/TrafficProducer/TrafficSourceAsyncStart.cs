using System.Threading.Channels;

namespace RuntimeStatistic.TrafficProducer;

public sealed class TrafficSourceAsyncStart<T>
{
    private readonly IEventsGenerator<T> _generator;
    private readonly ChannelWriter<T> _writer;
    private readonly uint _period;

    public TrafficSourceAsyncStart(IEventsGenerator<T> generator, Channel<T> channel, uint period)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _period = period;
        _writer = channel.Writer;
    }

    public async Task StartGenerationAsync(CancellationToken cancellationToken)
    {
        PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_period));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            T eventRecord = _generator.Next();
            await _writer.WriteAsync(eventRecord, cancellationToken);
        }

        _writer.Complete();
    }
}