using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace CosmosTargetConsole.Models
{
    public class DatabaseModel
    {
        public string Name { get; set; }

        public int? RequestUnits { get; set; }

        public CollectionModel[] Collections { get; set; }

        public async Task AddDatabaseAsync(ExecutionContext context)
        {
            var db = await context.Gateway.AddDatabaseAsync(Name, RequestUnits);

            await ConvergeTargetAsync(context.AddDatabase(db));
        }

        public async Task ConvergeTargetAsync(ExecutionContext context)
        {
            var currentCollections =
                await context.Gateway.GetCollectionsAsync(context.Database);
            var currentIds = from coll in currentCollections
                             select coll.Id;
            var targetIds = from coll in (Collections ?? new CollectionModel[0])
                            select coll.Name;
            var toCreate = from coll in (Collections ?? new CollectionModel[0])
                           where !currentIds.Contains(coll.Name)
                           select coll;
            var toRemove = from coll in currentCollections
                           where !targetIds.Contains(coll.Id)
                           select coll;
            var toUpdate = from coll in currentCollections
                           where targetIds.Contains(coll.Id)
                           select coll;

            await ConvergeOfferAsync(context);
            await RemovingCollectionsAsync(context, toRemove);
            await AddingCollectionsAsync(context, toCreate);
            await UpdatingCollectionsAsync(context, toUpdate);
        }

        private async Task ConvergeOfferAsync(ExecutionContext context)
        {
            var offer = await context.Gateway.GetOfferAsync(context.Database.SelfLink);

            if (offer == null && RequestUnits != null)
            {
                throw new NotSupportedException(
                    "Can't add request units on a database after creation");
            }
            else if (offer != null && RequestUnits == null)
            {
                throw new NotSupportedException(
                    "Can't remove request units on a database after creation");
            }
            else if (offer != null && RequestUnits != null)
            {
                if (offer.Content.OfferThroughput != RequestUnits)
                {
                    var newOffer = await context.Gateway.ReplaceOfferAsync(
                        offer,
                        RequestUnits.Value);

                    Console.WriteLine("Update Request Units from "
                        + $"{offer.Content.OfferThroughput} to {RequestUnits}");
                    if (newOffer.Content.OfferThroughput != RequestUnits)
                    {
                        throw new InvalidOperationException("Can't sync throughput RU");
                    }
                }
            }
        }

        private async Task RemovingCollectionsAsync(
            ExecutionContext context,
            IEnumerable<DocumentCollection> toRemove)
        {
            foreach (var collection in toRemove)
            {
                Console.WriteLine($"Removing database:  {collection.Id}");
                if (!context.CanDestroyCollection)
                {
                    Console.WriteLine("(Skipped:  add Destructive Flags "
                        + "'collection' for destroying collections)");
                }
                else
                {
                    await context.Gateway.DeleteCollectionAsync(collection);
                }
            }
        }

        private async Task AddingCollectionsAsync(
            ExecutionContext context,
            IEnumerable<CollectionModel> toCreate)
        {
            foreach (var target in toCreate)
            {
                Console.WriteLine($"Adding collection:  {target.Name}");

                await target.AddCollectionAsync(context);
            }
        }

        private async Task UpdatingCollectionsAsync(
            ExecutionContext context,
            IEnumerable<DocumentCollection> toUpdate)
        {
            foreach (var coll in toUpdate)
            {
                var target = (from t in Collections
                              where t.Name == coll.Id
                              select t).First();

                Console.WriteLine($"Updating collection:  {coll.Id}");
                await target.ConvergeTargetAsync(context.AddCollection(coll));
            }
        }
    }
}