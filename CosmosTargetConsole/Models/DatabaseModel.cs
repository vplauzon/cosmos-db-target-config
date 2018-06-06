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
            var doDestroyCollections = destructiveFlags.Contains("collection");

            await RemovingCollectionAsync(gateway, toRemove, doDestroyCollections, outputPrefix);

            //var added = await AddingCollectionAsync(gateway, toCreate);

            //await UpdatingCollectionAsync(gateway, currentCollections.Concat(added));
        }

        private async Task RemovingCollectionAsync(
            CosmosGateway gateway,
            IEnumerable<DocumentCollection> toRemove,
            bool doDestroyCollections,
            string outputPrefix)
        {
            if (toRemove.Any())
            {
                Console.WriteLine(outputPrefix + "Removing databases:");

                foreach (var collection in toRemove)
                {
                    Console.WriteLine(outputPrefix + collection.Id);
                    if (!doDestroyCollections)
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
    }
}