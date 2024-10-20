using LogEntry = LogsAnalyzer.LogEntries.LogEntry;

namespace LogsAnalyzer.LogReader;

public interface ILinesSourceAsync : IAsyncEnumerable<LogEntry>
{
}