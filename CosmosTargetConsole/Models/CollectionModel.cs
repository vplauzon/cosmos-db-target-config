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

        public async Task AddCollectionAsync(ExecutionContext context)
        {
            var collection = await context.Gateway.AddCollectionAsync(
                context.Database,
                Name,
                PartitionKey,
                RequestUnits);

            await ConvergeTargetAsync(context.AddCollection(collection));
        }

        public async Task ConvergeTargetAsync(ExecutionContext context)
        {
            ValidationPartitionKey(context.Collection.PartitionKey);
            await ConvergeOfferAsync(context);
            await ConvergeStoredProcedureAsync(context);
        }

        private int GetEffectiveRequestUnits()
        {
            return RequestUnits ??
                (PartitionKey == null ? DEFAULT_RU_PARTITIONED : DEFAULT_RU_UNPARTITIONED);
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

        private async Task ConvergeOfferAsync(ExecutionContext context)
        {
            var offer = await context.Gateway.GetOfferAsync(context.Collection.SelfLink);

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
                    var newOffer = await context.Gateway.ReplaceOfferAsync(
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

        private async Task ConvergeStoredProcedureAsync(ExecutionContext context)
        {
            var currentSprocs =
                await context.Gateway.GetStoredProceduresAsync(context.Collection);
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

            await RemovingStoredProceduresAsync(context, toRemove);
            await AddingStoredProceduresAsync(context, toCreate);
            await UpdatingStoredProceduresAsync(context, toUpdate);
        }

        private async Task RemovingStoredProceduresAsync(
            ExecutionContext context,
            IEnumerable<StoredProcedure> toRemove)
        {
            foreach (var sproc in toRemove)
            {
                Console.WriteLine($"Removing stored procedure:  {sproc.Id}");
                if (!context.CanDestroyStoredProcedure)
                {
                    Console.WriteLine("(Skipped:  add Destructive Flags "
                        + "'storedProcedure' for destroying stored procedures)");
                }
                else
                {
                    await context.Gateway.DeleteStoredProcedureAsync(sproc);
                }
            }
        }

        private async Task AddingStoredProceduresAsync(
            ExecutionContext context,
            IEnumerable<StoredProcedureModel> toCreate)
        {
            foreach (var target in toCreate)
            {
                Console.WriteLine($"Adding stored procedure:  {target.Name}");

                await target.AddStoredProcedureAsync(context);
            }

        }

        private async Task UpdatingStoredProceduresAsync(
            ExecutionContext context,
            IEnumerable<StoredProcedure> toUpdate)
        {
            foreach (var sproc in toUpdate)
            {
                var target = (from t in StoredProcedures
                              where t.Name == sproc.Id
                              select t).First();

                await target.ConvergeTargetAsync(context, sproc);
            }
        }
    }
}