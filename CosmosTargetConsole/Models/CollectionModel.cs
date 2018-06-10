using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace CosmosTargetConsole.Models
{
    public class CollectionModel
    {
        public string Name { get; set; }

        public ThroughputModel Throughput { get; set; }

        public async Task ConvergeTargetAsync(
            CosmosGateway gateway,
            Database db,
            DocumentCollection collection,
            string[] destructiveFlags)
        {
            await ConvergeOfferAsync(gateway, collection);
        }

        private async Task ConvergeOfferAsync(CosmosGateway gateway, DocumentCollection collection)
        {
            var offer = await gateway.GetOfferAsync(collection.SelfLink);

            if (offer == null && Throughput != null)
            {
                throw new NotImplementedException();
            }
            else if (offer != null && Throughput == null)
            {
                throw new NotImplementedException();
            }
            else if (offer != null && Throughput != null)
            {
                if (offer.Content.OfferThroughput != Throughput.RU
                    || offer.Content.OfferIsRUPerMinuteThroughputEnabled != Throughput.EnableRUPerMinute)
                {
                    await gateway.ReplaceOfferAsync(
                        offer,
                        Throughput.RU,
                        Throughput.EnableRUPerMinute);
                }
            }
        }
    }
}