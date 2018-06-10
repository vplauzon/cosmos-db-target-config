using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace CosmosTargetConsole.Models
{
    public class CollectionModel
    {
        public string Name { get; set; }

        public ThroughputModel Throughput { get; set; }

        public string PartitionKey { get; set; }

        public async Task AddCollectionAsync(CosmosGateway gateway, Database db, string[] destructiveFlags)
        {
            var collection = await gateway.AddCollectionAsync(db, Name, PartitionKey);

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

            if (offer == null && Throughput != null)
            {
                throw new NotImplementedException();
            }
            else if (offer != null)
            {
                var throughput = Throughput ?? ThroughputModel.CreateDefaultUnpartitioned();

                if (offer.Content.OfferThroughput != throughput.RU
                    || offer.Content.OfferIsRUPerMinuteThroughputEnabled != throughput.EnableRUPerMinute)
                {
                    var newOffer = await gateway.ReplaceOfferAsync(
                        offer,
                        throughput.RU,
                        throughput.EnableRUPerMinute);

                    if (newOffer.Content.OfferThroughput != throughput.RU)
                    {
                        throw new InvalidOperationException("Can't sync throughput RU");
                    }
                    if (newOffer.Content.OfferIsRUPerMinuteThroughputEnabled
                        != throughput.EnableRUPerMinute)
                    {
                        throw new InvalidOperationException(
                            "Can't sync throughput enable RU per minute");
                    }
                }
            }
        }
    }
}