using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StateMachineModule.Core.Models;
public class StateMachineDefinition : AuditableEntity, ICloneable
{
    public string Version { get; set; }
    public string EntityType { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public IList<StateMachineState> States { get; set; }
    public string StatesGraph { get; set; }
    public string StatesCaptureUrl { get; set; }

    public object Clone()
    {
        var result = MemberwiseClone() as StateMachineDefinition;
        return result;
    }
}
