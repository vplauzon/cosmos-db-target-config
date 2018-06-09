using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace CosmosTargetConsole.Models
{
    public class DatabaseModel
    {
        public string Name { get; set; }

        public CollectionModel[] Collections { get; set; }

        public async Task ConvergeTargetAsync(
            CosmosGateway gateway,
            Database db,
            string outputPrefix,
            string[] destructiveFlags)
        {
            var currentCollections = await gateway.GetCollectionsAsync(db);
            var currentIds = from coll in currentCollections
                             select coll.Id;
            var targetIds = from coll in Collections
                            select coll.Name;
            var toCreate = from coll in Collections
                           where !currentIds.Contains(coll.Name)
                           select coll;
            var toRemove = from coll in currentCollections
                           where !targetIds.Contains(coll.Id)
                           select coll;
            var toUpdate = from coll in currentCollections
                           where targetIds.Contains(coll.Id)
                           select coll;
            var doDestroy = destructiveFlags.Contains("collection");

            await RemovingCollectionsAsync(gateway, toRemove, doDestroy, outputPrefix);
            await AddingCollectionsAsync(
                gateway,
                db,
                toCreate,
                outputPrefix,
                destructiveFlags);
            await UpdatingCollectionsAsync(
                gateway,
                db,
                toUpdate,
                outputPrefix,
                destructiveFlags);
        }

        private async Task RemovingCollectionsAsync(
            CosmosGateway gateway,
            IEnumerable<DocumentCollection> toRemove,
            bool doDestroy,
            string outputPrefix)
        {
            if (toRemove.Any())
            {
                Console.WriteLine(outputPrefix + "Removing databases:");

                foreach (var collection in toRemove)
                {
                    Console.WriteLine(outputPrefix + collection.Id);
                    if (!doDestroy)
                    {
                        Console.WriteLine(outputPrefix
                            + "(Skipped:  add Destructive Flags 'collection' for destroying collections)");
                    }
                    else
                    {
                        await gateway.DeleteCollectionAsync(collection);
                    }
                }
            }
        }

        private async Task AddingCollectionsAsync(
            CosmosGateway gateway,
            Database db,
            IEnumerable<CollectionModel> toCreate,
            string outputPrefix,
            string[] destructiveFlags)
        {
            if (toCreate.Any())
            {
                Console.WriteLine(outputPrefix + "Adding collections:");

                foreach (var target in toCreate)
                {
                    Console.WriteLine(outputPrefix + target.Name);

                    var collection = await gateway.AddCollectionAsync(db, target.Name);

                    await target.ConvergeTargetAsync(
                        gateway,
                        db,
                        collection,
                        outputPrefix + "  ",
                        destructiveFlags);
                }
            }
        }

        private async Task UpdatingCollectionsAsync(
            CosmosGateway gateway,
            Database db,
            IEnumerable<DocumentCollection> toUpdate,
            string outputPrefix,
            string[] destructiveFlags)
        {
            if (toUpdate.Any())
            {
                Console.WriteLine(outputPrefix + "Updating collections:");

                foreach (var coll in toUpdate)
                {
                    var target = (from t in Collections
                                  where t.Name == coll.Id
                                  select t).First();

                    Console.WriteLine(coll.Id);
                    await target.ConvergeTargetAsync(
                        gateway,
                        db,
                        coll,
                        "  " + outputPrefix,
                        destructiveFlags);
                }
            }
        }
    }
}