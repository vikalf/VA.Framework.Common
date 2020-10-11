using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VA.Framework.Common.Azure.CosmosDb.Definition;

namespace VA.Framework.Common.Azure.CosmosDb.Implementation
{
    public class AzureCosmosDbClient : IAzureCosmosDbClient
    {
        private readonly CosmosClient _cosmosClient;

        public AzureCosmosDbClient(string cosmosDbConnectionString)
        {
            _cosmosClient = new CosmosClient(cosmosDbConnectionString);
        }

        public async Task AddItemAsync<T>(string databaseName, string containerName, T item, string partitionKeyValue)
        {
            var _container = _cosmosClient.GetContainer(databaseName, containerName);
            await _container.CreateItemAsync<T>(item, new PartitionKey(partitionKeyValue));
        }

        public async Task DeleteItemAsync<T>(string databaseName, string containerName, string id)
        {
            var _container = _cosmosClient.GetContainer(databaseName, containerName);
            await _container.DeleteItemAsync<T>(id, new PartitionKey(id));
        }

        public async Task<T> GetItemAsync<T>(string databaseName, string containerName, string id)
        {
            try
            {
                var _container = _cosmosClient.GetContainer(databaseName, containerName);
                ItemResponse<T> response = await _container.ReadItemAsync<T>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>(string databaseName, string containerName, string queryString)
        {
            var _container = _cosmosClient.GetContainer(databaseName, containerName);
            var query = _container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task UpdateItemAsync<T>(string databaseName, string containerName, T item, string id)
        {
            var _container = _cosmosClient.GetContainer(databaseName, containerName);
            await _container.UpsertItemAsync<T>(item, new PartitionKey(id));
        }

    }
}
