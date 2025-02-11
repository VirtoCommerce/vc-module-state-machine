using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Data.Commands;
public class CreateStateMachineDefinitionCommand : ICommand<StateMachineDefinition>
{
    public StateMachineDefinition Definition { get; set; }
}
