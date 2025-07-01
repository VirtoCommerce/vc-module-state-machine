using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StateMachineModule.Core.Models;
public class StateMachineAttribute : AuditableEntity, ICloneable
{
    public string DefinitionId { get; set; }
    public string Item { get; set; }
    public string AttributeKey { get; set; }
    public string Value { get; set; }

    public object Clone()
    {
        var result = MemberwiseClone() as StateMachineDefinition;
        return result;
    }
}
