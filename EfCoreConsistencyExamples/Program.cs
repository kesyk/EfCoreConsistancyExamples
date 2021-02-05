using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EfCore
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var dbContext = new MyDbContext())
            {
                await dbContext.Database.MigrateAsync();
                await dbContext.Database.ExecuteSqlCommandAsync("DELETE FROM Users");
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