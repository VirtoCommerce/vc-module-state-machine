using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StateMachineModule.Core.Models;
public class StateMachineLocalization : AuditableEntity, ICloneable
{
    public string DefinitionId { get; set; }
    public string Item { get; set; }
    public string Locale { get; set; }
    public string Value { get; set; }

    public object Clone()
    {
        var result = MemberwiseClone() as StateMachineDefinition;
        return result;
    }
}
