namespace LogsAnalyzer.LogEntries;

public readonly record struct LogEntry(ulong CustomerId, uint PageId, DateTime DateTime);