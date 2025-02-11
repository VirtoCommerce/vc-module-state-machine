using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.StateMachineModule.Data.Repositories;

namespace VirtoCommerce.StateMachineModule.Data.SqlServer;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<StateMachineDbContext>
{
    public StateMachineDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<StateMachineDbContext>();
        var connectionString = args.Length != 0 ? args[0] : "Server=(local);User=virto;Password=virto;Database=VirtoCommerce3;";

        builder.UseSqlServer(
            connectionString,
            options => options.MigrationsAssembly(typeof(SqlServerDataAssemblyMarker).Assembly.GetName().Name));

        return new StateMachineDbContext(builder.Options);
    }
}
