

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WorkflowEngine.Infrastructure.ProcessEngine.Presistence;

internal class RepositoryDBContextFactory : IDesignTimeDbContextFactory<RepositoryDBContext>
{
    public RepositoryDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RepositoryDBContext>();

        optionsBuilder.UseNpgsql(
             "Host=localhost;Port=5433;Database=Repository;Username=postgres;Password=postgres"
        );

        return new RepositoryDBContext(optionsBuilder.Options);
    }
}
