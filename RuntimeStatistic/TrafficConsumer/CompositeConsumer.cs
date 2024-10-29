using System.Diagnostics;

namespace RuntimeStatistic.TrafficConsumer;

public class CompositeConsumer<T, T1> : IConsumer<T>
{
    private readonly Func<T, T1> _handler;
    private readonly Stopwatch _sw;
    private IConsumer<T1>? _next;

    private CompositeConsumer(Func<T, T1> handler, Stopwatch sw)
    {
        _handler = handler;
        _sw = sw;
    }

    private CompositeConsumer(Action<T> action, Stopwatch sw)
    {
        _handler = t => { action(t); return default; };
    }

    public static CompositeConsumer<T, T1> Create(Func<T, T1> handler, Stopwatch? sw = null)
    {
        return new CompositeConsumer<T, T1>(handler, sw??new Stopwatch());
    }

    public static IConsumer<T> Create(Action<T> handler)
    {
        return new CompositeConsumer<T, T1>(handler, new Stopwatch());
    }

    public CompositeConsumer<T1, T2> Then<T2>(Func<T1, T2> handler)
    {
        var next = new CompositeConsumer<T1, T2>(handler, _sw);
        _next = next;
        return next;
    } 

    public IConsumer<T1> Then(Action<T1> handler)
    {
        var next = new CompositeConsumer<T1, object>(handler, _sw);
        _next = next;
        return next;
    } 
   
    public void Consume(T item)
    {
        long ms1  = _sw.ElapsedMilliseconds;
        T1 result = _handler(item);
        long ms2  = _sw.ElapsedMilliseconds;
        _next?.Consume(result);
        long ms3  = _sw.ElapsedMilliseconds;
        
        Console.WriteLine($"Processing {ms1} : {ms2} : {ms3} for {item}");
    }
}