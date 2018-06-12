using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace CosmosTargetConsole.Models
{
    public class StoredProcedureModel
    {
        public string Name { get; set; }

        public Uri TargetUrl { get; set; }

        public async Task AddStoredProcedureAsync(ExecutionContext context)
        {
            var body = await GetBodyAsync(context);

            await context.Gateway.UpsertStoredProcedureAsync(context.Collection, new StoredProcedure
            {
                Id = Name,
                Body = body
            });
        }

        public async Task ConvergeTargetAsync(
            ExecutionContext context,
            StoredProcedure sproc)
        {
            var body = await GetBodyAsync(context);

            if (body != sproc.Body)
            {
                Console.WriteLine($"Updating stored procedure:  {Name}");
                await context.Gateway.UpsertStoredProcedureAsync(
                    context.Collection,
                    new StoredProcedure
                    {
                        Id = Name,
                        Body = body
                    });
            }
        }

        private async Task<string> GetBodyAsync(ExecutionContext context)
        {
            var effectiveTargetUri = new Uri(context.ConfigUrl, TargetUrl);
            var body = await ContentHelper.GetContentAsync(effectiveTargetUri);

            return body;
        }
    }
}