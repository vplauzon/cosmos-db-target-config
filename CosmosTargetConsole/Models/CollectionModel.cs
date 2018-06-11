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

        public StoredProcedureModel[] StoredProcedures { get; set; }

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
            await ConvergeStoredProcedureAsync(gateway, collection, destructiveFlags);
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
                throw new NotSupportedException(
                    "Can't add request units on a collection after creation");
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

        private async Task ConvergeStoredProcedureAsync(
            CosmosGateway gateway,
            DocumentCollection collection,
            string[] destructiveFlags)
        {
            var currentSprocs = await gateway.GetStoredProceduresAsync(collection);
            var currentIds = from s in currentSprocs
                             select s.Id;
            var targetIds = from s in (StoredProcedures ?? new StoredProcedureModel[0])
                            select s.Name;
            var toCreate = from s in (StoredProcedures ?? new StoredProcedureModel[0])
                           where !currentIds.Contains(s.Name)
                           select s;
            var toRemove = from s in currentSprocs
                           where !targetIds.Contains(s.Id)
                           select s;
            var toUpdate = from s in currentSprocs
                           where targetIds.Contains(s.Id)
                           select s;
            var doDestroy = destructiveFlags.Contains("storedProcedure");

            await RemovingStoredProceduresAsync(gateway, toRemove, doDestroy);
            await AddingStoredProceduresAsync(
                gateway,
                collection,
                toCreate,
                destructiveFlags);
            await UpdatingStoredProceduresAsync(
                gateway,
                collection,
                toUpdate,
                destructiveFlags);
        }

        private async Task RemovingStoredProceduresAsync(
            CosmosGateway gateway,
            IEnumerable<StoredProcedure> toRemove,
            bool doDestroy)
        {
            foreach (var sproc in toRemove)
            {
                Console.WriteLine($"Removing stored procedure:  {sproc.Id}");
                if (!doDestroy)
                {
                    Console.WriteLine("(Skipped:  add Destructive Flags "
                        + "'storedProcedure' for destroying stored procedures)");
                }
                else
                {
                    await gateway.DeleteStoredProcedureAsync(sproc);
                }
            }
        }

        private Task AddingStoredProceduresAsync(
            CosmosGateway gateway,
            DocumentCollection collection,
            IEnumerable<StoredProcedureModel> toCreate,
            string[] destructiveFlags)
        {
            throw new NotImplementedException();
        }

        private Task UpdatingStoredProceduresAsync(
            CosmosGateway gateway,
            DocumentCollection collection,
            IEnumerable<StoredProcedure> toUpdate,
            string[] destructiveFlags)
        {
            throw new NotImplementedException();
        }
    }
}