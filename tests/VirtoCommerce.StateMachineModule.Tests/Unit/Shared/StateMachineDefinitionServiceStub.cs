using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Tests.Unit.Shared;
[ExcludeFromCodeCoverage]
public class StateMachineDefinitionServiceStub : IStateMachineDefinitionService
{
    public Task DeleteAsync(IList<string> ids, bool softDelete = false)
    {
        throw new System.NotImplementedException();
    }

    public Task<StateMachineDefinition> GetActiveStateMachineDefinitionAsync(string entityType)
    {
        throw new System.NotImplementedException();
    }

    public Task<IList<StateMachineDefinition>> GetAsync(IList<string> ids, string responseGroup = null, bool clone = true)
    {
        var result = new List<StateMachineDefinition>();

        foreach (var id in ids)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var stateMachineInstance = new StateMachineDefinition { Id = id };
                result.Add(stateMachineInstance);
            }
        }

        return Task.FromResult(result as IList<StateMachineDefinition>);
    }

    public Task SaveChangesAsync(IList<StateMachineDefinition> models)
    {
        throw new System.NotImplementedException();
    }

    public Task<StateMachineDefinition> SaveStateMachineDefinitionAsync(StateMachineDefinition definition)
    {
        return Task.FromResult(definition);
    }
}
