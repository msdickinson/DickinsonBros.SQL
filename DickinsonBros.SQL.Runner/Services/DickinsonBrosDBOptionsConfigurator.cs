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
            var certificateEncryptionService = provider.GetRequiredService<ICertificateEncryptionService<RunnerCertificateEncryptionServiceOptions>>();
            var telemetryServiceOptions = configuration.GetSection(nameof(TelemetryServiceOptions)).Get<TelemetryServiceOptions>();
            telemetryServiceOptions.ConnectionString = certificateEncryptionService.Decrypt(telemetryServiceOptions.ConnectionString);
            configuration.Bind($"{nameof(TelemetryServiceOptions)}", options);

            options.ConnectionString = telemetryServiceOptions.ConnectionString;
        }
    }
}
