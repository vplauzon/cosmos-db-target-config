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

        public async Task AddCollectionAsync(
            CosmosGateway gateway,
            Database db,
            string[] destructiveFlags)
        {
            var collection = await gateway.AddCollectionAsync(
                db,
                Name,
                PartitionKey,
                RequestUnits);

            await ConvergeTargetAsync(gateway, db, collection, destructiveFlags);
        }

        private int GetEffectiveRequestUnits()
        {
            return RequestUnits ??
                (PartitionKey == null ? DEFAULT_RU_PARTITIONED : DEFAULT_RU_UNPARTITIONED);
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
                var ru = GetEffectiveRequestUnits();

                if (offer.Content.OfferThroughput != ru)
                {
                    var newOffer = await gateway.ReplaceOfferAsync(
                        offer,
                        ru);

                    Console.WriteLine("Update Request Units from "
                        + $"{offer.Content.OfferThroughput} to {RequestUnits}");
                    if (newOffer.Content.OfferThroughput != ru)
                    {
                        throw new InvalidOperationException("Can't sync throughput RU");
                    }
                }
            }
        }
    }
}