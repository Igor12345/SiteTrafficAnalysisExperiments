using Infrastructure;
using Infrastructure.IOOperations;
using LogsAnalyzer.Analyzers;

namespace ConsoleUI;

public class LogsReader : ILinesSourceAsync
{
    private readonly FileReaderFactory _fileReaderFactory;
    private readonly string[] _files;

    public LogsReader(FileReaderFactory fileReaderFactory, string[] files)
    {
        _fileReaderFactory = Guard.NotNull(fileReaderFactory);
        _files = Guard.NotNull(files);
    }

    public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        foreach (string file in _files)
        {
            using var fileReader = _fileReaderFactory.Create(file);

            while (fileReader.Peek() > -1)
            {
                var line = await fileReader.ReadLineAsync(cancellationToken);
                if (line == null)
                    break;
                yield return line;
            }
        }
    }
}