using System.Collections.Concurrent;
using Infrastructure;
using LogsAnalyzer.LogEntries;

namespace RuntimeStatistic.TrafficConsumer;

//will assume there significantly are less pages than visitors, per epoch 
public class TrafficHistory
{
    private readonly Func<DateTime, int> _epochIdentifier;
    Dictionary<uint, HashSet<ulong>> _pagesHistory = new();
    ConcurrentDictionary<ulong, uint> _usersStatistic = new();
    ConcurrentDictionary<int, object> _lockers = new();
    
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
            _usersStatistic.AddOrUpdate(logEntry.CustomerId, 0, (id, count) => count + 1);
        }
    }

    public uint GetUniqueVisits(ulong customerId)
    {
        if(_usersStatistic.TryGetValue(customerId, out var count))
            return count;
        return 0;
    }


}