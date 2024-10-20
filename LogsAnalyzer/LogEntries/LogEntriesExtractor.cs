using Infrastructure;
using LogsAnalyzer.DataStructures;
using System.Text;

namespace LogsAnalyzer.LogEntries;

public sealed class LogEntriesExtractor
{
    private readonly ILogEntryParser _parser;
    private readonly byte[] _eol;

    /// <summary>
    /// This class works only with bytes and knows nothing about encoding.
    /// It needs to know the byte sequences for the end of a line and for the delimiter between a number and a text to extract and then parse lines.
    /// </summary>
    public LogEntriesExtractor(ILogEntryParser parser)
    {
        _parser = Guard.NotNull(parser);
        _eol = Encoding.UTF8.GetBytes(Environment.NewLine); ;
    }

    //this is hardcoded for UTF-8
    public ExtractionResult ExtractRecords(ReadOnlySpan<byte> input, ExpandableStorage<LogEntry> records, int offset = 0)
    {
        int lineIndex = 0;
        int endLine = 0;
        int endOfLastLine = -1;
        for (int i = 0; i < input.Length - 1; i++)
        {
            if (input[i] == _eol[0] && input[i + 1] == _eol[1])
            {
                endOfLastLine = i + 1;
                var startLine = endLine;

                //todo text will include eof. the question with the last line.
                endLine = i + 2;
                var result = _parser.Parse(input[startLine..endLine]);

                if (!result.Success)
                    return ExtractionResult.Error(result.ErrorMessage);

                records.Add(result.Value);
                lineIndex++;
                i++;
            }
        }

        return ExtractionResult.Ok(lineIndex, endOfLastLine + 1);
    }
}