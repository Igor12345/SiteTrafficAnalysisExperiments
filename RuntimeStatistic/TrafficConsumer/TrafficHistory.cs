using System.Collections.Concurrent;
using Infrastructure;
using LogsAnalyzer.LogEntries;
using userId = ulong;
using pageId = uint;

namespace RuntimeStatistic.TrafficConsumer;

//will assume there significantly are fewer pages than visitors, per epoch 
public class TrafficHistory
{
    private readonly Func<DateTime, int> _epochIdentifier;
    private readonly Dictionary<pageId, HashSet<userId>> _pagesHistory = new();
    private readonly ConcurrentDictionary<userId, uint> _usersStatistic = new();
    private readonly ConcurrentDictionary<int, object> _lockers = new();
    
    public TrafficHistory(Func<DateTime, int> epochIdentifier)
    {
        _epochIdentifier = Guard.NotNull(epochIdentifier);
    }

    public void AddOrUpdate(LogEntry logEntry)
    {
        int epoch = _epochIdentifier(logEntry.DateTime);
        var locker = _lockers.GetOrAdd(epoch, new object());
        bool newVisit;
        lock (locker)
        {
            if (!_pagesHistory.TryGetValue(logEntry.PageId, out var visits))
            {
                _pagesHistory[logEntry.PageId] = visits = new();
            }
            newVisit = visits.Add(logEntry.CustomerId);
        }

        if (newVisit)
        {
            _usersStatistic.AddOrUpdate(logEntry.CustomerId, 1, (_, count) => count + 1);
        }
    }

    public uint GetUniqueVisits(userId customerId)
    {
        if(_usersStatistic.TryGetValue(customerId, out var count))
            return count;
        return 0;
    }
}