using Infrastructure;
using LogsAnalyzer.LogEntries;

namespace RuntimeStatistic.TrafficConsumer.Extensions;

public static class TrafficHistoryExtensions
{
    public static Result<LogEntry> Save(this TrafficHistory trafficHistory, Result<LogEntry> logEntry)
    {
        if (!logEntry.Success)
            return logEntry;
        trafficHistory.AddOrUpdate(logEntry.Value);
        return logEntry;
    }
}