using AdventureWorks.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureWorks.Context
{
    public class AdventureWorksCosmosContext : IAdventureWorksProductContext
    {
        private readonly Container _container;
        public AdventureWorksCosmosContext(string connectionString, string database = "Retail", string container = "Online")
        {
            _container = new CosmosClient(connectionString)
                        .GetDatabase(database)
                        .GetContainer(container);
        }

        public async Task<Model> FindModelAsync(Guid id, string category)
        {
            var result = await _container.ReadItemAsync<Model>(id.ToString(), new PartitionKey(category));

            return result;
        }

        public async Task<List<Model>> GetModelsAsync()
        {
            var iterator = _container.GetItemQueryIterator<Model>("SELECT c.id,c.name,c.category,c.description,c.photo FROM c");

            List<Model> documents = new List<Model>();

            while (iterator.HasMoreResults)
            {
                var currentDocuments = await iterator.ReadNextAsync();
                documents.AddRange(currentDocuments);
            }

            return documents;
        }

        public async Task<Product> FindProductAsync(Guid id)
        {
            string queryText = $@"SELECT *
                              FROM c in models.products
                              WHERE c.id = @id";

            QueryDefinition queryDef = new QueryDefinition(queryText)
                            .WithParameter("@id", id);

            var iterator = _container.GetItemQueryIterator<Product>(queryDef);

            List<Product> matches = new List<Product>();
            while (iterator.HasMoreResults)
            {
                var next = await iterator.ReadNextAsync();
                matches.AddRange(next);
            }

            return matches.SingleOrDefault();
        }
    }
}