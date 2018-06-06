using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace CosmosTargetConsole.Models
{
    public class DatabaseModel
    {
        public string Name { get; set; }

        public CollectionModel[] Collections { get; set; }

        public async Task ConvergeTargetAsync(CosmosGateway gateway, Database db)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}