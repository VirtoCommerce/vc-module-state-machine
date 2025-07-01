using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.StateMachineModule.Data.Models;

namespace VirtoCommerce.StateMachineModule.Data.Repositories;

public class StateMachineDbContext : DbContextBase
{
    public StateMachineDbContext(DbContextOptions<StateMachineDbContext> options)
        : base(options)
    {
    }

    protected StateMachineDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StateMachineDefinitionEntity>().ToTable("StateMachineDefinition").HasKey(x => x.Id);
        modelBuilder.Entity<StateMachineDefinitionEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();

        modelBuilder.Entity<StateMachineInstanceEntity>().ToTable("StateMachineInstance").HasKey(x => x.Id);
        modelBuilder.Entity<StateMachineInstanceEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
        modelBuilder.Entity<StateMachineInstanceEntity>().HasOne(x => x.StateMachineDefinition).WithMany()
            .HasForeignKey(x => x.StateMachineId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StateMachineLocalizationEntity>().ToTable("StateMachineLocalization").HasKey(x => x.Id);
        modelBuilder.Entity<StateMachineLocalizationEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();

        modelBuilder.Entity<StateMachineAttributeEntity>().ToTable("StateMachineAttribute").HasKey(x => x.Id);
        modelBuilder.Entity<StateMachineAttributeEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();

        base.OnModelCreating(modelBuilder);

        switch (Database.ProviderName)
        {
            case "Pomelo.EntityFrameworkCore.MySql":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.StateMachineModule.Data.MySql"));
                break;
            case "Npgsql.EntityFrameworkCore.PostgreSQL":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.StateMachineModule.Data.PostgreSql"));
                break;
            case "Microsoft.EntityFrameworkCore.SqlServer":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.StateMachineModule.Data.SqlServer"));
                break;
        }
    }
}
