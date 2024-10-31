using Infrastructure;
using Infrastructure.ByteOperations;
using System.Text;

namespace LogsAnalyzer.LogEntries;

public sealed class LogEntryParser : ILogEntryParser
{
    private readonly string _delimiter;
    private readonly byte _delimiterByte;
    private const int MaxNumberLength = 100;

    private delegate bool TryParse<T>(ReadOnlySpan<char> chars, out T result);

    public LogEntryParser(string delimiter)
    {
        _delimiter = Guard.NotNullOrEmpty(delimiter);
        if (delimiter.Length != 1)
            throw new ArgumentException("The delimiter can be only one character in the current implementation");

        _delimiterByte = Encoding.UTF8.GetBytes(delimiter)[0];
    }

    public Result<LogEntry> ParseShort(string line)
    {
        var parts = line.Split(_delimiter);
        if (parts.Length != 3)
            return Result<LogEntry>.Error($"Invalid line: {line}");

        if (!ulong.TryParse(parts[1], out var customerId))
            return Result<LogEntry>.Error($"Invalid customer id: {parts[1]}");

        if (!uint.TryParse(parts[2], out var pageId))
            return Result<LogEntry>.Error($"Invalid page id: {parts[2]}");

        return Result<LogEntry>.Ok( new LogEntry(customerId, pageId, default));
    }

    public Result<LogEntry> Parse(string line)
    {
        var parts = line.Split(_delimiter);
        if (parts.Length != 3)
            return Result<LogEntry>.Error($"Invalid line: {line}");

        if (!DateTime.TryParse(parts[0], out var dateTime))
            return Result<LogEntry>.Error($"Invalid time mark: {parts[0]}");

        if (!ulong.TryParse(parts[1], out var customerId))
            return Result<LogEntry>.Error($"Invalid customer id: {parts[1]}");

        if (!uint.TryParse(parts[2], out var pageId))
            return Result<LogEntry>.Error($"Invalid page id: {parts[2]}");

        return Result<LogEntry>.Ok(new LogEntry(customerId, pageId, dateTime));
    }

    public Result<LogEntry> Parse(ReadOnlyMemory<byte> line)
    {
        LogEntry record = new LogEntry(default, default, default);
        //with side effects!!!
        var result = Result<(int, int)>.Ok((0, 0))
            .Bind(st => LookingForPart<DateTime, int>(line, DateTime.TryParse, st, "Invalid time mark:")
                .Tap(tm => record = record with { DateTime = tm.Item1 }))
            .Bind(tm => LookingForPart<UInt64, DateTime>(line, UInt64.TryParse, tm, "Invalid customer id:")
                .Tap(t => record = record with { CustomerId = t.Item1 }))
            .Bind(us => LookingForPart<UInt32, UInt64>(line, UInt32.TryParse, us, "Invalid page id:")
                .Tap(t => record = record with { PageId = t.Item1 }))
            .Bind(r => Result<LogEntry>.Ok(record));

        return result;
    }

    private Result<(T, int)> LookingForPart<T, TU>(ReadOnlyMemory<byte> line, TryParse<T> tryParse, (TU, int) prevResult,
        string errorMessagePrefix)
    {
        int startFrom = prevResult.Item2;
        Span<char> numberChars = stackalloc char[MaxNumberLength];
        ReadOnlySpan<byte> lineSpan = line.Span;
        for (int i = startFrom; i < lineSpan.Length - 1; i++)
        {
            if (lineSpan[i] == _delimiterByte || i == lineSpan.Length - 2)
            {
                if (i >= numberChars.Length)
                    break;
                for (int j = startFrom; j < i; j++)
                {
                    //todo only utf-8 encoding
                    numberChars[j] = (char)lineSpan[j];
                }

                bool success = tryParse(numberChars[startFrom..i], out var result);
                if (success)
                {
                    return Result<(T, int)>.Ok((result, i + 1));
                }

                return Result<(T, int)>.Error(
                    $"{errorMessagePrefix} {WrongPartToString(numberChars[startFrom..i])}, in the line {ByteToStringConverter.Convert(lineSpan)}");
            }
        }

        return Result<(T, int)>.Error($"Invalid line: {ByteToStringConverter.Convert(lineSpan)}");
    }

    private string WrongPartToString(Span<char> chars)
    {
        return new string(chars);
    }
}