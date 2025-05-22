using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.StateMachineModule.Core.Models;
using VirtoCommerce.StateMachineModule.Core.Services;

namespace VirtoCommerce.StateMachineModule.Tests.Unit.Shared;
[ExcludeFromCodeCoverage]
public class StateMachineInstanceServiceStub : IStateMachineInstanceService
{
    public Task<StateMachineInstance> CreateStateMachineInstanceAsync(string stateMachineDefinitionId, string stateMachineInstanceId, IHasDynamicProperties entity, string state = null)
    {
        var stateMachineInstance = new StateMachineInstance
        {
            Id = stateMachineInstanceId,
        };
        var stateMachineDefinition = GetStateMachineDefinition();
        stateMachineInstance.Configure(stateMachineDefinition, "Null");
        return Task.FromResult(stateMachineInstance);
    }

    public Task DeleteAsync(IList<string> ids, bool softDelete = false)
    {
        throw new NotImplementedException();
    }

    public Task<StateMachineInstance> FireTriggerAsync(StateMachineInstance stateMachineInstance, string trigger, StateMachineTriggerContext context)
    {
        stateMachineInstance.Start(context);
        stateMachineInstance.Fire(trigger, context);
        return Task.FromResult(stateMachineInstance);
    }

    public Task<IList<StateMachineInstance>> GetAsync(IList<string> ids, string responseGroup = null, bool clone = true)
    {
        var result = new List<StateMachineInstance>();
        var stateMachineDefinition = GetStateMachineDefinition();

        foreach (var id in ids)
        {
            if (!string.IsNullOrEmpty(id) && id != "InvalidInstanceId")
            {
                var stateMachineInstance = new StateMachineInstance { Id = id };
                stateMachineInstance.Configure(stateMachineDefinition, "Null");
                result.Add(stateMachineInstance);
            }
        }

        return Task.FromResult(result as IList<StateMachineInstance>);
    }

    public Task<StateMachineInstance> GetForEntity(string entityId, string entityType)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(IList<StateMachineInstance> models)
    {
        return Task.CompletedTask;
    }

    private StateMachineDefinition GetStateMachineDefinition()
    {
        return new StateMachineDefinition
        {
            Id = "TestStateMachineDefinitionId",
            Name = "My test state machine definition",
            EntityType = "TestEntityType",
            IsActive = true,
            Version = "Version1",
            States = TestHepler.LoadArrayFromJsonFile("testStateMachineDefinition.json").ToObject<List<StateMachineState>>(),
            CreatedDate = new DateTime(2025, 02, 14),
            ModifiedDate = new DateTime(2025, 02, 14),
            CreatedBy = "Test Created By",
            ModifiedBy = "Test Modified By"
        };
    }
}
