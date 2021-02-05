using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EfCoreConsistencyExamples
{
    public class TriggersExample
    {
        public async Task Start()
        {
            Console.WriteLine("TRIGGERS EXAMPLE");

            await CreateTrigger();

            // Staring 10 async tasks and make them execute insert->update->delete
            await Task.WhenAll(Enumerable.Range(1, 10).Select(taskIndex =>
                InsertAsync(taskIndex)
                    .ContinueWith(insertTask =>
                        UpdateAsync(insertTask.Result.TaskId, insertTask.Result.UserId)
                            // Can be commented to see that values are consisted
                            .ContinueWith(updateTask =>
                                DeleteAsync(insertTask.Result.TaskId, insertTask.Result.UserId))
                    )));

            await DeleteTrigger();
        }

        private async Task CreateTrigger()
        {
            using (var dbContext = new MyDbContext())
            {
                await dbContext.Database.ExecuteSqlCommandAsync(@"
                    CREATE TRIGGER raw_numbers_users ON Users
                       AFTER INSERT
                       AS
                       BEGIN
                         UPDATE Users
                         SET Data=(SELECT COUNT(Data) FROM Users)+1
                         FROM inserted
                         WHERE Users.id = inserted.id;
                       END"
                );
            }
        }

        private async Task DeleteTrigger()
        {
            using (var dbContext = new MyDbContext())
            {
                await dbContext.Database.ExecuteSqlCommandAsync("drop trigger raw_numbers_users;");
            }
        }

        private async Task<(int TaskId, int UserId)> InsertAsync(int id)
        {
            using (var dbContext = new MyDbContext())
            {
                var userToAdd = new UserEntity();

                dbContext.Users.Add(userToAdd);

                await dbContext.SaveChangesAsync();

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