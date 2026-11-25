using System;
using Newtonsoft.Json;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.StateMachineModule.Core.Models;

/// <summary>
/// An entity type state machines can be defined for, registered by the module that owns the entity
/// (see <see cref="Services.IStateMachineEntityTypeRegistrar"/>).
/// </summary>
public class StateMachineEntityType
{
    /// <summary>Full type name, as stored in <see cref="StateMachineDefinition.EntityType"/></summary>
    public string Id { get; set; }

    /// <summary>Display name, in English</summary>
    public string Name { get; set; }

    /// <summary>
    /// Creates the condition tree prototype offered when editing a transition condition; null when the type has no
    /// transition conditions. Not serialized: the admin UI loads the prototype by the type's id.
    /// </summary>
    [JsonIgnore]
    public Func<IConditionTree> ConditionTreePrototypeFactory { get; set; }

    /// <summary>Whether transitions of this type's state machines can have conditions</summary>
    public bool HasConditionTree => ConditionTreePrototypeFactory != null;
}
