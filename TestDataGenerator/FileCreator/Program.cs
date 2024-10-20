using System.Diagnostics;
using FileCreator.Lines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace FileCreator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args).Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            FileCreatorConfiguration? fileCreatorConfiguration = configuration.GetRequiredSection("Config").Get<FileCreatorConfiguration>();
            Debug.Assert(fileCreatorConfiguration != null, nameof(fileCreatorConfiguration) + " != null");

            //todo
            if (string.IsNullOrEmpty(fileCreatorConfiguration.OutputDirectory))
            {
                fileCreatorConfiguration.OutputDirectory = Directory.GetCurrentDirectory();
            }
            else
            {
                if (!Path.IsPathFullyQualified(fileCreatorConfiguration.OutputDirectory))
                    fileCreatorConfiguration.OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(),
                        fileCreatorConfiguration.OutputDirectory);
                if (!Directory.Exists(fileCreatorConfiguration.OutputDirectory))
                    Directory.CreateDirectory(fileCreatorConfiguration.OutputDirectory);
            }

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(fileCreatorConfiguration);
                    services.AddSingleton<ILineCreator, LineCreator>();
                    services.AddSingleton<LinesGenerator>();
                    services.AddScoped<LinesWriterFactory>();
                    services.AddHostedService<CreatingFileService>();
                }).UseSerilog().Build();

            await host.RunAsync();
        }
    }
}
