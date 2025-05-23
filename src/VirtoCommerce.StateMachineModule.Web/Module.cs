using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
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
using VirtoCommerce.StateMachineModule.Data.ExportImport;
using VirtoCommerce.StateMachineModule.Data.MySql;
using VirtoCommerce.StateMachineModule.Data.PostgreSql;
using VirtoCommerce.StateMachineModule.Data.Repositories;
using VirtoCommerce.StateMachineModule.Data.Services;
using VirtoCommerce.StateMachineModule.Data.SqlServer;

namespace VirtoCommerce.StateMachineModule.Web;

public class Module : IModule, IHasConfiguration, IExportSupport, IImportSupport
{
    public ManifestModuleInfo ModuleInfo { get; set; }
    public IConfiguration Configuration { get; set; }

    private IApplicationBuilder _appBuilder;

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

        serviceCollection.AddTransient<IStateMachineDefinitionSearchService, StateMachineDefinitionSearchService>();
        serviceCollection.AddTransient<IStateMachineDefinitionService, StateMachineDefinitionService>();

        serviceCollection.AddTransient<IStateMachineInstanceSearchService, StateMachineInstanceSearchService>();
        serviceCollection.AddTransient<IStateMachineInstanceService, StateMachineInstanceService>();

        serviceCollection.AddTransient<IStateMachineLocalizationSearchService, StateMachineLocalizationSearchService>();
        serviceCollection.AddTransient<IStateMachineLocalizationCrudService, StateMachineLocalizationCrudService>();

        serviceCollection.AddTransient<StateMachineTriggerEvent>();

        serviceCollection.AddTransient<StateMachineExportImport>();

        serviceCollection.AddMediatR(configuration => configuration.RegisterServicesFromAssemblyContaining<Anchor>());
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        _appBuilder = appBuilder;

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

    public Task ExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
        ICancellationToken cancellationToken)
    {
        return _appBuilder.ApplicationServices.GetRequiredService<StateMachineExportImport>().DoExportAsync(outStream, options,
            progressCallback, cancellationToken);
    }

    public Task ImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
        ICancellationToken cancellationToken)
    {
        return _appBuilder.ApplicationServices.GetRequiredService<StateMachineExportImport>().DoImportAsync(inputStream, options,
            progressCallback, cancellationToken);
    }
}

