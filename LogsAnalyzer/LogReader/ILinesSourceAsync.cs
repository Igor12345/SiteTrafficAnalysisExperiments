using LogsAnalyzer.LogEntries;

namespace LogsAnalyzer.LogReader;

public interface ILinesSourceAsync : IAsyncEnumerable<LogEntry>
{
}