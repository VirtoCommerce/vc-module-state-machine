using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.StateMachineModule.Data.Repositories;

namespace VirtoCommerce.StateMachineModule.Data.PostgreSql;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<StateMachineDbContext>
{
    public StateMachineDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<StateMachineDbContext>();
        var connectionString = args.Any() ? args[0] : "Server=localhost;Username=virto;Password=virto;Database=VirtoCommerce3;";

        builder.UseNpgsql(
            connectionString,
            options => options.MigrationsAssembly(typeof(PostgreSqlDataAssemblyMarker).Assembly.GetName().Name));

        return new StateMachineDbContext(builder.Options);
    }
}
