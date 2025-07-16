using System.Security.Claims;
using VirtoCommerce.StateMachineModule.Core.Common;
using VirtoCommerce.StateMachineModule.Core.Models;

namespace VirtoCommerce.StateMachineModule.Data.Commands;
public class FireStateMachineTriggerCommand : ICommand<StateMachineInstance>
{
    public string StateMachineInstanceId { get; set; }
    public string Trigger { get; set; }
    public string EntityId { get; set; }
    public ClaimsPrincipal User { get; set; }
    public string Locale { get; set; }
}
