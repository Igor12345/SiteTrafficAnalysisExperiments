namespace Infrastructure;

//todo wrong project
public readonly record struct LogEntry(ulong CustomerId, uint PageId, DateTime DateTime);