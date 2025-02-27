using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.MySql.Extensions;
using VirtoCommerce.Platform.Data.PostgreSql.Extensions;
using VirtoCommerce.Platform.Data.SqlServer.Extensions;
using VirtoCommerce.StateMachineModule.Core;
using VirtoCommerce.StateMachineModule.Core.Events;
using VirtoCommerce.StateMachineModule.Core.Services;
using VirtoCommerce.StateMachineModule.Data;
using VirtoCommerce.StateMachineModule.Data.MySql;
using VirtoCommerce.StateMachineModule.Data.PostgreSql;
using VirtoCommerce.StateMachineModule.Data.Repositories;
using VirtoCommerce.StateMachineModule.Data.Services;
using VirtoCommerce.StateMachineModule.Data.SqlServer;

namespace VirtoCommerce.StateMachineModule.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }
    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContext<StateMachineDbContext>(options =>
        {
            var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
            var connectionString = Configuration.GetConnectionString(ModuleInfo.Id) ?? Configuration.GetConnectionString("VirtoCommerce");

            switch (databaseProvider)
            {
                case "MySql":
                    options.UseMySqlDatabase(connectionString, typeof(MySqlDataAssemblyMarker), Configuration);
                    break;
                case "PostgreSql":
                    options.UsePostgreSqlDatabase(connectionString, typeof(PostgreSqlDataAssemblyMarker), Configuration);
                    break;
                default:
                    options.UseSqlServerDatabase(connectionString, typeof(SqlServerDataAssemblyMarker), Configuration);
                    break;
            }
        });

        serviceCollection.AddTransient<IStateMachineRepository, StateMachineRepository>();
        serviceCollection.AddTransient<Func<IStateMachineRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<IStateMachineRepository>());

        serviceCollection.AddTransient<IStateMachineDefinitionsSearchService, StateMachineDefinitionsSearchService>();
        serviceCollection.AddTransient<IStateMachineDefinitionService, StateMachineDefinitionService>();
        serviceCollection.AddTransient<IStateMachineInstancesSearchService, StateMachineInstancesSearchService>();
        serviceCollection.AddTransient<IStateMachineInstanceService, StateMachineInstanceService>();

        serviceCollection.AddTransient<StateMachineTriggerEvent>();

        serviceCollection.AddMediatR(configuration => configuration.RegisterServicesFromAssemblyContaining<Anchor>());
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        var serviceProvider = appBuilder.ApplicationServices;

        // Register settings
        var settingsRegistrar = serviceProvider.GetRequiredService<ISettingsRegistrar>();
        settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

        // Register permissions
        var permissionsRegistrar = serviceProvider.GetRequiredService<IPermissionsRegistrar>();
        permissionsRegistrar.RegisterPermissions(ModuleInfo.Id, "StateMachineModule", ModuleConstants.Security.Permissions.AllPermissions);

        // Apply migrations
        using var serviceScope = serviceProvider.CreateScope();
        using var dbContext = serviceScope.ServiceProvider.GetRequiredService<StateMachineDbContext>();
        dbContext.Database.Migrate();
    }

    public void Uninstall()
    {
        // Nothing to do here
    }
}

