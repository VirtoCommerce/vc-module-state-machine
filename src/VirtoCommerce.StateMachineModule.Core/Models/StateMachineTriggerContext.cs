using System.Security.Claims;
using VirtoCommerce.CoreModule.Core.Common;

namespace VirtoCommerce.StateMachineModule.Core.Models;
public class StateMachineTriggerContext : EvaluationContextBase
{
    public StateMachineInstance StateMachineInstance { get; set; }
    public ClaimsPrincipal Principal { get; set; }
    public string EntityId { get; set; }
}
