using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.StateMachineModule.Core.Models;
public class StateMachineTransition
{
    public string Trigger { get; set; }
    public string Description { get; set; }
    public string ToState { get; set; }
    [Obsolete("Use Attributes, backward compatibility only")]
    public string Icon => Attributes?.FirstOrDefault(x => x.AttributeKey == "Icon")?.Value;
    public string LocalizedValue { get; set; }

    public IConditionTree Condition { get; set; }
    public IList<StateMachineAttribute> Attributes { get; set; }
}
