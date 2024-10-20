using Infrastructure;
using Infrastructure.ByteOperations;
using Infrastructure.IOOperations;
using LogsAnalyzer.LogEntries;

namespace LogsAnalyzer.LogReader;

public class LogAsBytesReader : ILinesSourceAsync
{
    private readonly LogEntriesExtractor _logEntriesExtractor;
    private readonly FileReaderFactory _fileReaderFactory;
    private readonly string[] _files;
    private readonly BuffersPool _buffersPool;

    public LogAsBytesReader(FileReaderFactory fileReaderFactory, string[] files,
        LogEntriesExtractor logEntriesExtractor)
    {
        _fileReaderFactory = Guard.NotNull(fileReaderFactory);
        _files = Guard.NotNull(files);
        _logEntriesExtractor = Guard.NotNull(logEntriesExtractor);
        _buffersPool = new BuffersPool();
    }

    public async IAsyncEnumerator<LogEntry> GetAsyncEnumerator(
        CancellationToken cancellationToken = new CancellationToken())
    {
        //The check is redundant in case of sequential reading from file into containers
        int iteration = 0;
        foreach (string file in _files)
        {
            using var fileReader = _fileReaderFactory.CreateBytesProducer(file, iteration);

            byte[] restOfLastLine = Array.Empty<byte>();
            while (true)
            {
                DataChunkContainer? container = await _buffersPool.GetNextAsync();
                restOfLastLine.CopyTo(container.RowData, 0);
                container.PrePopulatedBytesLength = restOfLastLine.Length;
                iteration = container.PackageNumber;

                //todo use different types, like PreContainer, PopulatedContainer to avoid accidental mistakes
                DataChunkContainer populatedContainer = await fileReader.WriteBytesToBufferAsync(container);
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
                if (remainingBytesLength <= 0)
                {
                    Console.WriteLine(
                        $"Iteration: {iteration++}, rest of the LAST line is {ByteToStringConverter.Convert(restOfLastLine)}");
                }

                restOfLastLine = new byte[remainingBytesLength];
                populatedContainer.RowData.AsSpan()[
                    extractionResult.StartRemainingBytes..populatedContainer.WrittenBytesLength].CopyTo(restOfLastLine);

                //todo
                Console.WriteLine(
                    $"Iteration: {iteration++}, rest of the line {ByteToStringConverter.Convert(restOfLastLine)}");

                bool last = populatedContainer.IsLastPart;

                //don't use try/finally, let the exception propagate, there is nothing we can do here.
                _buffersPool.ReleaseContainer(populatedContainer);
                if (last)
                    break;
            }
        }
    }

    private void HandleError(string errorMessage)
    {
        //todo
        Console.WriteLine(errorMessage);
    }
}