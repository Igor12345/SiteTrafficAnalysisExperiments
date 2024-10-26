using System.Collections.ObjectModel;
using System.Reactive.Concurrency;

namespace RuntimeStatistic.TrafficProducer;

public class EventSourceBuilder
{
    public IEventSourceBuilder<T> Create<T>(Action<IEventSourceConfiguration<T>>? configurationDelegate)
    {
        EventSourceConfiguration<T> configuration = new EventSourceConfiguration<T>();
        configurationDelegate?.Invoke(configuration);
        BuilderInternal<T> builder = new BuilderInternal<T>(configuration);
        return builder;
    }

    public interface IEventSourceBuilder<T>
    {
        EventSourceUsingRx<T> Build();
    }

    public interface IEventSourceConfiguration<T>
    {
        void UseGenerator(IEventsGenerator<T> generator);
        void GenerateEventEvery(TimeSpan interval);
        void UseScheduler(IScheduler scheduler);
        void ExecuteOnEveryEvent(Action<T> action);
        void UseAdditionalGenerator(IEventsGenerator<T> generator, TimeSpan interval);
    }

    private class EventSourceConfiguration<T> : IEventSourceConfiguration<T>
    {
        private readonly List<Action<T>> _actions = new();
        public IEnumerable<Action<T>> Actions => new ReadOnlyCollection<Action<T>>(_actions);
        
        private readonly List<(IEventsGenerator<T> generator, TimeSpan interval)> _additionalGenerators = new();

        public IEnumerable<(IEventsGenerator<T> generator, TimeSpan interval)> AdditionalGenerators =>
            new ReadOnlyCollection<(IEventsGenerator<T>, TimeSpan)>(_additionalGenerators);
        public IEventsGenerator<T>? PrimaryGenerator { get; private set; }
        public TimeSpan Interval { get; private set; } = TimeSpan.FromSeconds(1);
        public IScheduler Scheduler { get; private set; } = System.Reactive.Concurrency.Scheduler.Default;

        public void UseGenerator(IEventsGenerator<T> generator)
        {
            PrimaryGenerator = generator ?? throw new ArgumentNullException(nameof(generator));
        }

        public void GenerateEventEvery(TimeSpan interval)
        {
            Interval = interval;
        }
        
        public void UseScheduler(IScheduler scheduler)
        {
            Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }
        
        public void ExecuteOnEveryEvent(Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            _actions.Add(action);
        }

        public void UseAdditionalGenerator(IEventsGenerator<T> generator, TimeSpan interval)
        {
            ArgumentNullException.ThrowIfNull(generator);
            _additionalGenerators.Add((generator, interval));
        }
    }

    private class BuilderInternal<T>(EventSourceConfiguration<T> configuration) : IEventSourceBuilder<T>
    {
        private readonly EventSourceConfiguration<T> _configuration =
            configuration ?? throw new ArgumentNullException(nameof(configuration));
        
        public EventSourceUsingRx<T> Build()
        {
            if (_configuration.PrimaryGenerator == null)
                throw new InvalidOperationException("Primary generator not configured");
            Action<T> doOnEveryEvent = _ => { };
            if (_configuration.Actions.Any())
            {
                if (_configuration.Actions.Count() == 1)
                {
                    doOnEveryEvent = _configuration.Actions.First();
                }
                else
                {
                    doOnEveryEvent = e =>
                    {
                        foreach (var action in _configuration.Actions)
                        {
                            action.Invoke(e);
                        }
                    };
                }
            }

            EventSourceUsingRx<T> eventSource =
                new EventSourceUsingRx<T>(_configuration.PrimaryGenerator, _configuration.Interval)
                {
                    DoOnEveryEvent = doOnEveryEvent,
                    Scheduler = _configuration.Scheduler
                };
            foreach (var additionalGenerator in _configuration.AdditionalGenerators)
            {
                eventSource.AddAdditionalGenerator(additionalGenerator.generator, additionalGenerator.interval);
            }

            return eventSource;
        }
    }
}