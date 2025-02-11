using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StateMachineModule.Core.Models;
public class StateMachineState : ValueObject
{
    public StateMachineState()
    {
        Type = GetType().Name;
    }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public bool IsInitial { get; set; }
    public bool IsFinal { get; set; }
    public object StateData { get; set; }
    public IList<StateMachineTransition> Transitions { get; set; }
}
