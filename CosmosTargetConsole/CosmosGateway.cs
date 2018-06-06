using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosTargetConsole
{
    public class CosmosGateway
    {
        private readonly DocumentClient _client;
        private readonly Uri _collectionUri;

        public CosmosGateway(Uri endpoint, string key)
        {
            _client = new DocumentClient(endpoint, key);
        }

        public async Task<IList<Database>> GetDatabasesAsync()
        {
            var dbs = await _client.ReadDatabaseFeedAsync();

            return dbs.ToList();
        }

        public async Task DeleteDatabaseAsync(string link)
        {
            await _client.DeleteDatabaseAsync(link);
        }

        private async Task<List<T>> GetAllResultsAsync<T>(IDocumentQuery<T> query)
        {
            var list = new List<T>();

            while (query.HasMoreResults)
            {
                var docs = await query.ExecuteNextAsync<T>();

                list.AddRange(docs);
            }

            return list;
        }
    }
}