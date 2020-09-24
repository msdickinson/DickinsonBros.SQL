using DickinsonBros.Encryption.Certificate.Abstractions;
using DickinsonBros.SQL.Runner.Models;
using DickinsonBros.Telemetry.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DickinsonBros.SQL.Runner.Services
{
    public class DickinsonBrosDBOptionsConfigurator : IConfigureOptions<DickinsonBrosDBOptions>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public DickinsonBrosDBOptionsConfigurator(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        void IConfigureOptions<DickinsonBrosDBOptions>.Configure(DickinsonBrosDBOptions options)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var provider = scope.ServiceProvider;
            var configuration = provider.GetRequiredService<IConfiguration>();
            var configurationEncryptionService = provider.GetRequiredService<IConfigurationEncryptionService>();
            var telemetryServiceOptions = configuration.GetSection(nameof(TelemetryServiceOptions)).Get<TelemetryServiceOptions>(); 
            configuration.Bind($"{nameof(TelemetryServiceOptions)}", options);

            options.ConnectionString = configurationEncryptionService.Decrypt(telemetryServiceOptions.ConnectionString);
        }
    }
}
