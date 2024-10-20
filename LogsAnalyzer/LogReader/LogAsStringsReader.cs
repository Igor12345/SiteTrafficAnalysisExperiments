using Infrastructure;
using Infrastructure.IOOperations;
using LogsAnalyzer.LogEntries;

namespace LogsAnalyzer.LogReader;

public class LogAsStringsReader : ILinesSourceAsync
{
    private readonly ILogEntryParser _parser;
    private readonly FileReaderFactory _fileReaderFactory;
    private readonly string[] _files;

    public LogAsStringsReader(FileReaderFactory fileReaderFactory, string[] files, ILogEntryParser parser)
    {
        _fileReaderFactory = Guard.NotNull(fileReaderFactory);
        _files = Guard.NotNull(files);
        _parser = Guard.NotNull(parser);
    }

    public async IAsyncEnumerator<LogEntry> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        foreach (string file in _files)
        {
            using var fileReader = _fileReaderFactory.Create(file);

            while (fileReader.Peek() > -1)
            {
                var line = await fileReader.ReadLineAsync(cancellationToken);
                if (line == null)
                    break;

                var parsingResult = _parser.Parse(line);
                if (!parsingResult.Success)
                {
                    HandleError(parsingResult.ErrorMessage);
                }
                else
                {
                    yield return parsingResult.Value;
                }
            }
        }
    }

    private void HandleError(string errorMessage)
    {
        Console.WriteLine($"Error: {errorMessage}");
    }
}