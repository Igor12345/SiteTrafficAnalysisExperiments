using FileCreator.Lines;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileCreator;

internal sealed class CreatingFileService : IHostedService
{
    private readonly LogCreator _logCreator;

    public CreatingFileService(FileCreatorConfiguration config, LinesGenerator generator,
        LinesWriterFactory linesWriterFactory, ILogger<CreatingFileService> logger)
    {
        _logCreator = new LogCreator(config, generator, linesWriterFactory, logger);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        int filesNumber = 2;
        for (int i = 0; i < filesNumber; i++)
        {
            _logCreator.CreateFile();
        }

        return Task.CompletedTask;
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}