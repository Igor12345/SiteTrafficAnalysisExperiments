using LogsAnalyzer.LogEntries;

namespace LogsAnalyzer.Analyzers;

public interface ILinesSourceAsync : IAsyncEnumerable<LogEntry>
{
}