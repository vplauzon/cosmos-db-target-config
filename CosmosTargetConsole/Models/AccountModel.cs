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

        public async Task ConvergeTargetAsync(ExecutionContext context)
        {
            var currentDatabases = await context.Gateway.GetDatabasesAsync();
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

            await RemovingDbAsync(context, toRemove);
            await AddingDbAsync(context, toCreate);
            await UpdatingDbAsync(context, toUpdate);
        }

        private async Task RemovingDbAsync(
            ExecutionContext context,
            IEnumerable<Database> toRemove)
        {
            foreach (var db in toRemove)
            {
                Console.WriteLine($"Removing database:  {db.Id}");
                if (!context.CanDestroyDatabase)
                {
                    Console.WriteLine("(Skipped:  add Destructive Flags 'database' for destroying dbs)");
                }
                else
                {
                    await context.Gateway.DeleteDatabaseAsync(db);
                }
            }
        }

        private async Task AddingDbAsync(
            ExecutionContext context,
            IEnumerable<DatabaseModel> toCreate)
        {
            foreach (var target in toCreate)
            {
                Console.WriteLine($"Adding database:  {target.Name}");

                await target.AddDatabaseAsync(context);
            }
        }

        private async Task UpdatingDbAsync(
            ExecutionContext context,
            IEnumerable<Database> toUpdate)
        {
            foreach (var db in toUpdate)
            {
                var target = (from t in Databases
                              where t.Name == db.Id
                              select t).First();

                Console.WriteLine($"Updating database:  {db.Id}");
                await target.ConvergeTargetAsync(context.AddDatabase(db));
            }
        }
    }
}