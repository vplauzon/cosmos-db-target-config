using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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

        #region Database Operations
        public async Task<IList<Database>> GetDatabasesAsync()
        {
            var dbs = await _client.ReadDatabaseFeedAsync();

            return dbs.ToList();
        }
        public async Task DeleteDatabaseAsync(Database database)
        {
            await _client.DeleteDatabaseAsync(database.SelfLink);
        }

        public async Task<Database> AddDatabaseAsync(string name)
        {
            var response = await _client.CreateDatabaseAsync(new Database { Id = name });

            return response.Resource;
        }
        #endregion

        #region Collection Operations
        public async Task<IList<DocumentCollection>> GetCollectionsAsync(Database db)
        {
            var collections = await _client.ReadDocumentCollectionFeedAsync(db.CollectionsLink);

            return collections.ToList();
        }

        public async Task DeleteCollectionAsync(DocumentCollection collection)
        {
            await _client.DeleteDocumentCollectionAsync(collection.SelfLink);
        }

        public async Task<DocumentCollection> AddCollectionAsync(Database db, string name)
        {
            var response = await _client.CreateDocumentCollectionAsync(
                db.SelfLink,
                new DocumentCollection
                {
                    Id = name
                });

            return response.Resource;
        }
        #endregion

        #region Offer Operations
        public async Task<OfferV2> GetOfferAsync(string resourceLink)
        {
            var offerQuery = from o in _client.CreateOfferQuery()
                             where o.ResourceLink == resourceLink
                             select o;
            var offer = (await GetAllResultsAsync(offerQuery.AsDocumentQuery()))
                .FirstOrDefault()
                as OfferV2;

            return offer;
        }

        public async Task<OfferV2> ReplaceOfferAsync(OfferV2 offer, int ru, bool enableRUPerMinute)
        {
            var newOffer = new OfferV2(
                offer,
                ru,
                enableRUPerMinute);
            var response = await _client.ReplaceOfferAsync(newOffer);

            return response.Resource as OfferV2;
        }
        #endregion

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