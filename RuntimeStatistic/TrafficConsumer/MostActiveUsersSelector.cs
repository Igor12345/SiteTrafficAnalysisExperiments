using Infrastructure;
using Infrastructure.DataStructures;
using LogsAnalyzer.LogEntries;

namespace RuntimeStatistic.TrafficConsumer;

//todo split responsibilities: update history, update priority queue
public class MostActiveUsersSelector : IConsumer<Result<LogEntry>>
{
    private readonly TrafficHistory _history;
    private readonly IndexPriorityQueue<LogEntry> _activeUsers;

    public MostActiveUsersSelector(TrafficHistory history)
    {
        _history = history;
        int queueCapacity = 100;
        _activeUsers = new IndexPriorityQueue<LogEntry>(queueCapacity);
    }

    public void Consume(Result<LogEntry> result)
    {
        if (!result.Success)
            return;

        var logEntry = result.Value;
        Console.WriteLine($"--- Activity users selector: {logEntry} added to queue, thread: {Thread.CurrentThread.ManagedThreadId}");
        //todo a bit unsafe
        int visits = (int)_history.GetUniqueVisits(logEntry.CustomerId);
        //todo implement Update, to change priority 
        _activeUsers.Enqueue(logEntry, visits);
    }

    public ulong[] GetMostActiveUsers(int number)
    {
        return _activeUsers.Peek(number).Select(item => item.value.CustomerId).ToArray();
    }
}