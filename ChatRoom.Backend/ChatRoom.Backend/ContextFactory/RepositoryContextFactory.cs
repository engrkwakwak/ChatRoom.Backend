using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Repository;

namespace ChatRoom.Backend.ContextFactory {
    public class RepositoryContextFactory : IDesignTimeDbContextFactory<RepositoryContext> {
        public RepositoryContext CreateDbContext(string[] args) {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<RepositoryContext>()
                .UseSqlServer(configuration.GetConnectionString("SqlConnection"),
                    b => b.MigrationsAssembly("ChatRoom.Backend"));

            return new RepositoryContext(builder.Options);
        }
    }
}
