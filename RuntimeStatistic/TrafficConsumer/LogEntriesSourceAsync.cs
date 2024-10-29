using System.Collections.Concurrent;
using System.Globalization;
using LogsAnalyzer.LogEntries;
using LogsAnalyzer.LogReader;

namespace RuntimeStatistic.TrafficConsumer;

public class LogEntriesSourceAsync: ILinesSourceAsync, IConsumer<(string dateTime, UserId userId, PageId pageId)>
{
    private readonly BlockingCollection<LogEntry> _logEntries = new();
    public async IAsyncEnumerator<LogEntry> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        foreach (LogEntry logEntry in _logEntries.GetConsumingEnumerable(cancellationToken))
        {
            yield return logEntry;
        }
    }

    public void Consume((string dateTime, UserId userId, PageId pageId) item)
    {
        if (!DateTime.TryParseExact(item.dateTime, "s", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal,
                out var dateTime))
            return;

        LogEntry logEntry = new LogEntry(item.userId, item.pageId, dateTime);
        _logEntries.Add(logEntry);
    }
}