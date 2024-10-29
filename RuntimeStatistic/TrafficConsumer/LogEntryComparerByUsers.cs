using LogsAnalyzer.LogEntries;

namespace RuntimeStatistic.TrafficConsumer;

public class LogEntryComparerByUsers : IEqualityComparer<LogEntry>
{
    public bool Equals(LogEntry x, LogEntry y)
    {
        return x.CustomerId == y.CustomerId;
    }

    public int GetHashCode(LogEntry obj)
    {
        return HashCode.Combine(obj.CustomerId);
    }
}