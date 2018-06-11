using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace CosmosTargetConsole.Models
{
    public class StoredProcedureModel
    {
        public string Name { get; set; }

        public string TargetUrl { get; set; }

        public async Task AddStoredProcedureAsync(CosmosGateway gateway, DocumentCollection collection)
        {
            var body = await ContentHelper.GetContentAsync(TargetUrl);

            await gateway.UpsertStoredProcedureAsync(collection, new StoredProcedure
            {
                Id = Name,
                Body = body
            });
        }

        public async Task ConvergeTargetAsync(
            CosmosGateway gateway,
            DocumentCollection collection,
            StoredProcedure sproc)
        {
            var body = await ContentHelper.GetContentAsync(TargetUrl);

            if (body != sproc.Body)
            {
                Console.WriteLine($"Updating stored procedure:  {Name}");
                await gateway.UpsertStoredProcedureAsync(collection, new StoredProcedure
                {
                    Id = Name,
                    Body = body
                });
            }
        }
    }
}