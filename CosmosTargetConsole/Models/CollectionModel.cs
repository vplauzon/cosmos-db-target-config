using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace CosmosTargetConsole.Models
{
    public class CollectionModel
    {
        private const int DEFAULT_RU_UNPARTITIONED = 400;
        private const int DEFAULT_RU_PARTITIONED = 10000;

        public string Name { get; set; }

        public int? RequestUnits { get; set; }

        public string PartitionKey { get; set; }

        public async Task AddCollectionAsync(CosmosGateway gateway, Database db, string[] destructiveFlags)
        {
            var ru = RequestUnits ??
                (PartitionKey == null ? DEFAULT_RU_PARTITIONED : DEFAULT_RU_UNPARTITIONED);
            var collection = await gateway.AddCollectionAsync(
                db,
                Name,
                PartitionKey,
                ru);

            await ConvergeTargetAsync(gateway, db, collection, destructiveFlags);
        }

        public async Task ConvergeTargetAsync(
            CosmosGateway gateway,
            Database db,
            DocumentCollection collection,
            string[] destructiveFlags)
        {
            ValidationPartitionKey(collection.PartitionKey);
            await ConvergeOfferAsync(gateway, collection);
        }

        private void ValidationPartitionKey(PartitionKeyDefinition partitionKey)
        {
            if (partitionKey.Paths.Count > 1)
            {
                throw new NotSupportedException("Unsupported partition key with multiple paths");
            }
            else if (partitionKey.Paths.Count == 0 && PartitionKey != null)
            {
                throw new InvalidOperationException("Can't add partition key after creation");
            }
            else if (partitionKey.Paths.Count == 1 && partitionKey.Paths[0] != PartitionKey)
            {
                throw new InvalidOperationException("Can't change partition key after creation");
            }
        }

        private async Task ConvergeOfferAsync(CosmosGateway gateway, DocumentCollection collection)
        {
            var offer = await gateway.GetOfferAsync(collection.SelfLink);

            if (offer == null && RequestUnits != null)
            {
                throw new NotImplementedException();
            }
            else if (offer != null)
            {
                var ru = RequestUnits ?? DEFAULT_RU_UNPARTITIONED;

                if (offer.Content.OfferThroughput != ru)
                {
                    var newOffer = await gateway.ReplaceOfferAsync(
                        offer,
                        ru);

                    if (newOffer.Content.OfferThroughput != ru)
                    {
                        throw new InvalidOperationException("Can't sync throughput RU");
                    }
                }
            }
        }
    }
}