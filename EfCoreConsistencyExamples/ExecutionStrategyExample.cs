using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EfCore
{
    public class ExecutionStrategyExample
    {
        public async Task Start()
        {
            Console.WriteLine("EXECUTION STRATEGY EXAMPLE");

            // Staring 10 async tasks and make them execute insert->update->delete
            await Task.WhenAll(Enumerable.Range(1, 10).Select(taskIndex =>
                InsertAsync(taskIndex)
                    .ContinueWith(insertTask =>
                        UpdateAsync(insertTask.Result.TaskId, insertTask.Result.UserId)
                            // Can be commented to see that values are consisted
                            .ContinueWith(updateTask =>
                                DeleteAsync(insertTask.Result.TaskId, insertTask.Result.UserId))
                    )));
        }

        private async Task<(int TaskId, int UserId)> InsertAsync(int id)
        {
            using (var dbContext = new MyDbContext())
            {
                var userToAdd = new UserEntity();
                dbContext.Users.Add(userToAdd);

                var strategy = dbContext.Database.CreateExecutionStrategy();
                await strategy.ExecuteInTransactionAsync(
                    dbContext,
                    async (context, token) =>
                    {
                        userToAdd.Data = await dbContext.Users.MaxAsync(x => x.Data, token) + 1 ?? 1;
                        return await dbContext.SaveChangesAsync(false, token);
                    }, (context, token) => context.Users.AnyAsync(x => x.Id == userToAdd.Id, token),
                    IsolationLevel.Serializable);

                dbContext.ChangeTracker.AcceptAllChanges();

                Console.WriteLine($"T{id} INSERT Has Been Finished");

                return (id, userToAdd.Id);
            }
        }

        private async Task DeleteAsync(int id, int userId)
        {
            using (var dbContext = new MyDbContext())
            {
                dbContext.Users.Remove(new UserEntity {Id = userId});

                await dbContext.SaveChangesAsync();

                Console.WriteLine($"T{id} REMOVE Has Been Finished");
            }
        }

        private async Task UpdateAsync(int id, int userId)
        {
            Console.WriteLine(userId);
            using (var dbContext = new MyDbContext())
            {
                var entity = await dbContext.Users.FirstAsync(x => x.Id == userId);
                entity.Data += 1;

                await dbContext.SaveChangesAsync();

                Console.WriteLine($"T{id} UPDATE Has Been Finished");
            }
        }
    }
}