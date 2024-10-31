using Infrastructure;
using LogsAnalyzer.LogEntries;

namespace RuntimeStatistic.TrafficConsumer.Extensions;

public static class TrafficHistoryExtensions
{
    public static Result<LogEntry> Save(this TrafficHistory trafficHistory, Result<LogEntry> logEntryResult)
    {
        logEntryResult.OnSuccess(trafficHistory.AddOrUpdate);
        return logEntryResult;
    }
}