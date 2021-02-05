using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace EfCore
{
    public class MyDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=127.0.0.1;Database=master;User=sa;Password=yourStrong(!)Password", options => options.EnableRetryOnFailure());

            optionsBuilder.UseLoggerFactory(new LoggerFactory(new[] {new DebugLoggerProvider()}));
            optionsBuilder.EnableSensitiveDataLogging();
        }

        public DbSet<UserEntity> Users { get; set; }
    }
}