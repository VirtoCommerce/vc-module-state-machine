using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Core.Services;
public interface IStateMachineDefinitionService : ICrudService<StateMachineDefinition>
{
    Task<StateMachineDefinition> GetActiveStateMachineDefinitionAsync(string entityType);
    Task<StateMachineDefinition> SaveStateMachineDefinitionAsync(StateMachineDefinition definition);
}
