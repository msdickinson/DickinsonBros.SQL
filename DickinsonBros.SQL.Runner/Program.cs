using DickinsonBros.Encryption;
using DickinsonBros.Encryption.Abstractions;
using DickinsonBros.Encryption.Models;
using DickinsonBros.Logger;
using DickinsonBros.Logger.Abstractions;
using DickinsonBros.Redactor;
using DickinsonBros.Redactor.Abstractions;
using DickinsonBros.Redactor.Models;
using DickinsonBros.SQL.Abstractions;
using DickinsonBros.SQL.Runner.Services.AccountDB;
using DickinsonBros.SQL.Runner.Services.AccountDB.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DickinsonBros.SQL.Runner
{
    class Program
    {
        IConfiguration _configuration;
        async static Task Main()
        {
            await new Program().DoMain();
        }
        async Task DoMain()
        {
            try
            {
                using (var applicationLifetime = new Services.ApplicationLifetime())
                {
                    var services = InitializeDependencyInjection();
                    ConfigureServices(services, applicationLifetime);
                    using (var provider = services.BuildServiceProvider())
                    {
                        var dickinsonBrosSQLRunnerDBService = provider.GetRequiredService<IDickinsonBrosSQLRunnerDBService>();

                        var queueItem = new QueueDTO
                        {
                            Payload = @"{""X"": ""1""}"
                        };
                        var queueItems = new List<QueueDTO>
                        {
                            queueItem,
                            queueItem,
                            queueItem,
                            queueItem,
                            queueItem
                        };

                        //ExecuteAsync (Delete, Insert Item, Insert Items)
                        await dickinsonBrosSQLRunnerDBService.DeleteAllQueueItemsAsync().ConfigureAwait(false);
                        await dickinsonBrosSQLRunnerDBService.InsertQueueItemAsync(queueItem).ConfigureAwait(false);
                        await dickinsonBrosSQLRunnerDBService.InsertQueueItemsAsync(queueItems).ConfigureAwait(false);

                        //BulkCopyAsync
                        await dickinsonBrosSQLRunnerDBService.BulkInsertQueueItemsAsync(queueItems).ConfigureAwait(false);

                        //QueryFirstAsync
                        var queueItemObserved = await dickinsonBrosSQLRunnerDBService.QueryQueueFirstAsync().ConfigureAwait(false);

                        //QueryFirstOrDefaultAsync
                        var queueItemOrDefaultObserved = await dickinsonBrosSQLRunnerDBService.QueryQueueFirstOrDefaultAsync().ConfigureAwait(false);

                        //QueryAsync
                        var queueItemsObserved = await dickinsonBrosSQLRunnerDBService.SelectLast50QueueItemsProc().ConfigureAwait(false);

                        //ExecuteAsync (Update)
                        queueItemObserved.Payload = @"{""X"": ""2""}";
                        await dickinsonBrosSQLRunnerDBService.UpdateQueueItemAsync(queueItemObserved).ConfigureAwait(false);

                        applicationLifetime.StopApplication();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Console.WriteLine("End...");
                Console.ReadKey();
            }

        }

        private void ConfigureServices(IServiceCollection services, Services.ApplicationLifetime applicationLifetime)
        {
            services.AddOptions();
            services.AddScoped<ICorrelationService, CorrelationService>();
            services.AddLogging(cfg => cfg.AddConsole());
            services.AddSingleton<IApplicationLifetime>(applicationLifetime);
            services.AddSingleton<IRedactorService, RedactorService>();
            services.AddSingleton<ISQLService, SQLService>();
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddSingleton<IDickinsonBrosSQLRunnerDBService, DickinsonBrosSQLRunnerDBService>();

            services.AddScoped(typeof(ILoggingService<>), typeof(LoggingService<>));
            services.AddSingleton<IRedactorService, RedactorService>();

            services.Configure<JsonRedactorOptions>(_configuration.GetSection(nameof(JsonRedactorOptions)));
            services.Configure<EncryptionSettings>(_configuration.GetSection(nameof(EncryptionSettings)));
            services.AddSingleton<IConfigureOptions<DickinsonBrosSQLRunnerDB>, DickinsonBrosSQLRunnerDBOptionsConfigurator>();
        }



        IServiceCollection InitializeDependencyInjection()
        {
            var aspnetCoreEnvironment = Environment.GetEnvironmentVariable("BUILD_CONFIGURATION");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{aspnetCoreEnvironment}.json", true);
            _configuration = builder.Build();
            var services = new ServiceCollection();
            services.AddSingleton(_configuration);
            return services;
        }
    }
}
