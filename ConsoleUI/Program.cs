﻿using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ConsoleUI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

            hostBuilder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddEnvironmentVariables();
                configBuilder.AddCommandLine(args);
            });

            hostBuilder.ConfigureLogging((_, configLogging) =>
            {
                configLogging.AddConsole();
                configLogging.AddDebug();
            });


            hostBuilder.ConfigureServices((context, services) =>
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger();

                LogReaderConfiguration? configuration =
                    context.Configuration.GetRequiredSection("Config").Get<LogReaderConfiguration>();

                Debug.Assert(configuration != null, nameof(configuration) + " != null");

                //todo
                if (string.IsNullOrEmpty(configuration.LogsFolder))
                {
                    configuration.LogsFolder = Directory.GetCurrentDirectory();
                }
                else
                {
                    if (!Path.IsPathFullyQualified(configuration.LogsFolder))
                        configuration.LogsFolder = Path.Combine(Directory.GetCurrentDirectory(),
                            configuration.LogsFolder);
                }

                Debug.Assert(configuration != null, nameof(configuration) + " != null");
                services.AddSingleton(configuration);
                services.AddSingleton<IResultsSaver, ResultsSaver>();
                services.AddHostedService<LogsAnalyzerService>();
            });

            IHost host = hostBuilder.UseSerilog().Build();
            await host.RunAsync();
        }
    }
}
