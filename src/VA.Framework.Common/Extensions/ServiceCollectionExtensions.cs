using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VA.Framework.Common.Azure.CosmosDb.Definition;
using VA.Framework.Common.Azure.CosmosDb.Implementation;
using VA.Framework.Common.Azure.KeyVault.Definition;
using VA.Framework.Common.Azure.KeyVault.Implementation;
using VA.Framework.Common.Azure.ServiceBus.Definition;
using VA.Framework.Common.Azure.ServiceBus.Implementation;
using VA.Framework.Common.Azure.Storage.Blobs.Definition;
using VA.Framework.Common.Azure.Storage.Blobs.Implementation;
using VA.Framework.Common.Environment.Definition;
using VA.Framework.Common.Environment.Implementation;
using VA.Framework.Common.Helpers.Definition;
using VA.Framework.Common.Helpers.Implementation;
using VA.Framework.Common.RedisCaching.Definition;
using VA.Framework.Common.RedisCaching.Implementation;

namespace VA.Framework.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEnvironmentService(this IServiceCollection services)
        {
            services.AddSingleton<IEnvironmentSettings, EnvironmentSettings>();
            return services;
        }

        public static IServiceCollection AddExcelDataReaderHelper(this IServiceCollection services)
        {
            services.AddSingleton<IExcelDataReaderHelper, ExcelDataReaderHelper>();
            return services;
        }

        public static IServiceCollection AddServiceBusQueueClient(this IServiceCollection services, string serviceBusConnectionString)
        {
            services.AddSingleton<IAzureServiceBusQueueClient>(sb => new AzureServiceBusQueueClient(serviceBusConnectionString));
            return services;
        }

        public static IServiceCollection AddBlobServiceClient(this IServiceCollection services, string storageAccountConnectionString)
        {
            services.AddSingleton<IAzureBlobServiceClient>(sb => new AzureBlobServiceClient(storageAccountConnectionString));
            return services;
        }

        public static IServiceCollection AddCosmosDbClient(this IServiceCollection services, string cosmosDbConnectionString)
        {
            services.AddSingleton<IAzureCosmosDbClient>(sb => new AzureCosmosDbClient(cosmosDbConnectionString));
            return services;
        }

        public static IServiceCollection AddKeyVaultSecretsClient(this IServiceCollection services, string keyVaultBaseUrl, string clientId, string clientSecret)
        {
            services.AddSingleton<IAzureKeyVaultSecretsClient>(sb => new AzureKeyVaultSecretsClient(keyVaultBaseUrl, clientId, clientSecret));
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
