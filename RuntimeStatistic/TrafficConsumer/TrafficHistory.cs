using System.Collections.Concurrent;
using System.Diagnostics;
using Infrastructure;
using LogsAnalyzer.LogEntries;


namespace RuntimeStatistic.TrafficConsumer;

//will assume there significantly are fewer pages than visitors, per epoch 
public class TrafficHistory
{
    private readonly Stopwatch _sw;
    private readonly Func<DateTime, int> _epochIdentifier;
    private readonly Dictionary<UInt32, HashSet<UserId>> _pagesHistory = new();
    private readonly ConcurrentDictionary<UserId, uint> _usersStatistic = new();
    private readonly ConcurrentDictionary<int, object> _lockers = new();
    
    public TrafficHistory(Func<DateTime, int> epochIdentifier, Stopwatch? sw = null)
    {
        _sw = sw ?? new Stopwatch();
        _epochIdentifier = Guard.NotNull(epochIdentifier);
    }

    public void AddOrUpdate(LogEntry logEntry)
    {
        long ticks = _sw.ElapsedMilliseconds;
        int epoch = _epochIdentifier(logEntry.DateTime);
        var locker = _lockers.GetOrAdd(epoch, new object());
        bool newVisit;
        long ticks2 = 0;
        lock (locker)
        {
            ticks2 = _sw.ElapsedMilliseconds; 
            if (!_pagesHistory.TryGetValue(logEntry.PageId, out var visits))
            {
                _pagesHistory[logEntry.PageId] = visits = new();
            }
            newVisit = visits.Add(logEntry.CustomerId);
        }
        long ticks3 = _sw.ElapsedMilliseconds;

        if (newVisit)
        {
            _usersStatistic.AddOrUpdate(logEntry.CustomerId, 1, (_, count) => count + 1);
        }
        long ticks4 = _sw.ElapsedMilliseconds;
        Console.WriteLine($"TrafficHistory.AddOrUpdate start {ticks}, lock {ticks2}, after {ticks3}, exit {ticks4}");
    }

    public uint GetUniqueVisits(UserId customerId)
    {
        if(_usersStatistic.TryGetValue(customerId, out var count))
            return count;
        return 0;
    }
}