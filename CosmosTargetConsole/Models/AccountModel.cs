using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var targetIds = from db in (Databases ?? new DatabaseModel[0])
                            select db.Name;
            var toCreate = from db in (Databases ?? new DatabaseModel[0])
                           where !currentIds.Contains(db.Name)
                           select db;
            var toRemove = from db in currentDatabases
                           where !targetIds.Contains(db.Id)
                           select db;
            var toUpdate = from db in currentDatabases
                           where targetIds.Contains(db.Id)
                           select db;

            await RemovingDbAsync(gateway, toRemove);
            await AddingDbAsync(gateway, toCreate);
            await UpdatingDbAsync(gateway, toUpdate);
        }

        private async Task RemovingDbAsync(
            CosmosGateway gateway,
            IEnumerable<Database> toRemove)
        {
            var doDestroyDb = DestructiveFlags.Contains("database");

            foreach (var db in toRemove)
            {
                Console.WriteLine($"Removing database:  {db.Id}");
                if (!doDestroyDb)
                {
                    Console.WriteLine("(Skipped:  add Destructive Flags 'database' for destroying dbs)");
                }
                else
                {
                    await gateway.DeleteDatabaseAsync(db);
                }
            }
        }

        private async Task AddingDbAsync(
            CosmosGateway gateway,
            IEnumerable<DatabaseModel> toCreate)
        {
            foreach (var target in toCreate)
            {
                Console.WriteLine($"Adding database:  {target.Name}");

                await target.AddDatabaseAsync(
                    gateway,
                    DestructiveFlags);
            }
        }

        private async Task UpdatingDbAsync(
            CosmosGateway gateway,
            IEnumerable<Database> toUpdate)
        {
            foreach (var db in toUpdate)
            {
                var target = (from t in Databases
                              where t.Name == db.Id
                              select t).First();

                Console.WriteLine($"Updating database:  {db.Id}");
                await target.ConvergeTargetAsync(gateway, db, DestructiveFlags);
            }
        }
    }
}