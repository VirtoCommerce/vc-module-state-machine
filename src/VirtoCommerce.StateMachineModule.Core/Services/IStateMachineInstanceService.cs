using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Core.Services;
public interface IStateMachineInstanceService : ICrudService<StateMachineInstance>
{
    Task<StateMachineInstance> CreateStateMachineInstanceAsync(string stateMachineDefinitionId, string stateMachineInstanceId, IHasDynamicProperties entity, string state = null);
    Task<StateMachineInstance> GetForEntity(string entityId, string entityType);
    Task<StateMachineInstance> FireTriggerAsync(StateMachineInstance stateMachineInstance, string trigger, StateMachineTriggerContext context);
}
