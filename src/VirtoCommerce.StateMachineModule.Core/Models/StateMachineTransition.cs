using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.StateMachineModule.Core.Models;
public class StateMachineTransition
{
    public string Trigger { get; set; }
    public string Description { get; set; }
    public string ToState { get; set; }
    public string Icon { get; set; }
    public string LocalizedValue { get; set; }

    public IConditionTree Condition { get; set; }
}
