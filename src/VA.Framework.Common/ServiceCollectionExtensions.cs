using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VA.Framework.Common.Azure.CosmosDb.Definition;
using VA.Framework.Common.Azure.CosmosDb.Implementation;
using VA.Framework.Common.Azure.KeyVault.Definition;
using VA.Framework.Common.Azure.KeyVault.Implementation;
using VA.Framework.Common.Azure.ServiceBus.Definition;
using VA.Framework.Common.Azure.ServiceBus.Implementation;
using VA.Framework.Common.Azure.Storage.Blobs.Definition;
using VA.Framework.Common.Azure.Storage.Blobs.Implementation;
using VA.Framework.Common.Database.Definition;
using VA.Framework.Common.Database.Implementation;
using VA.Framework.Common.Environment.Definition;
using VA.Framework.Common.Environment.Implementation;
using VA.Framework.Common.Helpers.Definition;
using VA.Framework.Common.Helpers.Implementation;
using VA.Framework.Common.RedisCaching.Definition;
using VA.Framework.Common.RedisCaching.Implementation;

namespace VA.Framework.Common
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEnvironmentService(this IServiceCollection services) =>
            services.AddSingleton<IEnvironmentSettings, EnvironmentSettings>();

        public static IServiceCollection AddServiceBusQueueClient(this IServiceCollection services, string serviceBusConnectionString) =>
            services.AddSingleton<IAzureServiceBusQueueClient>(sb => new AzureServiceBusQueueClient(serviceBusConnectionString));

        public static IServiceCollection AddBlobServiceClient(this IServiceCollection services, string storageAccountConnectionString) =>
            services.AddSingleton<IAzureBlobServiceClient>(sb => new AzureBlobServiceClient(storageAccountConnectionString));

        public static IServiceCollection AddCosmosDbClient(this IServiceCollection services, string cosmosDbConnectionString) =>
            services.AddSingleton<IAzureCosmosDbClient>(sb => new AzureCosmosDbClient(cosmosDbConnectionString));

        public static IServiceCollection AddKeyVaultSecretsClient(this IServiceCollection services, string keyVaultBaseUrl, string clientId, string clientSecret) =>
            services.AddSingleton<IAzureKeyVaultSecretsClient>(sb => new AzureKeyVaultSecretsClient(keyVaultBaseUrl, clientId, clientSecret));

        public static IServiceCollection AddDatabaseConnection(this IServiceCollection services) =>
            services.AddSingleton<IDatabaseConnection, DatabaseConnection>();

        public static IServiceCollection AddExcelDataReaderHelper(this IServiceCollection services) =>
            services.AddSingleton<IExcelDataReaderHelper, ExcelDataReaderHelper>();

        public static IServiceCollection AddSerilogConsole(this IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddSerilog(new LoggerConfiguration()
                              .WriteTo.Console()
                              .MinimumLevel.Information()
                              .CreateLogger(), dispose: true);
            });
            return services;
        }

        public static IServiceCollection AddSerilogFile(this IServiceCollection services, string path)
        {
            services.AddLogging(builder =>
            {
                builder.AddSerilog(new LoggerConfiguration()
                              .WriteTo.Console()
                              .WriteTo.File(path)
                              .MinimumLevel.Information()
                              .CreateLogger(), dispose: true);
            });
            return services;
        }

        public static IServiceCollection AddRedisCacheService(this IServiceCollection services, string keyPrefix = "")
        {
            services.LoadRedisCache(keyPrefix);
            return services;
        }

        private static void LoadRedisCache(this IServiceCollection services, string keyPrefix)
        {
            services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            var sp = services.BuildServiceProvider();
            var redisCacheConnectionPoolManager = sp.GetRequiredService<IRedisCacheConnectionPoolManager>();
            var logger = sp.GetRequiredService<ILogger<CacheObjectManager>>();
            services.AddScoped<ICacheObjectManager>(provider => new CacheObjectManager(redisCacheConnectionPoolManager, logger, keyPrefix));
        }

    }
}
