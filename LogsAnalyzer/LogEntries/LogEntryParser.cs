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

    public Result<LogEntry> Parse(ReadOnlySpan<byte> lineSpan)
    {
        var timeMarkResult = LookingForPart<DateTime>(lineSpan, DateTime.TryParse, 0, "Invalid time mark:");
        if (timeMarkResult.IsError)
            return Result<LogEntry>.Error(timeMarkResult.ErrorMessage);

        var userIdResult =
            LookingForPart<UInt64>(lineSpan, UInt64.TryParse, timeMarkResult.Value.Item2, "Invalid customer id:");
        if (userIdResult.IsError)
            return Result<LogEntry>.Error(userIdResult.ErrorMessage);

        var pageIdResult =
            LookingForPart<UInt32>(lineSpan, UInt32.TryParse, userIdResult.Value.Item2, "Invalid page id:");
        if (pageIdResult.IsError)
            return Result<LogEntry>.Error(pageIdResult.ErrorMessage);


        return Result<LogEntry>.Ok(new LogEntry(userIdResult.Value.Item1, pageIdResult.Value.Item1,
            timeMarkResult.Value.Item1));
    }

    private Result<(T, int)> LookingForPart<T>(ReadOnlySpan<byte> lineSpan, TryParse<T> tryParse, int startFrom,
        string errorMessagePrefix)
    {
        Span<char> numberChars = stackalloc char[MaxNumberLength];
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