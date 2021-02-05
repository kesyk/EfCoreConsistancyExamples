using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EfCoreConsistencyExamples
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            // Migrating and cleaning the table
            using (var dbContext = new MyDbContext())
            {
                await dbContext.Database.MigrateAsync();
                await dbContext.Database.ExecuteSqlCommandAsync("TRUNCATE TABLE Users");
            }
            
            // Execution Strategy example
            var executionStrategyExample = new ExecutionStrategyExample();
            await executionStrategyExample.Start();
            
            // Triggers example
            var triggersExample = new TriggersExample();
            await triggersExample.Start();

            Console.ReadKey();
        }

    }
}