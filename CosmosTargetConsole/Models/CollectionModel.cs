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
                        Console.WriteLine("Can't sync throughput RU");
                    }
                    if (newOffer.Content.OfferIsRUPerMinuteThroughputEnabled != throughput.EnableRUPerMinute)
                    {
                        Console.WriteLine("Can't sync throughput enable RU per minute");
                    }
                }
            }
        }
    }
}