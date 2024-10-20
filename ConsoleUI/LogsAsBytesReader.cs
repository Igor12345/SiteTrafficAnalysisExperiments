using Infrastructure;
using Infrastructure.IOOperations;
using LogsAnalyzer.Analyzers;
using LogsAnalyzer.DataStructures;
using LogsAnalyzer.LogEntries;

namespace ConsoleUI;

public class LogsAsBytesReader : ILinesSourceAsync
{
    private readonly LogEntriesExtractor _logEntriesExtractor;
    private readonly FileReaderFactory _fileReaderFactory;
    private readonly string[] _files;

    public LogsAsBytesReader(FileReaderFactory fileReaderFactory, string[] files, LogEntriesExtractor logEntriesExtractor)
    {
        _fileReaderFactory = Guard.NotNull(fileReaderFactory);
        _files = Guard.NotNull(files);
        _logEntriesExtractor = Guard.NotNull(logEntriesExtractor);
    }

    public async IAsyncEnumerator<LogEntry> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (string file in _files)
        {
            using var fileReader = _fileReaderFactory.CreateBytesProducer(file);

            byte[] bytes = await fileReader.WriteBytesToBufferAsync();

            ExpandableStorage<LogEntry> storage = new ExpandableStorage<LogEntry>(chunkSize: 4096);
            var extractionResult = _logEntriesExtractor.ExtractRecords(bytes, storage);

            if (extractionResult.Success)
            {
                for (int i = 0; i < extractionResult.LinesNumber; i++)
                {
                    yield return storage[i];
                }
            }
            else
            {
                HandleError(extractionResult.Message);
            }
        }
    }

    private void HandleError(string errorMessage)
    {
        //todo
        Console.WriteLine(errorMessage);
    }
}