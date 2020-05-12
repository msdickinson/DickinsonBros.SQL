using DickinsonBros.Encryption.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace DickinsonBros.SQL.Runner.Services.AccountDB
{
    public class DickinsonBrosSQLRunnerDBOptionsConfigurator : IConfigureOptions<DickinsonBrosSQLRunnerDB>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public DickinsonBrosSQLRunnerDBOptionsConfigurator(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        void IConfigureOptions<DickinsonBrosSQLRunnerDB>.Configure(DickinsonBrosSQLRunnerDB options)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var configuration = provider.GetRequiredService<IConfiguration>();
                var encryptionService = provider.GetRequiredService<IEncryptionService>();

                var dickinsonBrosSQLRunnerDBSettings = configuration.GetSection(nameof(DickinsonBrosSQLRunnerDB)).Get<DickinsonBrosSQLRunnerDB>();
                dickinsonBrosSQLRunnerDBSettings.ConnectionString = encryptionService.Decrypt(dickinsonBrosSQLRunnerDBSettings.ConnectionString);
                configuration.Bind($"{nameof(DickinsonBrosSQLRunnerDB)}", options);

                options.ConnectionString = encryptionService.Decrypt(options.ConnectionString);
            }
        }
    }
}
