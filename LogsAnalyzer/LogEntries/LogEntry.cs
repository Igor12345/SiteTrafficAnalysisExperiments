using System.Globalization;
using Infrastructure;

namespace LogsAnalyzer.LogEntries;

public readonly record struct LogEntry(ulong CustomerId, uint PageId, DateTime DateTime)
{
    public static Result<LogEntry> ParseRecord((string dateTime, ulong userId, uint pageId) item)
    {
        if (!DateTime.TryParseExact(item.dateTime, "s", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal,
                out var dateTime))
            return Result<LogEntry>.Error($"Cannot parse {item.dateTime}"); //todo

        return Result<LogEntry>.Ok(new LogEntry(item.userId, item.pageId, dateTime));
    }
}