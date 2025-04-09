using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.StateMachineModule.Core.Services;
using VirtoCommerce.StateMachineModule.Data.Repositories;

namespace VirtoCommerce.StateMachineModule.Tests.Unit.Shared;
[ExcludeFromCodeCoverage]
public class ServiceBuilder
{
    public ServiceCollection GetServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddTransient<ILoggerFactory, LoggerFactory>();
        services.AddTransient<IOptions<CrudOptions>, CrudOptionsMock>();

        services.AddTransient<IStateMachineRepository, StateMachineRepositoryMock>();
        services.AddTransient<IStateMachineDefinitionService, StateMachineDefinitionServiceStub>();
        services.AddTransient<IStateMachineDefinitionSearchService, StateMachineDefinitionsSearchServiceStub>();
        services.AddTransient<IStateMachineInstanceService, StateMachineInstanceServiceStub>();
        services.AddTransient<IStateMachineInstanceSearchService, StateMachineInstancesSearchServiceStub>();

        return services;
    }
}
