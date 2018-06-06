using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosTargetConsole.Models
{
    public class AccountModel
    {
        public string[] DestructiveFlags { get; set; }

        public DatabaseModel[] Databases { get; set; }

        public async Task ConvergeTargetAsync(CosmosGateway gateway)
        {
            var currentDatabases = await gateway.GetDatabasesAsync();
            var currentIds = from db in currentDatabases
                             select db.Id;
            var targetIds = from db in Databases
                            select db.Name;
            var toCreateDbs = from db in Databases
                              where !currentIds.Contains(db.Name)
                              select db;
            var toRemoveDbs = from db in currentDatabases
                              where !targetIds.Contains(db.Id)
                              select db;
            var doDestroyDb = DestructiveFlags.Contains("database");

            await RemovingDbAsync(gateway, toRemoveDbs, doDestroyDb);
        }

        private static async Task RemovingDbAsync(
            CosmosGateway gateway,
            IEnumerable<Database> toRemoveDbs,
            bool doDestroyDb)
        {
            if (toRemoveDbs.Any())
            {
                Console.WriteLine("Removing databases:");

                foreach (var db in toRemoveDbs)
                {
                    Console.WriteLine(db.Id);
                    if (!doDestroyDb)
                    {
                        Console.WriteLine("(Skipped:  add Destructive Flags 'database' for destroying dbs)");
                    }
                    else
                    {
                        await gateway.DeleteDatabaseAsync(db.SelfLink);
                    }
                }
            }
        }
    }
}