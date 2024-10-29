using System.Diagnostics;
using System.Threading.Channels;

namespace RuntimeStatistic.TrafficProducer;

public sealed class TrafficSourceAsyncStart<T>
{
    private readonly IEventsGenerator<T> _generator;
    private readonly ChannelWriter<T> _writer;
    private readonly TimeSpan _period;
    private readonly Stopwatch _sw;
    private readonly TimeProvider _timeProvider;

    public TrafficSourceAsyncStart(IEventsGenerator<T> generator, Channel<T> channel, TimeSpan period,
        TimeProvider? timeProvider = null, Stopwatch? sw = null)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _period = period;
        _sw = sw ?? new Stopwatch();
        _timeProvider = timeProvider ?? TimeProvider.System;
        _writer = channel.Writer;
    }

    public async Task StartGenerationAsync(CancellationToken cancellationToken)
    {
        PeriodicTimer timer = new PeriodicTimer(_period, _timeProvider);

        while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            T eventRecord = _generator.Next();
            long ticks = _sw.ElapsedMilliseconds;
            await _writer.WriteAsync(eventRecord, cancellationToken);
            long ticks2 = _sw.ElapsedMilliseconds;
            Console.WriteLine($"StartGenerationAsync before: {ticks}, after: {ticks2} for: {eventRecord}");
        }
        _writer.Complete();
    }
}