using DickinsonBros.DataTable.Extensions;
using DickinsonBros.DateTime.Extensions;
using DickinsonBros.Encryption.Certificate.Extensions;
using DickinsonBros.Encryption.Certificate.Models;
using DickinsonBros.Logger.Extensions;
using DickinsonBros.Redactor.Extensions;
using DickinsonBros.Redactor.Models;
using DickinsonBros.SQL.Extensions;
using DickinsonBros.SQL.Runner.Models;
using DickinsonBros.SQL.Runner.Services;
using DickinsonBros.SQL.Runner.Services.AccountDB;
using DickinsonBros.SQL.Runner.Services.AccountDB.Models;
using DickinsonBros.Stopwatch.Extensions;
using DickinsonBros.Telemetry.Abstractions;
using DickinsonBros.Telemetry.Extensions;
using DickinsonBros.Telemetry.Models;
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
                using var applicationLifetime = new Services.ApplicationLifetime();
                var services = InitializeDependencyInjection();
                ConfigureServices(services, applicationLifetime);
                using var provider = services.BuildServiceProvider();
                var telemetryService = provider.GetRequiredService<ITelemetryService>();
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
                await dickinsonBrosSQLRunnerDBService.DeleteAllMixedItemsAsync().ConfigureAwait(false);
                await dickinsonBrosSQLRunnerDBService.DeleteAllQueueItemsAsync().ConfigureAwait(false);
                await dickinsonBrosSQLRunnerDBService.InsertQueueItemAsync(queueItem).ConfigureAwait(false);
                await dickinsonBrosSQLRunnerDBService.InsertQueueItemsAsync(queueItems).ConfigureAwait(false);

                //BulkCopyAsync
                await dickinsonBrosSQLRunnerDBService.BulkInsertQueueItemsAsync(queueItems).ConfigureAwait(false);
                await dickinsonBrosSQLRunnerDBService.BulkInsertQueueItemsViaIEnumerableAsync(queueItems).ConfigureAwait(false);

                //Note: This is looking for most common cases, and common edge cases
                var mixedItems = new List<MixedDTO> 
                { 
                    GeneratingMixedDTO(),
                    GeneratingMixedDTO(),
                    GeneratingMixedDTO()
                };
                
                await dickinsonBrosSQLRunnerDBService.BulkInsertMixedItemsViaIEnumerableAsync(mixedItems).ConfigureAwait(false);

                //QueryFirstAsync
                var queueItemObserved = await dickinsonBrosSQLRunnerDBService.QueryQueueFirstAsync().ConfigureAwait(false);

                //QueryFirstOrDefaultAsync
                var queueItemOrDefaultObserved = await dickinsonBrosSQLRunnerDBService.QueryQueueFirstOrDefaultAsync().ConfigureAwait(false);

                //QueryAsync
                var queueItemsObserved = await dickinsonBrosSQLRunnerDBService.SelectLast50QueueItemsProc().ConfigureAwait(false);

                //ExecuteAsync (Update)
                queueItemObserved.Payload = @"{""X"": ""2""}";
                await dickinsonBrosSQLRunnerDBService.UpdateQueueItemAsync(queueItemObserved).ConfigureAwait(false);

                Console.WriteLine("Flush Telemetry");
                await telemetryService.FlushAsync().ConfigureAwait(false);

                applicationLifetime.StopApplication();
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

        private MixedDTO GeneratingMixedDTO()
        {
            return new MixedDTO
            {
                ByteArray = new byte[3] { 1, 2, 3 },
                Bool = false,
                Byte = 4,
                Char = 'a',
                DateTime = new System.DateTime(2020, 1, 1),
                Double = 5.1,
                Float = 6.1F,
                Guid = Guid.NewGuid(),
                Int = 7,
                NullString = null,
                NullValueType = null,
                SampleEnum = SampleEnum.Blue,
                TimeSpan = new TimeSpan(1, 0, 0),
                String = "SampleString"
            };
        }


        private void ConfigureServices(IServiceCollection services, Services.ApplicationLifetime applicationLifetime)
        {
            services.AddOptions();
            services.AddLogging(cfg => cfg.AddConsole());

            //Add ApplicationLifetime
            services.AddSingleton<IApplicationLifetime>(applicationLifetime);

            //Add DateTime Service
            services.AddDateTimeService();

            //Add Stopwatch Service
            services.AddStopwatchService();

            //Add Logging Service
            services.AddLoggingService();

            //Add Redactor Service
            services.AddRedactorService();
            services.Configure<RedactorServiceOptions>(_configuration.GetSection(nameof(RedactorServiceOptions)));

            //Add Certificate Encryption Service
            services.AddCertificateEncryptionService<CertificateEncryptionServiceOptions>();
            services.Configure<CertificateEncryptionServiceOptions<RunnerCertificateEncryptionServiceOptions>>(_configuration.GetSection(nameof(RunnerCertificateEncryptionServiceOptions)));

            //Add Telemetry Service
            services.AddTelemetryService();
            services.AddSingleton<IConfigureOptions<TelemetryServiceOptions>, TelemetryServiceOptionsConfigurator>();

            //Add SQLService
            services.AddSQLService();

            //Add SQLService
            services.AddDataTableService();

            //Add MemoryCatche
            services.AddMemoryCache();

            //Add Runner SQL Database Service
            services.AddSingleton<IDickinsonBrosSQLRunnerDBService, DickinsonBrosSQLRunnerDBService>();
            services.AddSingleton<IConfigureOptions<DickinsonBrosDBOptions>, DickinsonBrosDBOptionsConfigurator>();
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
