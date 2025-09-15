using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.StateMachineModule.Core.Models.Conditions;
public class StateMachineConditionHasAccountType : ConditionTree
{
    public bool NotHas { get; set; }
    public string AccountType { get; set; }

    public override bool IsSatisfiedBy(IEvaluationContext context)
    {
        var result = false;
        if (context is StateMachineTriggerContext stateMachineTriggerContext)
        {
            result = NotHas != stateMachineTriggerContext.Principal.IsInRole($"__{AccountType.ToLower()}");
        }

        return result;
    }
}
