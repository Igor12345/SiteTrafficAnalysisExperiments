using System.Buffers;
using FileCreator.Lines;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace FileCreator;

internal sealed class LogCreator
{
    private const int MaxLineLength = 100;

    private readonly ILogger _logger;
    private readonly LinesGenerator _generator;
    private readonly FileCreatorConfiguration _config;
    private readonly LinesWriterFactory _linesWriterFactory;

    public LogCreator(FileCreatorConfiguration config, LinesGenerator generator,
        LinesWriterFactory linesWriterFactory, ILogger<CreatingFileService>? logger = null)
    {
        _linesWriterFactory = Guard.NotNull(linesWriterFactory);
        _generator = Guard.NotNull(generator);
        _config = Guard.NotNull(config);
        _logger = logger ?? new EmptyLogger();
    }

    public void CreateFile()
    {
        ulong linesCount = 0;
        int linesToLog = 0;
        byte[]? buffer = null;
        try
        {
            buffer = ArrayPool<byte>.Shared.Rent(MaxLineLength);
            _generator.UpdateDate();
            using LinesWriter linesWriter = _linesWriterFactory.Create();
            foreach (int lineLength in _generator.Generate(buffer.AsMemory()))
            {
                linesWriter.Write(buffer.AsSpan()[..lineLength]);
                linesCount++;

                if (++linesToLog >= _config.LogEveryThsLine)
                {
                    linesToLog = 0;
                    _logger.LogInformation("{lines} lines.", linesCount);
                }

                if (linesCount >= _config.LinesNumber)
                    break;
            }

            linesWriter.Flush();
        }
        finally
        {
            if (buffer != null) ArrayPool<byte>.Shared.Return(buffer);
        }
        
        _logger.LogInformation("All lines created: {lines}.", linesCount);
        _logger.LogInformation($"The file: {_linesWriterFactory.LastFilePath}");
    }
}