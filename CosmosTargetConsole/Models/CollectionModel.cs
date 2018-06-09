using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace CosmosTargetConsole.Models
{
    public class CollectionModel
    {
        public string Name { get; set; }

        public Task ConvergeTargetAsync(
            CosmosGateway gateway,
            Database db,
            DocumentCollection collection,
            string[] destructiveFlags)
        {
            return Task.CompletedTask;
        }
    }
}