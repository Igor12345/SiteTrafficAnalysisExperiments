using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace RuntimeStatistic.TrafficProducer;

public sealed class EventSourceUsingRx<T>
{
    private readonly IEventsGenerator<T> _generator;
    private readonly TimeSpan _interval;
    private readonly List<(IEventsGenerator<T> generator, TimeSpan interval)> _otherGenerators = new();
    public IScheduler Scheduler { get; init; } = System.Reactive.Concurrency.Scheduler.Default;
    public Action<T>? DoOnEveryEvent { get; init; } = _ => { };

    internal void AddAdditionalGenerator(IEventsGenerator<T> generator, TimeSpan interval)
    {
        //todo check doubles
        _otherGenerators.Add((generator, interval));
    }

    internal EventSourceUsingRx(IEventsGenerator<T> generator, TimeSpan interval)
    {
        _generator = generator;
        _interval = interval;
    }

    public IObservable<T> Run()
    {
        var observable = Observable.Interval(_interval, Scheduler).Select(_ => _generator.Next());

        //can be used to produce errors, etc.
        foreach (var source in _otherGenerators)
        {
            var secondaryObservable = Observable.Interval(source.interval)
                .Select(_ => source.generator.Next());
            observable = observable.Merge(secondaryObservable);
        }

        return observable.Do(ExecuteActions);
    }

    private void ExecuteActions(T obj)
    {
        DoOnEveryEvent?.Invoke(obj);
    }
}