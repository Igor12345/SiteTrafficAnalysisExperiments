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

    public Result<LogEntry> Consume(Result<LogEntry> result)
    {
        result.OnSuccess(logEntry =>
        {
            //todo a bit unsafe
            int visits = (int)_history.GetUniqueVisits(logEntry.CustomerId);
            _activeUsers.Enqueue(logEntry.CustomerId, visits);
        });
        return result;
    }

    public UserId[] GetMostActiveUsers(int number)
    {
        return _activeUsers.Peek(number).Select(item => item.value).ToArray();
    }
}