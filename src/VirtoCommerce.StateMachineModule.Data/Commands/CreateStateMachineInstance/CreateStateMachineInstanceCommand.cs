using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Data.Commands;
public class CreateStateMachineInstanceCommand : ICommand<StateMachineInstance>
{
    public string StateMachineDefinitionId { get; set; }
    public string StateMachineInstanceId { get; set; }
    public IHasDynamicProperties Entity { get; set; }
}
