using Infrastructure;
using Infrastructure.IOOperations;
using LogsAnalyzer.LogEntries;
using Microsoft.Extensions.Logging;
using LogEntry = LogsAnalyzer.LogEntries.LogEntry;

namespace LogsAnalyzer.LogReader;

public class LogAsBytesReader : ILinesSourceAsync
{
    private readonly ILogger _logger;
    private readonly LogEntriesExtractor _logEntriesExtractor;
    private readonly FileReaderFactory _fileReaderFactory;
    private readonly string[] _files;
    private readonly BuffersPool<LogEntry> _buffersPool;

    public LogAsBytesReader(FileReaderFactory fileReaderFactory, string[] files,
        LogEntriesExtractor logEntriesExtractor, ILogger logger)
    {
        _fileReaderFactory = Guard.NotNull(fileReaderFactory);
        _files = Guard.NotNull(files);
        _logEntriesExtractor = Guard.NotNull(logEntriesExtractor);
        _buffersPool = new BuffersPool<LogEntry>();
        _logger = Guard.NotNull(logger);
    }

    public async IAsyncEnumerator<LogEntry> GetAsyncEnumerator(
        CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (string file in _files)
        {
            _logger.LogInformation($"Reading logs from the file {file}");
            await using var fileReader = _fileReaderFactory.CreateBytesProducer<LogEntry>(file, cancellationToken);

            byte[] restOfLastLine = [];
            while (true)
            {
                DataChunkContainer<LogEntry>? container = await _buffersPool.GetNextAsync();
                restOfLastLine.CopyTo(container.RowData, 0);
                container.PrePopulatedBytesLength = restOfLastLine.Length;

                //todo use different types, like PreContainer, PopulatedContainer to avoid accidental mistakes
                DataChunkContainer<LogEntry> populatedContainer = await fileReader.WriteBytesToBufferAsync(container);
                container = null;

                var extractionResult =
                    _logEntriesExtractor.ExtractRecords(
                        populatedContainer.RowData.AsSpan()[..populatedContainer.WrittenBytesLength],
                        populatedContainer.ParsedRecords);

                if (extractionResult.Success)
                {
                    for (int i = 0; i < extractionResult.LinesNumber; i++)
                    {
                        yield return populatedContainer.ParsedRecords[i];
                    }
                }
                else
                {
                    HandleError(extractionResult.Message);
                }

                //we need to preserve the rest of the last line, and parse it on the next iteration
                int remainingBytesLength = populatedContainer.WrittenBytesLength - extractionResult.StartRemainingBytes;
                
                restOfLastLine = new byte[remainingBytesLength];
                populatedContainer.RowData.AsSpan()[
                    extractionResult.StartRemainingBytes..populatedContainer.WrittenBytesLength].CopyTo(restOfLastLine);
                
                bool last = populatedContainer.IsLastPart;

                _logger.LogInformation($"{extractionResult.LinesNumber} entries have been read");

                //don't use try/finally, let the exception propagate, there is nothing we can do here.
                _buffersPool.ReleaseContainer(populatedContainer);
                if (last)
                    break;
            }
        }
    }

    private void HandleError(string errorMessage)
    {
        _logger.LogError(errorMessage);
    }
}