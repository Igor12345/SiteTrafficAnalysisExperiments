namespace RuntimeStatistic.TrafficConsumer;

public class CompositeConsumer<T, T1> : IConsumer<T>
{
    private readonly Func<T, T1> _handler;
    private IConsumer<T1>? _next;

    private CompositeConsumer(Func<T, T1> handler)
    {
        _handler = handler;
    }

    private CompositeConsumer(Action<T> action)
    {
        _handler = t => { action(t); return default; };
    }

    public static CompositeConsumer<T, T1> Create(Func<T, T1> handler)
    {
        return new CompositeConsumer<T, T1>(handler);
    }

    public static IConsumer<T> Create(Action<T> handler)
    {
        return new CompositeConsumer<T, T1>(handler);
    }

    public CompositeConsumer<T1, T2> Then<T2>(Func<T1, T2> handler)
    {
        var next = new CompositeConsumer<T1, T2>(handler);
        _next = next;
        return next;
    } 

    public IConsumer<T1> Then(Action<T1> handler)
    {
        var next = new CompositeConsumer<T1, object>(handler);
        _next = next;
        return next;
    } 
   
    public void Consume(T item)
    {
        T1 result = _handler(item);
        _next?.Consume(result);
    }
}