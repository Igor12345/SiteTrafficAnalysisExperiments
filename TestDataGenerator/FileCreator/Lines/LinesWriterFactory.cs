using Infrastructure;

namespace FileCreator.Lines;

internal sealed class LinesWriterFactory
{
    private readonly FileCreatorConfiguration _config;
    private int _lastFileNumber;

    public LinesWriterFactory(FileCreatorConfiguration config)
    {
        _config = Guard.NotNull(config);
    }

    public LinesWriter Create()
    {
        string fileName = _config.FileName.Replace("##", $"{++_lastFileNumber}");
        LastFilePath = Path.Combine(_config.OutputDirectory, fileName);
        return new LinesWriter(LastFilePath);
    }

    public string? LastFilePath { get; private set; }
}