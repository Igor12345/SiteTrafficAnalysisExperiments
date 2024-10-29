using System.Diagnostics;
using Infrastructure;
using Infrastructure.DataStructures;
using LogsAnalyzer.LogEntries;

namespace RuntimeStatistic.TrafficConsumer;

//todo split responsibilities: update history, update priority queue
public class MostActiveUsersSelector
{
    private readonly TrafficHistory _history;
    private readonly IndexPriorityQueue<UserId> _activeUsers;

    public MostActiveUsersSelector(TrafficHistory history, int queueCapacity)
    {
        _history = history;
        _activeUsers = new IndexPriorityQueue<UserId>(queueCapacity);
    }

    public Result<LogEntry> Consume(Result<LogEntry> result, Stopwatch? sw = null)
    {
        sw ??= Stopwatch.StartNew();
        if (!result.Success)
            return result;

        long ticks = sw.ElapsedMilliseconds;
        var logEntry = result.Value;
        //todo a bit unsafe
        int visits = (int)_history.GetUniqueVisits(logEntry.CustomerId);
        
        long ticks2 = sw.ElapsedMilliseconds;
        _activeUsers.Enqueue(logEntry.CustomerId, visits);

        long ticks3 = sw.ElapsedMilliseconds;
        
        Console.WriteLine($"MostActiveUsersSelector: enter {ticks}, after history {ticks2}, after queue {ticks3}, Thread: {Thread.CurrentThread.ManagedThreadId}");
        return result;
    }

    public ulong[] GetMostActiveUsers(int number)
    {
        return _activeUsers.Peek(number).Select(item => item.value).ToArray();
    }
}