using DickinsonBros.SQL.Abstractions;
using DickinsonBros.SQL.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DickinsonBros.SQL.Tests.Extensions
{
    [TestClass]
    public class IServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void AddSQLService_Should_Succeed()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act
            serviceCollection.AddSQLService();

            // Assert

            Assert.IsTrue(serviceCollection.Any(serviceDefinition => serviceDefinition.ServiceType == typeof(ISQLService) &&
                                           serviceDefinition.ImplementationType == typeof(SQLService) &&
                                           serviceDefinition.Lifetime == ServiceLifetime.Singleton));
        }
    }
}
