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
        StateMachineDefinition stateMachineDefinition = null;
        if (!string.IsNullOrEmpty(entityType) && entityType != "InvalidEntityType")
        {
            stateMachineDefinition = new StateMachineDefinition
            {
                Id = "TestDefinitionId",
                EntityType = entityType,
                States = new List<StateMachineState>
                {
                    new StateMachineState
                    {
                        Name = "TestState",
                        IsInitial = true,
                        IsFinal = true
                    }
                }
            };
        }
        return Task.FromResult(stateMachineDefinition);
    }

    public Task<IList<StateMachineDefinition>> GetAsync(IList<string> ids, string responseGroup = null, bool clone = true)
    {
        var result = new List<StateMachineDefinition>();

        foreach (var id in ids)
        {
            if (!string.IsNullOrEmpty(id) && id != "InvalidDefinitionId")
            {
                var stateMachineDefinition = new StateMachineDefinition
                {
                    Id = id,
                    States = new List<StateMachineState>()
                };
                result.Add(stateMachineDefinition);
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
