using DickinsonBros.SQL.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DickinsonBros.SQL.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddSQLService(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<ISQLService, SQLService>();

            return serviceCollection;
        }
    }
}
