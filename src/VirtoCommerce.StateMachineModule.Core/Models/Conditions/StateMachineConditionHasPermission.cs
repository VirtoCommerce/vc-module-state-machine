using System.Linq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.StateMachineModule.Core.Models.Conditions;
public class StateMachineConditionHasPermission : ConditionTree
{
    public bool NotHas { get; set; }
    public string[] Permissions { get; set; }

    public override bool IsSatisfiedBy(IEvaluationContext context)
    {
        var result = false;
        if (context is StateMachineTriggerContext stateMachineTriggerContext)
        {
            result = stateMachineTriggerContext.Principal.IsInRole("__administrator");
            var userPermissions = stateMachineTriggerContext.Principal.FindAll("permission").Select(x => x.Value);
            if (!NotHas)
            {
                if (userPermissions.Intersect(Permissions).Count() > 0)
                {
                    result = true;
                }
            }
            else
            {
                if (userPermissions.Intersect(Permissions).Count() == 0)
                {
                    result = true;
                }
            }
        }

        return result;
    }
}
